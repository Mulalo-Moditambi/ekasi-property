using Application.Abstractions.Authentication;
using Application.Inquiries.GetMine;
using Application.UnitTests.Abstractions;
using Application.UnitTests.Properties;
using Domain.Inquiries;
using Domain.Properties;
using SharedKernel;

namespace Application.UnitTests.Inquiries;

public sealed class GetMyLeadsQueryHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static IUserContext UserContext()
    {
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);

        return userContext;
    }

    [Fact]
    public async Task Handle_Should_ReturnOnlyLeadsAcrossPropertiesOwnedByCurrentUser()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property mine = PropertyTestData.CreateProperty(OwnerId);
        Property someoneElses = PropertyTestData.CreateProperty(Guid.NewGuid());
        context.Properties.AddRange(mine, someoneElses);

        var myLead = Inquiry.Create(mine.Id, "Lerato", "lerato@example.com", null, "Still available?", DateTime.UtcNow);
        var otherLead = Inquiry.Create(someoneElses.Id, "Sipho", "sipho@example.com", null, "Interested", DateTime.UtcNow);
        context.Inquiries.AddRange(myLead, otherLead);
        await context.SaveChangesAsync();

        var handler = new GetMyLeadsQueryHandler(context, UserContext());

        // Act
        Result<GetMyLeadsResponse> result = await handler.Handle(new GetMyLeadsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].Id.ShouldBe(myLead.Id);
        result.Value.Items[0].PropertyTitle.ShouldBe(mine.Title);
    }

    [Fact]
    public async Task Handle_Should_FilterByPropertyId_WhenProvided()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property first = PropertyTestData.CreateProperty(OwnerId);
        Property second = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.AddRange(first, second);

        var leadOnFirst = Inquiry.Create(first.Id, "Lerato", "lerato@example.com", null, "Message", DateTime.UtcNow);
        var leadOnSecond = Inquiry.Create(second.Id, "Sipho", "sipho@example.com", null, "Message", DateTime.UtcNow);
        context.Inquiries.AddRange(leadOnFirst, leadOnSecond);
        await context.SaveChangesAsync();

        var query = new GetMyLeadsQuery(PropertyId: first.Id);
        var handler = new GetMyLeadsQueryHandler(context, UserContext());

        // Act
        Result<GetMyLeadsResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].Id.ShouldBe(leadOnFirst.Id);
    }

    [Fact]
    public async Task Handle_Should_FilterBySearchTerm_MatchingNameEmailOrMessage()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);

        var match = Inquiry.Create(property.Id, "Lerato Dlamini", "lerato@example.com", null, "Message", DateTime.UtcNow);
        var noMatch = Inquiry.Create(property.Id, "Sipho Ngcobo", "sipho@example.com", null, "Message", DateTime.UtcNow);
        context.Inquiries.AddRange(match, noMatch);
        await context.SaveChangesAsync();

        var query = new GetMyLeadsQuery(Search: "Lerato");
        var handler = new GetMyLeadsQueryHandler(context, UserContext());

        // Act
        Result<GetMyLeadsResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].Id.ShouldBe(match.Id);
    }
}
