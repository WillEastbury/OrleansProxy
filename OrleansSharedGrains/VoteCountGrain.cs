using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;  
using OrleansSharedInterface;
using Orleans.Runtime;

namespace OrleansSharedGrains
{
    public class VoteCountGrain : Orleans.Grain, IVote
    {
        private readonly ILogger logger;
        private readonly IPersistentState<VoteCountState> _profile;

        public VoteCountGrain(ILogger<VoteCountGrain> logger,[PersistentState("mgs", "myGrainStorage")] IPersistentState<VoteCountState> profile)
        {
            this.logger = logger;
             _profile = profile;
        }

        async Task<int> IVote.CountMyVote()
        {

            _profile.State.Votes ++; 

            await _profile.WriteStateAsync();
            return _profile.State.Votes;
           
        }
    }


    [Serializable]
    public class VoteCountState
    {

        public int Votes { get; set; }
    
    }
}
