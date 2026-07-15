using Application.Abstractions.Authentication;
using Application.Properties.Update;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class UpdatePropertyCommandHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static UpdatePropertyCommand Command(Guid propertyId) => new()
    {
        PropertyId = propertyId,
        Title = "Updated backroom title",
        Description = "Updated description with more details.",
        Price = 2000m,
        Street = "456 Kumalo Street",
        Township = "Orlando East",
        City = "Soweto",
        Province = "Gauteng",
        PostalCode = "1804",
        Bedrooms = 2,
        Bathrooms = 1,
        HasElectricity = true,
        WaterIncluded = false,
        HasOwnEntrance = true,
        HasParking = true
    };

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenPropertyDoesNotExist()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        UpdatePropertyCommand command = Command(Guid.NewGuid());
        var handler = new UpdatePropertyCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotFound(command.PropertyId));
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenPropertyBelongsToAnotherUser()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(Guid.NewGuid());
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        UpdatePropertyCommand command = Command(property.Id);
        var handler = new UpdatePropertyCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotFound(property.Id));
    }

    [Fact]
    public async Task Handle_Should_ReturnAlreadySold_WhenPropertyIsSold()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId, ListingType.Sale, PropertyStatus.Sold);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        UpdatePropertyCommand command = Command(property.Id);
        var handler = new UpdatePropertyCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.AlreadySold(property.Id));
    }

    [Fact]
    public async Task Handle_Should_UpdatePropertyAndRaiseDomainEvent_WhenValid()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        UpdatePropertyCommand command = Command(property.Id);
        var handler = new UpdatePropertyCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Property updated = await context.Properties.SingleAsync(p => p.Id == property.Id);
        updated.Title.ShouldBe("Updated backroom title");
        updated.Price.ShouldBe(2000m);
        updated.Address.Township.ShouldBe("Orlando East");
        updated.UpdatedAt.ShouldNotBeNull();
        updated.DomainEvents.ShouldContain(domainEvent => domainEvent is PropertyUpdatedDomainEvent);
    }
}
