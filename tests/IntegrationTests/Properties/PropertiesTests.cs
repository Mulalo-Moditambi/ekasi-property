using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests.Properties;

public sealed class PropertiesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private sealed record PropertyDto(
        Guid Id,
        Guid OwnerId,
        string Title,
        int ListingType,
        int PropertyType,
        decimal Price,
        string Township,
        int Status);

    private sealed record PropertySummaryDto(Guid Id, string Title, decimal Price, string Township);

    private sealed record PagedResultDto(
        List<PropertySummaryDto> Items,
        int Page,
        int PageSize,
        int TotalCount,
        bool HasNextPage);

    private static object CreateRequest(Guid ownerId, int listingType = 0) => new
    {
        ownerId,
        title = "Backroom to rent in Orlando West",
        description = "Tidy backroom with own entrance and prepaid electricity.",
        listingType,
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

    private async Task<Guid> CreatePropertyAsync(Guid ownerId, int listingType = 0)
    {
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("properties", CreateRequest(ownerId, listingType));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    [Fact]
    public async Task CreateProperty_Should_ReturnUnauthorized_WhenTokenIsMissing()
    {
        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("properties", CreateRequest(Guid.NewGuid()));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchProperties_Should_BePubliclyAccessible()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("properties");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateProperty_Should_PersistListing_ThatCanBeRetrievedPublicly()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        // Act
        Guid propertyId = await CreatePropertyAsync(userId);

        // Assert
        propertyId.ShouldNotBe(Guid.Empty);

        HttpClient.DefaultRequestHeaders.Authorization = null;

        HttpResponseMessage getResponse = await HttpClient.GetAsync($"properties/{propertyId}");
        getResponse.EnsureSuccessStatusCode();

        PropertyDto? property = await getResponse.Content.ReadFromJsonAsync<PropertyDto>();
        property!.Id.ShouldBe(propertyId);
        property.OwnerId.ShouldBe(userId);
        property.Title.ShouldBe("Backroom to rent in Orlando West");
        property.Township.ShouldBe("Orlando West");
        property.Status.ShouldBe(0);
    }

    [Fact]
    public async Task SearchProperties_Should_ReturnCreatedListing_WhenFiltersMatch()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        Guid propertyId = await CreatePropertyAsync(userId);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync(
            "properties?township=Orlando&listingType=0&minPrice=1000&maxPrice=2000");

        // Assert
        response.EnsureSuccessStatusCode();
        PagedResultDto? results = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        results!.Items.ShouldContain(p => p.Id == propertyId);
        results.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task MarkPropertyRented_Should_ChangeStatus_AndHideListingFromSearch()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        Guid propertyId = await CreatePropertyAsync(userId);

        // Act
        HttpResponseMessage rentResponse = await HttpClient.PutAsync($"properties/{propertyId}/mark-rented", null);

        // Assert
        rentResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getResponse = await HttpClient.GetAsync($"properties/{propertyId}");
        getResponse.EnsureSuccessStatusCode();
        PropertyDto? property = await getResponse.Content.ReadFromJsonAsync<PropertyDto>();
        property!.Status.ShouldBe(1);

        HttpResponseMessage searchResponse = await HttpClient.GetAsync("properties?township=Orlando");
        searchResponse.EnsureSuccessStatusCode();
        PagedResultDto? results = await searchResponse.Content.ReadFromJsonAsync<PagedResultDto>();
        results!.Items.ShouldNotContain(p => p.Id == propertyId);
    }

    [Fact]
    public async Task MarkPropertySold_Should_ReturnProblem_WhenListingIsForRent()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        Guid propertyId = await CreatePropertyAsync(userId, listingType: 0);

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync($"properties/{propertyId}/mark-sold", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RelistProperty_Should_MakeWithdrawnListingSearchableAgain()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        Guid propertyId = await CreatePropertyAsync(userId);

        HttpResponseMessage withdrawResponse = await HttpClient.PutAsync($"properties/{propertyId}/withdraw", null);
        withdrawResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act
        HttpResponseMessage relistResponse = await HttpClient.PutAsync($"properties/{propertyId}/relist", null);

        // Assert
        relistResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getResponse = await HttpClient.GetAsync($"properties/{propertyId}");
        getResponse.EnsureSuccessStatusCode();
        PropertyDto? property = await getResponse.Content.ReadFromJsonAsync<PropertyDto>();
        property!.Status.ShouldBe(0);
    }

    private sealed record ImageDto(Guid Id, string Url);

    private sealed record PropertyImagesDto(Guid Id, List<ImageDto> Images);

    [Fact]
    public async Task AddImages_Should_StoreImages_ThatAppearOnTheListingAndAreServed()
    {
        // Arrange
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        Guid propertyId = await CreatePropertyAsync(userId);

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "files", "photo.png");

        // Act
        HttpResponseMessage uploadResponse = await HttpClient.PostAsync($"properties/{propertyId}/images", form);

        // Assert
        uploadResponse.EnsureSuccessStatusCode();
        List<Guid>? imageIds = await uploadResponse.Content.ReadFromJsonAsync<List<Guid>>();
        imageIds!.Count.ShouldBe(1);

        HttpResponseMessage getResponse = await HttpClient.GetAsync($"properties/{propertyId}");
        getResponse.EnsureSuccessStatusCode();
        PropertyImagesDto? property = await getResponse.Content.ReadFromJsonAsync<PropertyImagesDto>();
        property!.Images.Count.ShouldBe(1);
        property.Images[0].Url.ShouldStartWith("/uploads/");

        HttpResponseMessage imageResponse = await HttpClient.GetAsync(property.Images[0].Url);
        imageResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddImages_Should_ReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        using var file = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "files", "photo.png");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"properties/{Guid.NewGuid()}/images", form);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteProperty_Should_ReturnNotFound_WhenPropertyBelongsToAnotherUser()
    {
        // Arrange
        (Guid ownerId, AccessTokens ownerTokens) = await RegisterAndLoginAsync();
        Authenticate(ownerTokens.AccessToken);
        Guid propertyId = await CreatePropertyAsync(ownerId);

        (_, AccessTokens otherTokens) = await RegisterAndLoginAsync();
        Authenticate(otherTokens.AccessToken);

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"properties/{propertyId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private sealed record MyPropertyDto(Guid Id, int Status);

    private sealed record MyPagedResultDto(List<MyPropertyDto> Items, int TotalCount);

    [Fact]
    public async Task GetMyProperties_Should_ReturnUnauthorized_WhenTokenIsMissing()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("properties/mine");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProperties_Should_ReturnOnlyTheCallersListings_IncludingNonListedStatuses()
    {
        // Arrange
        (Guid ownerId, AccessTokens ownerTokens) = await RegisterAndLoginAsync();
        Authenticate(ownerTokens.AccessToken);
        Guid propertyId = await CreatePropertyAsync(ownerId);
        (await HttpClient.PutAsync($"properties/{propertyId}/withdraw", null)).EnsureSuccessStatusCode();

        (Guid otherOwnerId, AccessTokens otherTokens) = await RegisterAndLoginAsync();
        Authenticate(otherTokens.AccessToken);
        Guid otherPropertyId = await CreatePropertyAsync(otherOwnerId);

        Authenticate(ownerTokens.AccessToken);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("properties/mine");

        // Assert
        response.EnsureSuccessStatusCode();
        MyPagedResultDto? results = await response.Content.ReadFromJsonAsync<MyPagedResultDto>();
        results!.Items.ShouldContain(p => p.Id == propertyId && p.Status == 3);
        results.Items.ShouldNotContain(p => p.Id == otherPropertyId);
    }

    [Fact]
    public async Task DeleteImage_Should_RemoveImage_FromTheListing()
    {
        // Arrange
        (Guid ownerId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        Guid propertyId = await CreatePropertyAsync(ownerId);

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "files", "photo.png");
        HttpResponseMessage uploadResponse = await HttpClient.PostAsync($"properties/{propertyId}/images", form);
        uploadResponse.EnsureSuccessStatusCode();
        List<Guid>? imageIds = await uploadResponse.Content.ReadFromJsonAsync<List<Guid>>();
        Guid imageId = imageIds![0];

        // Act
        HttpResponseMessage deleteResponse = await HttpClient.DeleteAsync($"properties/{propertyId}/images/{imageId}");

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getResponse = await HttpClient.GetAsync($"properties/{propertyId}");
        getResponse.EnsureSuccessStatusCode();
        PropertyImagesDto? property = await getResponse.Content.ReadFromJsonAsync<PropertyImagesDto>();
        property!.Images.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReorderImages_Should_UpdateDisplayOrder()
    {
        // Arrange
        (Guid ownerId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        Guid propertyId = await CreatePropertyAsync(ownerId);

        using var form = new MultipartFormDataContent();
        var first = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47]);
        first.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(first, "files", "first.png");
        var second = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47]);
        second.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(second, "files", "second.png");
        HttpResponseMessage uploadResponse = await HttpClient.PostAsync($"properties/{propertyId}/images", form);
        uploadResponse.EnsureSuccessStatusCode();
        List<Guid>? imageIds = await uploadResponse.Content.ReadFromJsonAsync<List<Guid>>();
        var reorderRequest = new { imageIds = new[] { imageIds![1], imageIds[0] } };

        // Act
        HttpResponseMessage reorderResponse = await HttpClient.PutAsJsonAsync($"properties/{propertyId}/images/order", reorderRequest);

        // Assert
        reorderResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getResponse = await HttpClient.GetAsync($"properties/{propertyId}");
        getResponse.EnsureSuccessStatusCode();
        PropertyImagesDto? property = await getResponse.Content.ReadFromJsonAsync<PropertyImagesDto>();
        property!.Images[0].Id.ShouldBe(imageIds[1]);
        property.Images[1].Id.ShouldBe(imageIds[0]);
    }
}
