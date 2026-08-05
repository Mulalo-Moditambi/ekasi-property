using SharedKernel;

namespace Domain.Properties;

public static class PropertyErrors
{
    public static Error NotFound(Guid propertyId) => Error.NotFound(
        "Properties.NotFound",
        $"The property with the Id = '{propertyId}' was not found");

    public static Error NotListed(Guid propertyId) => Error.Problem(
        "Properties.NotListed",
        $"The property with the Id = '{propertyId}' is no longer listed");

    public static Error AlreadyListed(Guid propertyId) => Error.Problem(
        "Properties.AlreadyListed",
        $"The property with the Id = '{propertyId}' is already listed");

    public static Error AlreadySold(Guid propertyId) => Error.Problem(
        "Properties.AlreadySold",
        $"The property with the Id = '{propertyId}' has already been sold");

    public static Error NotARentalListing(Guid propertyId) => Error.Problem(
        "Properties.NotARentalListing",
        $"The property with the Id = '{propertyId}' is not listed for rent");

    public static Error NotASaleListing(Guid propertyId) => Error.Problem(
        "Properties.NotASaleListing",
        $"The property with the Id = '{propertyId}' is not listed for sale");

    public static Error TooManyImages(Guid propertyId, int maxImages) => Error.Problem(
        "Properties.TooManyImages",
        $"The property with the Id = '{propertyId}' cannot have more than {maxImages} images");

    public static Error ImageNotFound(Guid propertyId, Guid imageId) => Error.NotFound(
        "Properties.ImageNotFound",
        $"The image with the Id = '{imageId}' was not found on property '{propertyId}'");

    public static Error InvalidImageOrder(Guid propertyId) => Error.Problem(
        "Properties.InvalidImageOrder",
        $"The provided image order does not match the images on property '{propertyId}'");
}
