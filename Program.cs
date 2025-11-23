using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string? connectionString = configuration["AppConfigurationConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("AppConfigurationConnectionString", "The connection string is not configured in appsettings.json.");
        }

        var azureConfig = new ConfigurationBuilder()
            .AddAzureAppConfiguration(options =>
            {
                options.Connect(connectionString)
                       .UseFeatureFlags();
            })
            .Build();

        var services = new ServiceCollection();
        services.AddFeatureManagement(azureConfig);

        var serviceProvider = services.BuildServiceProvider();
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

        string? welcomeMessage = azureConfig["WelcomeMessage"];
        if (string.IsNullOrEmpty(welcomeMessage))
        {
            Console.WriteLine("Welcome message not found in App Configuration.");
        }
        else
        {
            Console.WriteLine(welcomeMessage);
        }

        // Verificar Feature Flags
        var featureName = "ShowAdditionalMessage";
        if (await featureManager.IsEnabledAsync(featureName))
        {
            Console.WriteLine("Additional functionality enabled! This message is dynamic.");
        }
        else
        {
            Console.WriteLine("Additional functionality is disabled.");
        }
    }
}
