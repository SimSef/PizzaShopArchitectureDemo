using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Orleans;
using PizzaShop.Orleans.Contract;
using PizzaShop.Web.Bff.Admin;

namespace PizzaShop.Web.Bff.Orders;

public static class CreateOrderEndpoint
{
    public static void MapCreateOrderEndpoint(this WebApplication app)
    {
        app.MapPost("/api/orders", async (CreateOrderRequest request, HttpContext httpContext, IClusterClient grainClient) =>
            {
                if (request.Items is null || request.Items.Count == 0 || !request.Items.Any(i => i.Quantity > 0))
                {
                    return Results.BadRequest(new { error = "Order must contain at least one pizza." });
                }

                var user = httpContext.User;

                var userId =
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    user.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Results.BadRequest(new { error = "Missing subject identifier." });
                }

                var userGrain = grainClient.GetGrain<IUserGrain>(userId);
                var userName = await userGrain.GetDisplayNameAsync();

                var normalizedItems = request.Items
                    .Where(i => i.Quantity > 0)
                    .Select(i =>
                    {
                        var lineTotal = i.UnitPrice * i.Quantity;
                        return new OrderLine(i.Name, i.Quantity, i.UnitPrice, lineTotal);
                    })
                    .ToList();

                var total = normalizedItems.Sum(i => i.LineTotal);
                if (total <= 0)
                {
                    return Results.BadRequest(new { error = "Total amount must be greater than zero." });
                }

                var summary = string.Join(", ", normalizedItems.Select(i => $"{i.Quantity}x {i.Name}"));

                var orderId = Guid.NewGuid();
                var orderGrain = grainClient.GetGrain<IOrderGrain>(orderId);
                await orderGrain.CreateAsync(userId, userName, summary, total);

                AdminDashboardState.RegisterOrder(userId, userName, summary, total, orderId);

                return Results.Ok(new { orderId, total });
            })
            .RequireAuthorization()
            .DisableAntiforgery();
    }
}

file sealed record CreateOrderRequest(List<OrderItemRequest> Items);

file sealed record OrderItemRequest(int PizzaId, string Name, int Quantity, decimal UnitPrice);

file sealed record OrderLine(string Name, int Quantity, decimal UnitPrice, decimal LineTotal);
