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
            IClusterClient cb = new ClientBuilder()
                .Configure<ClusterOptions>(options =>{
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
            builder.Services.AddSingleton<IClusterClient>(cb);
        }
    }
}
