using System.Net;
using System.Net.Http.Json;
using Infrastructure.Payments;

namespace IntegrationTests.Subscriptions;

public sealed class SubscriptionsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private const string Passphrase = "test-passphrase";

    private sealed record SubscriptionDto(
        Guid Id,
        int Status,
        DateTime TrialEndsAt,
        DateTime? GracePeriodEndsAt,
        DateTime? NextBillingDate);

    private async Task<SubscriptionDto> GetMySubscriptionAsync()
    {
        HttpResponseMessage response = await HttpClient.GetAsync("subscriptions/me");
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<SubscriptionDto>())!;
    }

    private async Task<Guid> CreateFirstListingAsync(Guid ownerId)
    {
        var request = new
        {
            ownerId,
            title = "Backroom to rent in Orlando West",
            description = "Tidy backroom with own entrance and prepaid electricity.",
            listingType = 0,
            propertyType = 0,
            price = 1800,
            street = "123 Vilakazi Street",
            township = "Orlando West",
            city = "Soweto",
            province = "Gauteng",
            postalCode = "1804",
            bedrooms = 1,
            bathrooms = 1,
            hasElectricity = true,
            waterIncluded = true,
            hasOwnEntrance = true,
            hasParking = false
        };

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("properties", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static (Dictionary<string, string> Fields, string Signature) BuildSignedItn(
        Guid subscriptionId, string pfPaymentId, string paymentStatus)
    {
        var fields = new Dictionary<string, string>
        {
            ["pf_payment_id"] = pfPaymentId,
            ["custom_str1"] = subscriptionId.ToString(),
            ["payment_status"] = paymentStatus,
            ["amount_gross"] = "25.00"
        };

        var signatureService = new PayFastSignatureService();
        string signature = signatureService.Sign(fields, Passphrase);

        return (fields, signature);
    }

    private static FormUrlEncodedContent BuildFormContent(Dictionary<string, string> fields, string signature)
    {
        var withSignature = new Dictionary<string, string>(fields) { ["signature"] = signature };

        return new FormUrlEncodedContent(withSignature);
    }

    [Fact]
    public async Task GetMine_Should_ReturnNotFound_BeforeFirstListingIsCreated()
    {
        // Arrange
        (_, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("subscriptions/me");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMine_Should_ReturnTrialingSubscription_AfterFirstListingIsCreated()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        // Act
        await CreateFirstListingAsync(userId);
        SubscriptionDto subscription = await GetMySubscriptionAsync();

        // Assert
        subscription.Status.ShouldBe(0);
        subscription.TrialEndsAt.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateSecondListing_Should_NotStartANewTrial()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        await CreateFirstListingAsync(userId);
        SubscriptionDto afterFirstListing = await GetMySubscriptionAsync();

        // Act
        await CreateFirstListingAsync(userId);
        SubscriptionDto afterSecondListing = await GetMySubscriptionAsync();

        // Assert
        afterSecondListing.Id.ShouldBe(afterFirstListing.Id);
        afterSecondListing.TrialEndsAt.ShouldBe(afterFirstListing.TrialEndsAt);
    }

    [Fact]
    public async Task PayFastItn_Should_ActivateSubscription_WhenPaymentComplete()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        await CreateFirstListingAsync(userId);
        SubscriptionDto subscription = await GetMySubscriptionAsync();

        (Dictionary<string, string> fields, string signature) = BuildSignedItn(
            subscription.Id, $"pf-{Guid.NewGuid():N}", "COMPLETE");

        // Act
        HttpClient.DefaultRequestHeaders.Authorization = null;
        using FormUrlEncodedContent content = BuildFormContent(fields, signature);
        HttpResponseMessage itnResponse = await HttpClient.PostAsync("subscriptions/payfast/itn", content);

        // Assert
        itnResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        Authenticate(tokens.AccessToken);
        SubscriptionDto updated = await GetMySubscriptionAsync();
        updated.Status.ShouldBe(1);
        updated.NextBillingDate.ShouldNotBeNull();
    }

    [Fact]
    public async Task PayFastItn_Should_BeIdempotent_WhenSamePaymentIdIsPostedTwice()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        await CreateFirstListingAsync(userId);
        SubscriptionDto subscription = await GetMySubscriptionAsync();

        string pfPaymentId = $"pf-{Guid.NewGuid():N}";
        (Dictionary<string, string> fields, string signature) = BuildSignedItn(
            subscription.Id, pfPaymentId, "COMPLETE");

        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        using FormUrlEncodedContent firstContent = BuildFormContent(fields, signature);
        HttpResponseMessage firstResponse = await HttpClient.PostAsync("subscriptions/payfast/itn", firstContent);
        using FormUrlEncodedContent secondContent = BuildFormContent(fields, signature);
        HttpResponseMessage secondResponse = await HttpClient.PostAsync("subscriptions/payfast/itn", secondContent);

        // Assert
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        Authenticate(tokens.AccessToken);
        SubscriptionDto updated = await GetMySubscriptionAsync();
        updated.Status.ShouldBe(1);
    }

    [Fact]
    public async Task PayFastItn_Should_ReturnBadRequest_WhenSignatureIsInvalid()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        await CreateFirstListingAsync(userId);
        SubscriptionDto subscription = await GetMySubscriptionAsync();

        (Dictionary<string, string> fields, _) = BuildSignedItn(subscription.Id, $"pf-{Guid.NewGuid():N}", "COMPLETE");

        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        using FormUrlEncodedContent content = BuildFormContent(fields, "not-a-real-signature");
        HttpResponseMessage response = await HttpClient.PostAsync("subscriptions/payfast/itn", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
