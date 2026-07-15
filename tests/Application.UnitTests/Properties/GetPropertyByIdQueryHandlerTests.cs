using Application.Properties.GetById;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class GetPropertyByIdQueryHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenPropertyDoesNotExist()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();

        var query = new GetPropertyByIdQuery(Guid.NewGuid());
        var handler = new GetPropertyByIdQueryHandler(context, CreateCache());

        // Act
        Result<PropertyResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PropertyErrors.NotFound(query.PropertyId));
    }

    [Fact]
    public async Task Handle_Should_ReturnProperty_WhenPropertyExists()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        Property property = PropertyTestData.CreateProperty(Guid.NewGuid());
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        var query = new GetPropertyByIdQuery(property.Id);
        var handler = new GetPropertyByIdQueryHandler(context, CreateCache());

        // Act
        Result<PropertyResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(property.Id);
        result.Value.Title.ShouldBe(property.Title);
        result.Value.Township.ShouldBe("Orlando West");
        result.Value.Status.ShouldBe(PropertyStatus.Listed);
    }
}
