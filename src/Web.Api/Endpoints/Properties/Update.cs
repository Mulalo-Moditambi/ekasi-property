using Application.Abstractions.Messaging;
using Application.Properties.Update;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class Update : IEndpoint
{
    public sealed class Request
    {
        public string Title { get; set; }
        public string Description { get; set; }
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
        app.MapPut("properties/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdatePropertyCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePropertyCommand
            {
                PropertyId = id,
                Title = request.Title,
                Description = request.Description,
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

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
