using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;  
using OrleansSharedInterface;
using Orleans.Runtime;

namespace OrleansSharedGrains
{
    public class HelloGrain : Orleans.Grain, IHello
    {
        private readonly ILogger logger;
        private readonly IPersistentState<HelloState> _profile;

        public HelloGrain(ILogger<HelloGrain> logger,[PersistentState("mgs", "myGrainStorage")] IPersistentState<HelloState> profile)
        {
            this.logger = logger;
             _profile = profile;
        }

        async Task<string> IHello.SayHello(string greeting)
        {
            int count = _profile.State.hellos;
            logger.LogInformation($"\n SayHello message received: greeting = '{greeting}'");
            _profile.State.hellos = count + 1;

            await _profile.WriteStateAsync();
            return $"\n Client said: '{greeting}', so HelloGrain says: Hello! {_profile.State.hellos.ToString()}";
           
        }
    }

    [Serializable]
    public class HelloState
    {
        public int hellos { get; set; }
   
    }

    
  
}
