namespace SimpleCheckout.API;

using SimpleCheckout.Application;

public static class CheckoutEndpoints
{
    public static void MapCheckoutEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/checkout").WithOpenApi().RequireAuthorization();

        group.MapPost("/", async (CheckoutRequest request, CheckoutService service) =>
        {
            try
            {
                var result = await service.CreateAsync(request);
                return Results.Created($"/checkout/{result.Id}", result);
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ex.Errors.ToDictionary(e => e, e => new[] { e }));
            }
        })
        .WithName("CreateCheckout")
        .Produces<CheckoutResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapGet("/{id:guid}", async (Guid id, CheckoutService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetCheckout")
        .Produces<CheckoutResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
