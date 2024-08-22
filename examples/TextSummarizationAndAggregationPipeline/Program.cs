using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Shared;
using static Shared.Steps;

var (kernel, summarizationFunction) = Init();
var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Data");

var pipeline = Directory
    .EnumerateFiles(path)
    .ToAsyncEnumerable()
    .ReportProgress()
    .SelectAwait(ReadFile)
    .Where(IsValidFileForProcessing)
    .SelectAwait(Summarize)
    .WriteResultToFile(path: Path.Combine(path, "summaries.txt"))
    .ForEachAsync(x => Console.WriteLine($"Processed {x.Name}"));

await pipeline;

static (Kernel kernel, KernelFunction summarizationFunction) Init()
{
    var builder = Host.CreateApplicationBuilder();
    var endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"]!;
    var deployment = builder.Configuration["AZURE_OPENAI_GPT_NAME"]!;
    var key = builder.Configuration["AZURE_OPENAI_KEY"]!;
    var kernelBuilder = builder.Services.AddKernel();

    kernelBuilder.AddAzureOpenAIChatCompletion(deployment, endpoint, key);

    var services = builder.Build().Services;

    var kernel = services.GetRequiredService<Kernel>();
    var prompt = """
        Please summarize the the following text in 20 words or less:
        ${input}
        """;
    var summarizationFunction = kernel.CreateFunctionFromPrompt(prompt);

    return (kernel, summarizationFunction);
}

async ValueTask<SummarizationPayload> Summarize(FilePayload file)
{
    var result = await summarizationFunction.InvokeAsync(
        kernel,
        new KernelArguments(new OpenAIPromptExecutionSettings() { MaxTokens = 400 }) { ["input"] = file.Content }
    );

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

        using var streamWriter = new StreamWriter(path);

        await foreach (var batch in values.Buffer(batchSize))
        {
            foreach (var value in batch)
            {
                await streamWriter.WriteLineAsync(value.Summary);

                yield return value;
            }

            streamWriter.Flush();
        }
    }

    public static async IAsyncEnumerable<T> ReportProgress<T>(this IAsyncEnumerable<T> values)
    {
        var totalCount = await values.CountAsync();

        await foreach (var value in values)
        {
            yield return value;
        }
    }
}
