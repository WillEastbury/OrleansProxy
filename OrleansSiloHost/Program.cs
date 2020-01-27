using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using OrleansSharedGrains;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
//using OrleansSharedInterface;
namespace OrleansSiloHost
{
    class Program
    {
        public static Task Main(string[] args)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            string keyVault = Environment.GetEnvironmentVariable("KeyVault");
            string DnsHost = Environment.GetEnvironmentVariable("DnsHost");
            var storSECRET = keyVaultClient.GetSecretAsync(keyVault).Result.Value;
            string ip = "127.0.0.1";  // Don't resolve localhost, just use the loopback IP
            if (DnsHost != "localhost")
            {
                ip = Dns.GetHostAddresses(DnsHost)[0].ToString();
            }
            return new HostBuilder()
                .UseOrleans(builder =>
                {
                    builder.Configure<ClusterOptions>(options => {
                        options.ClusterId = "will";
                        options.ServiceId = "WillsService";
                    });
                    builder.UseAzureStorageClustering(options => options.ConnectionString = storSECRET);
                    builder.AddAzureTableGrainStorage(
                        name: "myGrainStorage",
                        configureOptions: options =>
                        {
                            options.UseJson = true;
                            options.ConnectionString = storSECRET;
                        });
                    builder.Configure<ProcessExitHandlingOptions>(options => {
                            options.FastKillOnProcessExit = false;
                        });
                    builder.ConfigureEndpoints(IPAddress.Parse(ip), 33350, 20010, true);
                    builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(VoteCountGrain).Assembly).WithReferences());
                    builder.ConfigureLogging(logging => logging.AddConsole());
                    builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
                })

            .RunConsoleAsync();

        }
    }
}
