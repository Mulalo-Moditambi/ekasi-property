using Application.Abstractions.Authentication;
using Application.Abstractions.Storage;
using Application.Properties.AddImages;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class AddPropertyImagesCommandHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static AddPropertyImagesCommand Command(Guid propertyId, int fileCount = 1)
    {
        var command = new AddPropertyImagesCommand { PropertyId = propertyId };

        for (int i = 0; i < fileCount; i++)
        {
            command.Images.Add(new ImageUpload(new MemoryStream([1, 2, 3]), $"photo-{i}.jpg", "image/jpeg", 3));
        }

        return command;
    }

    private static IFileStorage CreateStorage()
    {
        IFileStorage storage = Substitute.For<IFileStorage>();
        storage.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => $"/uploads/{Guid.NewGuid():N}{callInfo.ArgAt<string>(1)}");

        return storage;
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
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        AddPropertyImagesCommand command = Command(property.Id);
        var handler = new AddPropertyImagesCommandHandler(
            context, CreateStorage(), dateTimeProvider, userContext, CreateCache());

        // Act
        Result<List<Guid>> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotFound(property.Id));
    }

    [Fact]
    public async Task Handle_Should_ReturnTooManyImages_WhenCapWouldBeExceeded()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(OwnerId);
        context.Properties.Add(property);

        for (int i = 0; i < 9; i++)
        {
            context.PropertyImages.Add(
                PropertyImage.Create(property.Id, $"/uploads/existing-{i}.jpg", i, DateTime.UtcNow));
        }

        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        AddPropertyImagesCommand command = Command(property.Id, fileCount: 2);
        var handler = new AddPropertyImagesCommandHandler(
            context, CreateStorage(), dateTimeProvider, userContext, CreateCache());

        // Act
        Result<List<Guid>> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.TooManyImages(property.Id, 10));
    }

    [Fact]
    public async Task Handle_Should_StoreImagesInOrderAndRaiseDomainEvents_WhenValid()
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
        IFileStorage storage = CreateStorage();

        AddPropertyImagesCommand command = Command(property.Id, fileCount: 2);
        var handler = new AddPropertyImagesCommandHandler(
            context, storage, dateTimeProvider, userContext, CreateCache());

        // Act
        Result<List<Guid>> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);

        List<PropertyImage> images = await context.PropertyImages
            .Where(i => i.PropertyId == property.Id)
            .OrderBy(i => i.SortOrder)
            .ToListAsync();
        images.Count.ShouldBe(2);
        images[0].SortOrder.ShouldBe(0);
        images[1].SortOrder.ShouldBe(1);
        images[0].Url.ShouldStartWith("/uploads/");
        images[0].DomainEvents.ShouldContain(domainEvent => domainEvent is PropertyImageAddedDomainEvent);

        await storage.Received(2).SaveAsync(Arg.Any<Stream>(), ".jpg", Arg.Any<CancellationToken>());
    }
}
