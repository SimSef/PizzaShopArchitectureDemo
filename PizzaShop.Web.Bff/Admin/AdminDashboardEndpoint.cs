using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Orleans;
using PizzaShop.Orleans.Contract;

namespace PizzaShop.Web.Bff.Admin;

public static class AdminDashboardEndpoint
{
    public static void MapAdminDashboardEndpoint(this WebApplication app)
    {
        app.MapGet("/api/admin/dashboard", async (HttpContext httpContext, IClusterClient grainClient) =>
            {
                if (httpContext.User?.Identity?.IsAuthenticated != true)
                {
                    return Results.Unauthorized();
                }

                var snapshot = AdminDashboardState.GetSnapshot();

                var userSummaries = await Task.WhenAll(snapshot.UserIds.Select(async userId =>
                {
                    var userGrain = grainClient.GetGrain<IUserGrain>(userId);
                    var displayName = await userGrain.GetDisplayNameAsync();
                    return new UserSummary(userId, displayName);
                }));

                return Results.Ok(new { users = userSummaries, orders = snapshot.Orders });
            })
            .RequireAuthorization();
    }
}

internal sealed record OrderOverview(Guid OrderId, string UserId, string UserName, string Summary, decimal TotalAmount, DateTimeOffset CreatedAt);

internal sealed class AdminDashboardState
{
    private readonly object _lock = new();
    private readonly HashSet<string> _userIds = new();
    private readonly List<OrderOverview> _orders = new();

    public static void RegisterOrder(string userId, string userName, string summary, decimal totalAmount, Guid orderId)
    {
        var now = DateTimeOffset.UtcNow;

        var state = Instance;

        lock (state._lock)
        {
            state._userIds.Add(userId);
            state._orders.Add(new OrderOverview(orderId, userId, userName, summary, totalAmount, now));
        }
    }

    public static AdminDashboardSnapshot GetSnapshot()
    {
        var state = Instance;

        lock (state._lock)
        {
            return new AdminDashboardSnapshot(state._userIds.ToList(), state._orders.ToList());
        }
    }

    private static readonly Lazy<AdminDashboardState> LazyInstance = new(() => new AdminDashboardState());
    private static AdminDashboardState Instance => LazyInstance.Value;
}

file sealed record UserSummary(string UserId, string DisplayName);

internal sealed record AdminDashboardSnapshot(IReadOnlyList<string> UserIds, IReadOnlyList<OrderOverview> Orders);
