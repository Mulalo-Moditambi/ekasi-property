using Application.Abstractions.Authentication;
using Application.Properties.Delete;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class DeletePropertyCommandHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenPropertyDoesNotExist()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);

        var command = new DeletePropertyCommand(Guid.NewGuid());
        var handler = new DeletePropertyCommandHandler(context, userContext, CreateCache());

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

        var command = new DeletePropertyCommand(property.Id);
        var handler = new DeletePropertyCommandHandler(context, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotFound(property.Id));
    }

    [Fact]
    public async Task Handle_Should_DeleteProperty_WhenValid()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);

        var command = new DeletePropertyCommand(property.Id);
        var handler = new DeletePropertyCommandHandler(context, userContext, CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        bool exists = await context.Properties.AnyAsync(p => p.Id == property.Id);
        exists.ShouldBeFalse();
    }
}
