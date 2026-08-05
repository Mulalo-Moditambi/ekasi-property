using Application.Properties.AddImages;
using Application.Properties.Create;
using Application.Properties.DeleteImage;
using Application.Properties.ReorderImages;
using Application.Properties.Update;
using Domain.Properties;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Properties;

public sealed class PropertyValidatorsTests
{
    private readonly CreatePropertyCommandValidator _createValidator = new();
    private readonly UpdatePropertyCommandValidator _updateValidator = new();
    private readonly AddPropertyImagesCommandValidator _addImagesValidator = new();
    private readonly DeletePropertyImageCommandValidator _deleteImageValidator = new();
    private readonly ReorderPropertyImagesCommandValidator _reorderImagesValidator = new();

    private static CreatePropertyCommand ValidCreateCommand => new()
    {
        OwnerId = Guid.NewGuid(),
        Title = "Neat backroom with own entrance",
        Description = "A tidy backroom in Soweto.",
        ListingType = ListingType.Rent,
        PropertyType = PropertyType.Backroom,
        Price = 1800m,
        Street = "123 Vilakazi Street",
        Township = "Orlando West",
        City = "Soweto",
        Province = "Gauteng",
        PostalCode = "1804",
        Bedrooms = 1,
        Bathrooms = 1
    };

    private static UpdatePropertyCommand ValidUpdateCommand => new()
    {
        PropertyId = Guid.NewGuid(),
        Title = "Updated title",
        Description = "Updated description.",
        Price = 2000m,
        Street = "456 Kumalo Street",
        Township = "Orlando East",
        City = "Soweto",
        Province = "Gauteng",
        PostalCode = "1804",
        Bedrooms = 1,
        Bathrooms = 1
    };

    [Fact]
    public void CreateValidator_Should_NotHaveErrors_WhenCommandIsValid()
    {
        TestValidationResult<CreatePropertyCommand> result = _createValidator.TestValidate(ValidCreateCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateValidator_Should_HaveError_WhenOwnerIdIsEmpty()
    {
        CreatePropertyCommand command = ValidCreateCommand;
        command.OwnerId = Guid.Empty;

        TestValidationResult<CreatePropertyCommand> result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.OwnerId);
    }

    [Fact]
    public void CreateValidator_Should_HaveError_WhenTitleIsEmpty()
    {
        CreatePropertyCommand command = ValidCreateCommand;
        command.Title = string.Empty;

        TestValidationResult<CreatePropertyCommand> result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Title);
    }

    [Fact]
    public void CreateValidator_Should_HaveError_WhenPriceIsZero()
    {
        CreatePropertyCommand command = ValidCreateCommand;
        command.Price = 0m;

        TestValidationResult<CreatePropertyCommand> result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public void CreateValidator_Should_HaveError_WhenListingTypeIsInvalid()
    {
        CreatePropertyCommand command = ValidCreateCommand;
        command.ListingType = (ListingType)99;

        TestValidationResult<CreatePropertyCommand> result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ListingType);
    }

    [Fact]
    public void CreateValidator_Should_HaveError_WhenTownshipIsEmpty()
    {
        CreatePropertyCommand command = ValidCreateCommand;
        command.Township = string.Empty;

        TestValidationResult<CreatePropertyCommand> result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Township);
    }

    [Fact]
    public void CreateValidator_Should_HaveError_WhenBedroomsIsNegative()
    {
        CreatePropertyCommand command = ValidCreateCommand;
        command.Bedrooms = -1;

        TestValidationResult<CreatePropertyCommand> result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Bedrooms);
    }

    [Fact]
    public void UpdateValidator_Should_NotHaveErrors_WhenCommandIsValid()
    {
        TestValidationResult<UpdatePropertyCommand> result = _updateValidator.TestValidate(ValidUpdateCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateValidator_Should_HaveError_WhenPropertyIdIsEmpty()
    {
        UpdatePropertyCommand command = ValidUpdateCommand;
        command.PropertyId = Guid.Empty;

        TestValidationResult<UpdatePropertyCommand> result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.PropertyId);
    }

    [Fact]
    public void UpdateValidator_Should_HaveError_WhenPriceIsZero()
    {
        UpdatePropertyCommand command = ValidUpdateCommand;
        command.Price = 0m;

        TestValidationResult<UpdatePropertyCommand> result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public void AddImagesValidator_Should_NotHaveErrors_WhenCommandIsValid()
    {
        var command = new AddPropertyImagesCommand
        {
            PropertyId = Guid.NewGuid(),
            Images = [new ImageUpload(Stream.Null, "photo.jpg", "image/jpeg", 1024)]
        };

        TestValidationResult<AddPropertyImagesCommand> result = _addImagesValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AddImagesValidator_Should_HaveError_WhenNoImagesProvided()
    {
        var command = new AddPropertyImagesCommand { PropertyId = Guid.NewGuid() };

        TestValidationResult<AddPropertyImagesCommand> result = _addImagesValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Images);
    }

    [Fact]
    public void AddImagesValidator_Should_HaveError_WhenContentTypeIsNotAnImage()
    {
        var command = new AddPropertyImagesCommand
        {
            PropertyId = Guid.NewGuid(),
            Images = [new ImageUpload(Stream.Null, "malware.exe", "application/octet-stream", 1024)]
        };

        TestValidationResult<AddPropertyImagesCommand> result = _addImagesValidator.TestValidate(command);

        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddImagesValidator_Should_HaveError_WhenFileIsTooLarge()
    {
        var command = new AddPropertyImagesCommand
        {
            PropertyId = Guid.NewGuid(),
            Images = [new ImageUpload(Stream.Null, "huge.jpg", "image/jpeg", 6 * 1024 * 1024)]
        };

        TestValidationResult<AddPropertyImagesCommand> result = _addImagesValidator.TestValidate(command);

        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void DeleteImageValidator_Should_NotHaveErrors_WhenCommandIsValid()
    {
        var command = new DeletePropertyImageCommand(Guid.NewGuid(), Guid.NewGuid());

        TestValidationResult<DeletePropertyImageCommand> result = _deleteImageValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DeleteImageValidator_Should_HaveError_WhenImageIdIsEmpty()
    {
        var command = new DeletePropertyImageCommand(Guid.NewGuid(), Guid.Empty);

        TestValidationResult<DeletePropertyImageCommand> result = _deleteImageValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ImageId);
    }

    [Fact]
    public void ReorderImagesValidator_Should_NotHaveErrors_WhenCommandIsValid()
    {
        var command = new ReorderPropertyImagesCommand { PropertyId = Guid.NewGuid(), ImageIds = [Guid.NewGuid()] };

        TestValidationResult<ReorderPropertyImagesCommand> result = _reorderImagesValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ReorderImagesValidator_Should_HaveError_WhenImageIdsIsEmpty()
    {
        var command = new ReorderPropertyImagesCommand { PropertyId = Guid.NewGuid(), ImageIds = [] };

        TestValidationResult<ReorderPropertyImagesCommand> result = _reorderImagesValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ImageIds);
    }
}
