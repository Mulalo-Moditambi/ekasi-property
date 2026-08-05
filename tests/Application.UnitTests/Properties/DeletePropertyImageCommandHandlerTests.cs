using Application.Abstractions.Authentication;
using Application.Abstractions.Storage;
using Application.Properties.DeleteImage;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class DeletePropertyImageCommandHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static IUserContext UserContext()
    {
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);

        return userContext;
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenPropertyBelongsToAnotherUser()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(Guid.NewGuid());
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        var command = new DeletePropertyImageCommand(property.Id, Guid.NewGuid());
        var handler = new DeletePropertyImageCommandHandler(
            context, Substitute.For<IFileStorage>(), UserContext(), CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotFound(property.Id));
    }

    [Fact]
    public async Task Handle_Should_ReturnImageNotFound_WhenImageDoesNotBelongToProperty()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        var missingImageId = Guid.NewGuid();
        var command = new DeletePropertyImageCommand(property.Id, missingImageId);
        var handler = new DeletePropertyImageCommandHandler(
            context, Substitute.For<IFileStorage>(), UserContext(), CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.ImageNotFound(property.Id, missingImageId));
    }

    [Fact]
    public async Task Handle_Should_RemoveImageAndDeleteFromStorage_WhenValid()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);
        var image = PropertyImage.Create(property.Id, "/uploads/photo.jpg", 0, DateTime.UtcNow);
        context.PropertyImages.Add(image);
        await context.SaveChangesAsync();

        IFileStorage storage = Substitute.For<IFileStorage>();
        var command = new DeletePropertyImageCommand(property.Id, image.Id);
        var handler = new DeletePropertyImageCommandHandler(context, storage, UserContext(), CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        (await context.PropertyImages.AnyAsync(i => i.Id == image.Id)).ShouldBeFalse();
        await storage.Received(1).DeleteAsync("/uploads/photo.jpg", Arg.Any<CancellationToken>());
    }
}
