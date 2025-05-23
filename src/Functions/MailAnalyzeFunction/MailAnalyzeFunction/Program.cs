using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MailAnalyzeFunction.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        // Azure OpenAI分析サービスの登録
        services.AddSingleton<EmailContentAnalyzer>();
        
        // CosmosDB業者比較サービスの登録
        services.AddSingleton<VendorComparisonService>();
    })
    .Build();

host.Run();
