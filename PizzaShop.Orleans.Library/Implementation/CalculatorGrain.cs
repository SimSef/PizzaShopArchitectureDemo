using Orleans;
using System.Threading.Tasks;
using PizzaShop.Orleans.Contract;

namespace PizzaShop.Orleans.Implementation
{
    public class CalculatorGrain : Grain, ICalculatorGrain
    {
        public Task<int> Add(int l, int r) =>
            Task.FromResult(l + r);
    }
}
