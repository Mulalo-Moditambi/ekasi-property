using Application.Abstractions.Authentication;
using Application.Properties.Relist;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class RelistPropertyCommandHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

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

        var command = new RelistPropertyCommand(property.Id);
        var handler = new RelistPropertyCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.AlreadySold(property.Id));
    }

    [Fact]
    public async Task Handle_Should_ReturnAlreadyListed_WhenPropertyIsListed()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        var command = new RelistPropertyCommand(property.Id);
        var handler = new RelistPropertyCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.AlreadyListed(property.Id));
    }

    [Fact]
    public async Task Handle_Should_RelistPropertyAndRaiseDomainEvent_WhenPropertyIsWithdrawn()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId, ListingType.Rent, PropertyStatus.Withdrawn);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        var command = new RelistPropertyCommand(property.Id);
        var handler = new RelistPropertyCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Property relisted = await context.Properties.SingleAsync(p => p.Id == property.Id);
        relisted.Status.ShouldBe(PropertyStatus.Listed);
        relisted.DomainEvents.ShouldContain(domainEvent => domainEvent is PropertyRelistedDomainEvent);
    }
}
