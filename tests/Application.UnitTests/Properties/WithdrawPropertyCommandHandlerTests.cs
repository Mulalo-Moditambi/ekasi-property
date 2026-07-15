using Application.Abstractions.Authentication;
using Application.Properties.Withdraw;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class WithdrawPropertyCommandHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_ReturnNotListed_WhenPropertyIsAlreadyWithdrawn()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId, ListingType.Rent, PropertyStatus.Withdrawn);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        var command = new WithdrawPropertyCommand(property.Id);
        var handler = new WithdrawPropertyCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotListed(property.Id));
    }

    [Fact]
    public async Task Handle_Should_WithdrawPropertyAndRaiseDomainEvent_WhenValid()
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

        var command = new WithdrawPropertyCommand(property.Id);
        var handler = new WithdrawPropertyCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Property withdrawn = await context.Properties.SingleAsync(p => p.Id == property.Id);
        withdrawn.Status.ShouldBe(PropertyStatus.Withdrawn);
        withdrawn.DomainEvents.ShouldContain(domainEvent => domainEvent is PropertyWithdrawnDomainEvent);
    }
}
