using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests.Inquiries;

public sealed class InquiriesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private sealed record InquiryDto(Guid Id, Guid PropertyId, string Name, string Email, string? Phone, string Message);

    private static object CreateListingRequest(Guid ownerId) => new
    {
        ownerId,
        title = "Backroom to rent in Zola",
        description = "Tidy backroom with own entrance.",
        listingType = 0,
        propertyType = 0,
        price = 1500,
        street = "10 Zola Street",
        township = "Zola",
        city = "Soweto",
        province = "Gauteng",
        postalCode = "1868",
        bedrooms = 1,
        bathrooms = 1,
        hasElectricity = true,
        waterIncluded = true,
        hasOwnEntrance = true,
        hasParking = false
    };

    private static object InquiryRequest() => new
    {
        name = "Thabo Mokoena",
        email = "thabo@example.com",
        phone = "0721234567",
        message = "Is this still available? I would like to view it this weekend."
    };

    [Fact]
    public async Task SubmitInquiry_Should_ReturnNotFound_WhenPropertyDoesNotExist()
    {
        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            $"properties/{Guid.NewGuid()}/inquiries", InquiryRequest());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetInquiries_Should_ReturnUnauthorized_WhenTokenIsMissing()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"properties/{Guid.NewGuid()}/inquiries");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitInquiry_Should_BePublic_AndVisibleToTheOwner()
    {
        // Arrange: owner creates a listing.
        (Guid ownerId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        HttpResponseMessage createResponse = await HttpClient.PostAsJsonAsync("properties", CreateListingRequest(ownerId));
        createResponse.EnsureSuccessStatusCode();
        Guid propertyId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act: an anonymous visitor submits an inquiry.
        HttpClient.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage submitResponse = await HttpClient.PostAsJsonAsync(
            $"properties/{propertyId}/inquiries", InquiryRequest());

        // Assert
        submitResponse.EnsureSuccessStatusCode();
        Guid inquiryId = await submitResponse.Content.ReadFromJsonAsync<Guid>();
        inquiryId.ShouldNotBe(Guid.Empty);

        // The owner can read it.
        Authenticate(tokens.AccessToken);
        HttpResponseMessage getResponse = await HttpClient.GetAsync($"properties/{propertyId}/inquiries");
        getResponse.EnsureSuccessStatusCode();

        List<InquiryDto>? inquiries = await getResponse.Content.ReadFromJsonAsync<List<InquiryDto>>();
        inquiries!.ShouldContain(i => i.Id == inquiryId && i.Name == "Thabo Mokoena");
    }

    [Fact]
    public async Task GetInquiries_Should_ReturnNotFound_WhenRequesterIsNotTheOwner()
    {
        // Arrange: owner creates a listing and a visitor inquires.
        (Guid ownerId, AccessTokens ownerTokens) = await RegisterAndLoginAsync();
        Authenticate(ownerTokens.AccessToken);

        HttpResponseMessage createResponse = await HttpClient.PostAsJsonAsync("properties", CreateListingRequest(ownerId));
        createResponse.EnsureSuccessStatusCode();
        Guid propertyId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act: a different authenticated user tries to read the inquiries.
        (_, AccessTokens otherTokens) = await RegisterAndLoginAsync();
        Authenticate(otherTokens.AccessToken);
        HttpResponseMessage response = await HttpClient.GetAsync($"properties/{propertyId}/inquiries");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
