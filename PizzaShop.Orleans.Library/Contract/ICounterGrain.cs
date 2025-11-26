using Orleans;
using System.Threading.Tasks;

namespace PizzaShop.Orleans.Contract
{
    public interface ICounterGrain : IGrainWithStringKey
    {
        Task<int> GetCountAsync();
        Task<int> IncrementAsync();
    }
}

