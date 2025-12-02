using Orleans;
using PizzaShop.Orleans.Contract;
using System;
using System.Threading.Tasks;

namespace PizzaShop.Orleans.Implementation;

public class OrderGrain() : Grain, IOrderGrain
{
    public Guid OrderId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Task CreateAsync(string userId, string userName, string summary, decimal totalAmount)
    {
        OrderId = this.GetPrimaryKey();
        UserId = userId;
        UserName = userName;
        Summary = summary;
        TotalAmount = totalAmount;
        CreatedAt = DateTimeOffset.UtcNow;

        return Task.CompletedTask;
    }
}

