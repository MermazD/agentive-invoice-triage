using AgentEngine.Agents;
using AgentEngine.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class InvoiceTriageAgentTests
{
    [Fact]
    public void Constructor_CanResolvePluginsFromDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<InvoiceExtractionPlugin>();
        services.AddSingleton<ClassificationPlugin>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AZURE_OPENAI_ENDPOINT"] = "https://example.openai.azure.com",
                ["AZURE_OPENAI_AGENT_DEPLOYMENT"] = "gpt-4.1"
            })
            .Build();

        using var provider = services.BuildServiceProvider();

        var agent = new InvoiceTriageAgent(config, provider);

        Assert.NotNull(agent);
    }
}
