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

    private sealed record LeadDto(Guid Id, Guid PropertyId, string PropertyTitle, string Name);

    private sealed record LeadsPagedResultDto(List<LeadDto> Items, int TotalCount);

    [Fact]
    public async Task GetMyLeads_Should_ReturnUnauthorized_WhenTokenIsMissing()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("inquiries/mine");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyLeads_Should_AggregateInquiriesAcrossAllOfTheOwnersProperties()
    {
        // Arrange: owner creates two listings.
        (Guid ownerId, AccessTokens ownerTokens) = await RegisterAndLoginAsync();
        Authenticate(ownerTokens.AccessToken);

        HttpResponseMessage firstCreate = await HttpClient.PostAsJsonAsync("properties", CreateListingRequest(ownerId));
        firstCreate.EnsureSuccessStatusCode();
        Guid firstPropertyId = await firstCreate.Content.ReadFromJsonAsync<Guid>();

        HttpResponseMessage secondCreate = await HttpClient.PostAsJsonAsync("properties", CreateListingRequest(ownerId));
        secondCreate.EnsureSuccessStatusCode();
        Guid secondPropertyId = await secondCreate.Content.ReadFromJsonAsync<Guid>();

        // A different owner's listing should never appear in these leads.
        (Guid otherOwnerId, AccessTokens otherTokens) = await RegisterAndLoginAsync();
        Authenticate(otherTokens.AccessToken);
        HttpResponseMessage otherCreate = await HttpClient.PostAsJsonAsync("properties", CreateListingRequest(otherOwnerId));
        otherCreate.EnsureSuccessStatusCode();
        Guid otherPropertyId = await otherCreate.Content.ReadFromJsonAsync<Guid>();

        // Act: visitors inquire on all three listings.
        HttpClient.DefaultRequestHeaders.Authorization = null;
        (await HttpClient.PostAsJsonAsync($"properties/{firstPropertyId}/inquiries", InquiryRequest())).EnsureSuccessStatusCode();
        (await HttpClient.PostAsJsonAsync($"properties/{secondPropertyId}/inquiries", InquiryRequest())).EnsureSuccessStatusCode();
        (await HttpClient.PostAsJsonAsync($"properties/{otherPropertyId}/inquiries", InquiryRequest())).EnsureSuccessStatusCode();

        Authenticate(ownerTokens.AccessToken);
        HttpResponseMessage response = await HttpClient.GetAsync("inquiries/mine");

        // Assert
        response.EnsureSuccessStatusCode();
        LeadsPagedResultDto? leads = await response.Content.ReadFromJsonAsync<LeadsPagedResultDto>();
        leads!.Items.ShouldContain(l => l.PropertyId == firstPropertyId);
        leads.Items.ShouldContain(l => l.PropertyId == secondPropertyId);
        leads.Items.ShouldNotContain(l => l.PropertyId == otherPropertyId);
    }

    [Fact]
    public async Task GetMyLeads_Should_FilterByPropertyId()
    {
        // Arrange
        (Guid ownerId, AccessTokens ownerTokens) = await RegisterAndLoginAsync();
        Authenticate(ownerTokens.AccessToken);

        HttpResponseMessage firstCreate = await HttpClient.PostAsJsonAsync("properties", CreateListingRequest(ownerId));
        firstCreate.EnsureSuccessStatusCode();
        Guid firstPropertyId = await firstCreate.Content.ReadFromJsonAsync<Guid>();

        HttpResponseMessage secondCreate = await HttpClient.PostAsJsonAsync("properties", CreateListingRequest(ownerId));
        secondCreate.EnsureSuccessStatusCode();
        Guid secondPropertyId = await secondCreate.Content.ReadFromJsonAsync<Guid>();

        HttpClient.DefaultRequestHeaders.Authorization = null;
        (await HttpClient.PostAsJsonAsync($"properties/{firstPropertyId}/inquiries", InquiryRequest())).EnsureSuccessStatusCode();
        (await HttpClient.PostAsJsonAsync($"properties/{secondPropertyId}/inquiries", InquiryRequest())).EnsureSuccessStatusCode();

        // Act
        Authenticate(ownerTokens.AccessToken);
        HttpResponseMessage response = await HttpClient.GetAsync($"inquiries/mine?propertyId={firstPropertyId}");

        // Assert
        response.EnsureSuccessStatusCode();
        LeadsPagedResultDto? leads = await response.Content.ReadFromJsonAsync<LeadsPagedResultDto>();
        leads!.Items.ShouldAllBe(l => l.PropertyId == firstPropertyId);
        leads.Items.ShouldNotBeEmpty();
    }
}
