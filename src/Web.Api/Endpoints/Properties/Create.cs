using Application.Abstractions.Messaging;
using Application.Properties.Create;
using Domain.Properties;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public Guid OwnerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ListingType { get; set; }
        public int PropertyType { get; set; }
        public decimal Price { get; set; }
        public string Street { get; set; }
        public string Township { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public bool HasElectricity { get; set; }
        public bool WaterIncluded { get; set; }
        public bool HasOwnEntrance { get; set; }
        public bool HasParking { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("properties", async (
            Request request,
            ICommandHandler<CreatePropertyCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreatePropertyCommand
            {
                OwnerId = request.OwnerId,
                Title = request.Title,
                Description = request.Description,
                ListingType = (ListingType)request.ListingType,
                PropertyType = (PropertyType)request.PropertyType,
                Price = request.Price,
                Street = request.Street,
                Township = request.Township,
                City = request.City,
                Province = request.Province,
                PostalCode = request.PostalCode,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                HasElectricity = request.HasElectricity,
                WaterIncluded = request.WaterIncluded,
                HasOwnEntrance = request.HasOwnEntrance,
                HasParking = request.HasParking
            };

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Write)
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
