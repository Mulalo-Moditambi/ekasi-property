using Application.Inquiries.Submit;
using Application.UnitTests.Abstractions;
using Application.UnitTests.Properties;
using Domain.Inquiries;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Inquiries;

public sealed class SubmitInquiryCommandHandlerTests : BaseHandlerTest
{
    private static SubmitInquiryCommand Command(Guid propertyId) => new()
    {
        PropertyId = propertyId,
        Name = "Thabo Mokoena",
        Email = "thabo@example.com",
        Phone = "0721234567",
        Message = "Is the backroom still available? I can view this weekend."
    };

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenPropertyDoesNotExist()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        SubmitInquiryCommand command = Command(Guid.NewGuid());
        var handler = new SubmitInquiryCommandHandler(context, dateTimeProvider);

        // Act
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotFound(command.PropertyId));
    }

    [Fact]
    public async Task Handle_Should_ReturnNotListed_WhenPropertyIsNotListed()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(Guid.NewGuid(), ListingType.Rent, PropertyStatus.Rented);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        SubmitInquiryCommand command = Command(property.Id);
        var handler = new SubmitInquiryCommandHandler(context, dateTimeProvider);

        // Act
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotListed(property.Id));
    }

    [Fact]
    public async Task Handle_Should_PersistInquiryAndRaiseDomainEvent_WhenValid()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(Guid.NewGuid());
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        SubmitInquiryCommand command = Command(property.Id);
        var handler = new SubmitInquiryCommandHandler(context, dateTimeProvider);

        // Act
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Inquiry inquiry = await context.Inquiries.SingleAsync(i => i.Id == result.Value);
        inquiry.PropertyId.ShouldBe(property.Id);
        inquiry.Name.ShouldBe("Thabo Mokoena");
        inquiry.Email.ShouldBe("thabo@example.com");
        inquiry.DomainEvents.ShouldContain(domainEvent => domainEvent is InquirySubmittedDomainEvent);
    }
}
