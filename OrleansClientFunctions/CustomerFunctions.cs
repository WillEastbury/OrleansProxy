using System.ComponentModel.Design;
using System.Security.Cryptography;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using Newtonsoft.Json;
using Orleans;
using Orleans.Runtime;
using OrleansSharedInterface;
using OrleansSharedInterface.ConcreteClasses;
using Orleans.Configuration;
using Orleans.Hosting;

namespace OrleansClientFunctions
{
    public class CustomerFunctions
    {
        public CustomerFunctions(IClusterClient _cb)
        {
           cb = _cb;
        }
        private IClusterClient cb = null; 

        [FunctionName("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customer/{CustomerId}")] IndexedCustomer req, ILogger log, string CustomerId)
        {
            // Creating a customer
            ITreeIndexNode<IndexedCustomer> createdCustomerRootIndexActor = cb.GetGrain<ITreeIndexNode<IndexedCustomer>>(CustomerId);
            
            await createdCustomerRootIndexActor.AddNode(CustomerId, req);
            IndexedCustomer thisone = await createdCustomerRootIndexActor.GetThisNodeData(); 

            return (ActionResult) new OkObjectResult($"Customer { req.CustomerID } Created OK as /r/n {thisone.CustomerID}");
            
        }
    }

}
