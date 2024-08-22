using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Shared;
using Spectre.Console;
using static Shared.Steps;

var kernel = Init();
var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Data");

var pipeline = Directory
    .EnumerateFiles(path)
    .ToAsyncEnumerable()
    .ReportProgress()
    .SelectAwait(ReadFile)
    .Where(IsValidFileForProcessing)
    .SelectAwait(Summarize)
    .WriteResultToFile(path: Path.Combine(Path.GetTempPath(), "summaries.txt"))
    .ForEachAsync(x => AnsiConsole.MarkupLine($"Processed [green]{x.Name}[/]"));

await pipeline;

static Kernel Init()
{
    var builder = Host.CreateApplicationBuilder(
        new HostApplicationBuilderSettings { EnvironmentName = Environments.Development }
    );
    builder.Services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));

    var endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"]!;
    var deployment = builder.Configuration["AZURE_OPENAI_GPT_NAME"]!;
    var key = builder.Configuration["AZURE_OPENAI_KEY"]!;
    var kernelBuilder = builder.Services.AddKernel();

    kernelBuilder.AddAzureOpenAIChatCompletion(deployment, endpoint, key);

    var services = builder.Build().Services;

    var kernel = services.GetRequiredService<Kernel>();

    return kernel;
}

async ValueTask<SummarizationPayload> Summarize(FilePayload file)
{
    var prompt = """
        {{$input}}
        Please summarize the content above in 20 words or less:

        The output format should be: [title]: [summary]
        """;

    var result = await kernel.InvokePromptAsync(prompt, new KernelArguments() { ["input"] = file.Content });

    return new(file.Name, result.ToString());
}

public static class MyPipelineExtensions
{
    public static async IAsyncEnumerable<SummarizationPayload> WriteResultToFile(
        this IAsyncEnumerable<SummarizationPayload> values,
        string path
    )
    {
        const int batchSize = 10;

        using var streamWriter = new StreamWriter(path, append: true);

        await foreach (var batch in values.Buffer(batchSize))
        {
            foreach (var value in batch)
            {
                await streamWriter.WriteLineAsync(value.Summary);

                yield return value;
            }

            await streamWriter.FlushAsync();
        }

        AnsiConsole.MarkupLine($"Results written to [green]{path}[/]");
    }

    public static async IAsyncEnumerable<string> ReportProgress(this IAsyncEnumerable<string> values)
    {
        var totalCount = await values.CountAsync();

        await foreach (var (value, index) in values.Select((value, index) => (value, index)))
        {
            yield return value;

            AnsiConsole
                .Progress()
                .Start(ctx =>
                {
                    var task = ctx.AddTask($"Processing - {Path.GetFileName(value)}", true, totalCount);
                    task.Increment(index + 1);
                    task.StopTask();
                });
        }
    }
}
