using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;
using AgentEngine.Plugins;           // ← Important

#pragma warning disable SKEXP0070

public static class KernelFactory
{
    public static Kernel Create(IConfiguration config)
        => Create(config, new ServiceCollection()
            .AddLogging()
            .AddSingleton<InvoiceExtractionPlugin>()
            .AddSingleton<ClassificationPlugin>()
            .BuildServiceProvider());

    public static Kernel Create(IConfiguration config, IServiceProvider serviceProvider)
    {
        var builder = Kernel.CreateBuilder();

        string endpoint = config["AZURE_OPENAI_ENDPOINT"] 
            ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT manquante");

        string deployment = config["AZURE_OPENAI_AGENT_DEPLOYMENT"] 
            ?? throw new InvalidOperationException("AZURE_OPENAI_AGENT_DEPLOYMENT manquante");

        builder.AddAzureOpenAIChatCompletion(
        deploymentName: deployment,
        endpoint: endpoint,
        credentials: new DefaultAzureCredential());
        
        // builder.AddAzureAIInferenceChatCompletion(
        //     modelId: deployment,
        //     endpoint: new Uri(endpoint),
        //     credential: new DefaultAzureCredential());

        var kernel = builder.Build();

        var extractionPlugin = serviceProvider.GetRequiredService<InvoiceExtractionPlugin>();
        var classificationPlugin = serviceProvider.GetRequiredService<ClassificationPlugin>();

        kernel.ImportPluginFromObject(extractionPlugin, "InvoiceExtractionPlugin");
        kernel.ImportPluginFromObject(classificationPlugin, "ClassificationPlugin");

        return kernel;
    }
}