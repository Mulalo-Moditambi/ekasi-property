using Application.Abstractions.Authentication;
using Application.Properties.GetMyProperties;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class GetMyPropertiesQueryHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static IUserContext UserContext()
    {
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);

        return userContext;
    }

    [Fact]
    public async Task Handle_Should_ReturnOnlyPropertiesOwnedByCurrentUser_RegardlessOfStatus()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property mine = PropertyTestData.CreateProperty(OwnerId, ListingType.Rent, PropertyStatus.Withdrawn);
        Property someoneElses = PropertyTestData.CreateProperty(Guid.NewGuid());
        context.Properties.AddRange(mine, someoneElses);
        await context.SaveChangesAsync();

        var handler = new GetMyPropertiesQueryHandler(context, UserContext());

        // Act
        Result<GetMyPropertiesResponse> result = await handler.Handle(new GetMyPropertiesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].Id.ShouldBe(mine.Id);
        result.Value.Items[0].Status.ShouldBe(PropertyStatus.Withdrawn);
    }

    [Fact]
    public async Task Handle_Should_FilterByStatus_WhenProvided()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property listed = PropertyTestData.CreateProperty(OwnerId);
        Property sold = PropertyTestData.CreateProperty(OwnerId, ListingType.Sale, PropertyStatus.Sold);
        context.Properties.AddRange(listed, sold);
        await context.SaveChangesAsync();

        var query = new GetMyPropertiesQuery(Status: PropertyStatus.Sold);
        var handler = new GetMyPropertiesQueryHandler(context, UserContext());

        // Act
        Result<GetMyPropertiesResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].Id.ShouldBe(sold.Id);
    }

    [Fact]
    public async Task Handle_Should_IncludeCoverImageUrl_WhenImagesExist()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);
        context.PropertyImages.Add(PropertyImage.Create(property.Id, "/uploads/cover.jpg", 0, DateTime.UtcNow));
        context.PropertyImages.Add(PropertyImage.Create(property.Id, "/uploads/second.jpg", 1, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var handler = new GetMyPropertiesQueryHandler(context, UserContext());

        // Act
        Result<GetMyPropertiesResponse> result = await handler.Handle(new GetMyPropertiesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items[0].CoverImageUrl.ShouldBe("/uploads/cover.jpg");
    }
}
