using System;
using System.Net;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using OrleansSharedInterface;
using Microsoft.Extensions.Hosting;
using OrleansSharedGrains;
using OrleansSharedInterface.ConcreteClasses;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(OrleansClientFunctions.Startup))]
namespace OrleansClientFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            string keyVault = Environment.GetEnvironmentVariable("KeyVault");
            string DnsHost = Environment.GetEnvironmentVariable("DnsHost");
            var storSECRET = keyVaultClient.GetSecretAsync(keyVault).Result.Value;
            string ip = "127.0.0.1";
            if (DnsHost != "localhost")  // Don't resolve localhost, just use the loopback IP
            {
                ip = Dns.GetHostAddresses(DnsHost)[0].ToString();
            }
                // RUNNING THE HOST INPROC WITH THE CLIENT PROXY --- 
                // If you want to run the proxy as a sidecar in the same host as the function app, just on another thread
                // This a) is extremely experimental
                // This b) will probably break at some point
                // But right now it does work, and it's FAST

                // // THIS IS OPTIONAL 
                // var hb = new HostBuilder()
                // .UseOrleans(builder =>
                // {
                //    builder.Configure<ClusterOptions>(options =>
                //    {
                //        options.ClusterId = "will";
                //        options.ServiceId = "WillsService";
                //    });
                //    builder.UseAzureStorageClustering(options => options.ConnectionString = storSECRET);
                //    builder.AddAzureTableGrainStorage(
                //        name: "myGrainStorage",
                //        configureOptions: options =>
                //        {
                //            options.UseJson = true;
                //            options.ConnectionString = storSECRET;
                //        });
                //    builder.Configure<ProcessExitHandlingOptions>(options =>
                //    {
                //        options.FastKillOnProcessExit = false;
                //    });
                //    builder.ConfigureEndpoints(IPAddress.Parse(ip), 33350, 20010, true);
                //    builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(VoteCountGrain).Assembly).WithReferences());
                //    builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ITreeIndexNode<IndexedCustomer>).Assembly).WithReferences());
                //    builder.ConfigureLogging(logging => logging.AddConsole());
                //    builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
                // });

                // Task nt = Task.Factory.StartNew(() => hb.RunConsoleAsync());
                // END OF OPTIONAL CODE

                // Start the Client
                IClusterClient cb = new ClientBuilder()
                   .Configure<ClusterOptions>(options => {
                       options.ClusterId = "will";
                       options.ServiceId = "WillsService";

                   })
                   .Configure<ClientMessagingOptions>(o2 => {
                       o2.LocalAddress = System.Net.IPAddress.Parse("127.0.0.1");

                   })
                   .UseAzureStorageClustering(options => options.ConnectionString = storSECRET)
                   .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IVote).Assembly))
                   .ConfigureLogging(logging => logging.AddConsole())
                   .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
                   .Build();
                cb.Connect().Wait();
            
                // Inject
                builder.Services.AddSingleton<IClusterClient>(cb);


        }
    }
}
