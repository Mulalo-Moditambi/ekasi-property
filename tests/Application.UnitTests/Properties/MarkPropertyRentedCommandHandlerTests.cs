using Application.Abstractions.Authentication;
using Application.Properties.MarkRented;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class MarkPropertyRentedCommandHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenPropertyDoesNotExist()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        var command = new MarkPropertyRentedCommand(Guid.NewGuid());
        var handler = new MarkPropertyRentedCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotFound(command.PropertyId));
    }

    [Fact]
    public async Task Handle_Should_ReturnNotARentalListing_WhenPropertyIsForSale()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId, ListingType.Sale);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        var command = new MarkPropertyRentedCommand(property.Id);
        var handler = new MarkPropertyRentedCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotARentalListing(property.Id));
    }

    [Fact]
    public async Task Handle_Should_ReturnNotListed_WhenPropertyIsAlreadyRented()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId, ListingType.Rent, PropertyStatus.Rented);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        var command = new MarkPropertyRentedCommand(property.Id);
        var handler = new MarkPropertyRentedCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotListed(property.Id));
    }

    [Fact]
    public async Task Handle_Should_MarkPropertyRentedAndRaiseDomainEvent_WhenValid()
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

        var command = new MarkPropertyRentedCommand(property.Id);
        var handler = new MarkPropertyRentedCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Property rented = await context.Properties.SingleAsync(p => p.Id == property.Id);
        rented.Status.ShouldBe(PropertyStatus.Rented);
        rented.UpdatedAt.ShouldNotBeNull();
        rented.DomainEvents.ShouldContain(domainEvent => domainEvent is PropertyRentedDomainEvent);
    }
}
