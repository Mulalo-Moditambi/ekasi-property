namespace Application.Properties;

internal static class PropertyCacheKeys
{
    internal static string ById(Guid propertyId) => $"properties-{propertyId}";
}
