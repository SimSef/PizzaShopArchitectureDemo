using Orleans;
using Orleans.Runtime;
using PizzaShop.Orleans.Contract;
using System.Threading.Tasks;

namespace PizzaShop.Orleans.Implementation
{
    [GenerateSerializer]
    public class CounterState
    {
        [Id(0)]
        public int Count { get; set; }
    }

    public class CounterGrain : Grain, ICounterGrain
    {
        private readonly IPersistentState<CounterState> _state;

        public CounterGrain(
            [PersistentState("count", "Default")] IPersistentState<CounterState> state)
        {
            _state = state;
        }

        public Task<int> GetCountAsync() => Task.FromResult(_state.State.Count);

        public async Task<int> IncrementAsync()
        {
            _state.State.Count++;
            await _state.WriteStateAsync();
            return _state.State.Count;
        }
    }
}

