using Orleans;
using PizzaShop.Orleans.Contract;
using System.Threading.Tasks;

namespace PizzaShop.Orleans.Implementation;

public class UserGrain() : Grain, IUserGrain
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    public Task SetProfileAsync(string username, string? email, string? firstName, string? lastName)
    {
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;

        return Task.CompletedTask;
    }
}
