using Application.Abstractions.Authentication;
using Application.Properties.MarkSold;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class MarkPropertySoldCommandHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_ReturnNotASaleListing_WhenPropertyIsForRent()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId, ListingType.Rent);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        var command = new MarkPropertySoldCommand(property.Id);
        var handler = new MarkPropertySoldCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotASaleListing(property.Id));
    }

    [Fact]
    public async Task Handle_Should_MarkPropertySoldAndRaiseDomainEvent_WhenValid()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId, ListingType.Sale);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        var command = new MarkPropertySoldCommand(property.Id);
        var handler = new MarkPropertySoldCommandHandler(context, dateTimeProvider, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Property sold = await context.Properties.SingleAsync(p => p.Id == property.Id);
        sold.Status.ShouldBe(PropertyStatus.Sold);
        sold.DomainEvents.ShouldContain(domainEvent => domainEvent is PropertySoldDomainEvent);
    }
}
