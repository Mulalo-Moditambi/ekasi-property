using Application.Abstractions.Authentication;
using Application.Inquiries.GetForProperty;
using Application.UnitTests.Abstractions;
using Application.UnitTests.Properties;
using Domain.Inquiries;
using Domain.Properties;
using SharedKernel;

namespace Application.UnitTests.Inquiries;

public sealed class GetPropertyInquiriesQueryHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

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

        var query = new GetPropertyInquiriesQuery(property.Id);
        var handler = new GetPropertyInquiriesQueryHandler(context, userContext);

        // Act
        Result<List<InquiryResponse>> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotFound(property.Id));
    }

    [Fact]
    public async Task Handle_Should_ReturnInquiriesNewestFirst_WhenOwnerRequestsThem()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);

        DateTime baseTime = DateTime.UtcNow;
        var older = Inquiry.Create(property.Id, "Lerato", "lerato@example.com", null, "Still available?", baseTime);
        var newer = Inquiry.Create(property.Id, "Sipho", "sipho@example.com", "0731112222", "Can I view?", baseTime.AddMinutes(5));
        context.Inquiries.AddRange(older, newer);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);

        var query = new GetPropertyInquiriesQuery(property.Id);
        var handler = new GetPropertyInquiriesQueryHandler(context, userContext);

        // Act
        Result<List<InquiryResponse>> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value[0].Name.ShouldBe("Sipho");
        result.Value[1].Name.ShouldBe("Lerato");
    }
}
