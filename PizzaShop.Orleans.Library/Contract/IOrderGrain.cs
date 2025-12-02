using Orleans;
using System.Threading.Tasks;

namespace PizzaShop.Orleans.Contract;

/// <summary>
/// Represents a single order placed by a user.
/// </summary>
[Alias("order")]
public interface IOrderGrain : IGrainWithGuidKey
{
    /// <summary>
    /// Initializes the order with basic details.
    /// </summary>
    Task CreateAsync(string userId, string userName, string summary, decimal totalAmount);
}

