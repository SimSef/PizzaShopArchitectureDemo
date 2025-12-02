using Orleans;
using System.Threading.Tasks;

namespace PizzaShop.Orleans.Contract
{
    /// <summary>
    /// Represents a user of the PizzaShop system.
    /// The grain identity should match the user identifier used by the frontend (for example, the OIDC subject or username).
    /// </summary>
    [Alias("user")]
    public interface IUserGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Sets or updates the user profile data for this grain.
        /// </summary>
        Task SetProfileAsync(string username, string? email, string? firstName, string? lastName);
    }
}
