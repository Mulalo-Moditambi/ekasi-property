using Application.Abstractions.Authentication;
using Application.Properties.ReorderImages;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class ReorderPropertyImagesCommandHandlerTests : BaseHandlerTest
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

        var command = new ReorderPropertyImagesCommand { PropertyId = property.Id, ImageIds = [Guid.NewGuid()] };
        var handler = new ReorderPropertyImagesCommandHandler(context, UserContext(), CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotFound(property.Id));
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalidImageOrder_WhenProvidedSetDoesNotMatchExistingImages()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);
        context.PropertyImages.Add(PropertyImage.Create(property.Id, "/uploads/a.jpg", 0, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var command = new ReorderPropertyImagesCommand { PropertyId = property.Id, ImageIds = [Guid.NewGuid()] };
        var handler = new ReorderPropertyImagesCommandHandler(context, UserContext(), CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.InvalidImageOrder(property.Id));
    }

    [Fact]
    public async Task Handle_Should_RewriteSortOrder_ToMatchRequestedSequence()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);
        var first = PropertyImage.Create(property.Id, "/uploads/a.jpg", 0, DateTime.UtcNow);
        var second = PropertyImage.Create(property.Id, "/uploads/b.jpg", 1, DateTime.UtcNow);
        context.PropertyImages.AddRange(first, second);
        await context.SaveChangesAsync();

        var command = new ReorderPropertyImagesCommand
        {
            PropertyId = property.Id,
            ImageIds = [second.Id, first.Id]
        };
        var handler = new ReorderPropertyImagesCommandHandler(context, UserContext(), CreateCache());

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        List<PropertyImage> images = await context.PropertyImages
            .Where(i => i.PropertyId == property.Id)
            .OrderBy(i => i.SortOrder)
            .ToListAsync();
        images[0].Id.ShouldBe(second.Id);
        images[1].Id.ShouldBe(first.Id);
    }
}
