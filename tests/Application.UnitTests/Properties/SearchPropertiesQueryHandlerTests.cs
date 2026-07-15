using Application.Properties.Search;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class SearchPropertiesQueryHandlerTests : BaseHandlerTest
{
    private static SearchPropertiesQuery EmptyQuery => new(null, null, null, null, null, null);

    [Fact]
    public async Task Handle_Should_ReturnOnlyListedProperties()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property listed = PropertyTestData.CreateProperty(Guid.NewGuid());
        Property rented = PropertyTestData.CreateProperty(Guid.NewGuid(), ListingType.Rent, PropertyStatus.Rented);
        Property withdrawn = PropertyTestData.CreateProperty(Guid.NewGuid(), ListingType.Rent, PropertyStatus.Withdrawn);
        context.Properties.AddRange(listed, rented, withdrawn);
        await context.SaveChangesAsync();

        var handler = new SearchPropertiesQueryHandler(context);

        // Act
        Result<SearchPropertiesResponse> result = await handler.Handle(EmptyQuery, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].Id.ShouldBe(listed.Id);
        result.Value.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_Should_FilterByTownship()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property inOrlando = PropertyTestData.CreateProperty(Guid.NewGuid());
        Property elsewhere = PropertyTestData.CreateProperty(Guid.NewGuid());
        elsewhere.Address = new Address("55 Main Road", "Khayelitsha", "Cape Town", "Western Cape", "7784");
        context.Properties.AddRange(inOrlando, elsewhere);
        await context.SaveChangesAsync();

        var query = new SearchPropertiesQuery("Orlando", null, null, null, null, null);
        var handler = new SearchPropertiesQueryHandler(context);

        // Act
        Result<SearchPropertiesResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].Id.ShouldBe(inOrlando.Id);
    }

    [Fact]
    public async Task Handle_Should_FilterByListingTypeAndPriceRange()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property cheapRental = PropertyTestData.CreateProperty(Guid.NewGuid(), ListingType.Rent);
        cheapRental.Price = 1500m;
        Property expensiveRental = PropertyTestData.CreateProperty(Guid.NewGuid(), ListingType.Rent);
        expensiveRental.Price = 6000m;
        Property saleHouse = PropertyTestData.CreateProperty(Guid.NewGuid(), ListingType.Sale);
        saleHouse.Price = 450000m;
        context.Properties.AddRange(cheapRental, expensiveRental, saleHouse);
        await context.SaveChangesAsync();

        var query = new SearchPropertiesQuery(null, ListingType.Rent, null, 1000m, 2000m, null);
        var handler = new SearchPropertiesQueryHandler(context);

        // Act
        Result<SearchPropertiesResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].Id.ShouldBe(cheapRental.Id);
    }

    [Fact]
    public async Task Handle_Should_FilterByMinBedrooms()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property oneBedroom = PropertyTestData.CreateProperty(Guid.NewGuid());
        Property threeBedroom = PropertyTestData.CreateProperty(Guid.NewGuid());
        threeBedroom.Bedrooms = 3;
        context.Properties.AddRange(oneBedroom, threeBedroom);
        await context.SaveChangesAsync();

        var query = new SearchPropertiesQuery(null, null, null, null, null, 2);
        var handler = new SearchPropertiesQueryHandler(context);

        // Act
        Result<SearchPropertiesResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].Id.ShouldBe(threeBedroom.Id);
    }

    [Fact]
    public async Task Handle_Should_PageResults_NewestFirst()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        DateTime baseTime = DateTime.UtcNow;

        for (int i = 0; i < 5; i++)
        {
            Property property = PropertyTestData.CreateProperty(Guid.NewGuid());
            property.Title = $"Listing {i}";
            property.CreatedAt = baseTime.AddMinutes(i);
            context.Properties.Add(property);
        }

        await context.SaveChangesAsync();

        var query = new SearchPropertiesQuery(null, null, null, null, null, null, Page: 2, PageSize: 2);
        var handler = new SearchPropertiesQueryHandler(context);

        // Act
        Result<SearchPropertiesResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(5);
        result.Value.Page.ShouldBe(2);
        result.Value.HasNextPage.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.Items[0].Title.ShouldBe("Listing 2");
        result.Value.Items[1].Title.ShouldBe("Listing 1");
    }

    [Fact]
    public async Task Handle_Should_ClampInvalidPaging()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        context.Properties.Add(PropertyTestData.CreateProperty(Guid.NewGuid()));
        await context.SaveChangesAsync();

        var query = new SearchPropertiesQuery(null, null, null, null, null, null, Page: 0, PageSize: 500);
        var handler = new SearchPropertiesQueryHandler(context);

        // Act
        Result<SearchPropertiesResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Page.ShouldBe(1);
        result.Value.PageSize.ShouldBe(50);
        result.Value.Items.Count.ShouldBe(1);
    }
}
