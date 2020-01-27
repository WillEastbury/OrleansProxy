using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansSharedInterface;

namespace OrleansClientFunctions
{
    public class HelloWorld
    {
        public HelloWorld(IClusterClient _cb)
        {
           cb = _cb;
        }
        private IClusterClient cb = null;     

        [FunctionName("OrleansVoteCounter")]
        public async Task<IActionResult> RunVote([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "votes/{sessionid}/{questionid}/{voterid}")] HttpRequest req, ILogger log, string voterid, string sessionid, string questionid)
        {
            return  (ActionResult)new OkObjectResult((await cb.GetGrain<IVote>($"{sessionid}/{questionid}/{voterid}").CountMyVote()).ToString());
        }
    }
}
