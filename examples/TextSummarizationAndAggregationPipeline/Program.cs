using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Shared;
using static Shared.Steps;

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

var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Data");
var files = Directory.EnumerateFiles(path).ToAsyncEnumerable();

var filesCount = await files.CountAsync();

var pipeline = files
    .SelectAwait(ReadFile)
    .Where(IsValidFileForProcessing)
    .Summarize(kernel, summarizationFunction)
    .StoreBatch(batchSize: 10, path: Path.Combine(path, "summaries.txt"))
    .ReportProgress(totalCount: filesCount)
    .ForEachAsync(x => Console.WriteLine($"Processed {x.Name}"));

await pipeline;

public static class MyPipelineExtensions
{
    public static async IAsyncEnumerable<SummarizationPayload> Summarize(
        this IAsyncEnumerable<FilePayload> values,
        Kernel kernel,
        KernelFunction function
    )
    {
        await foreach (var value in values)
        {
            var result = await function.InvokeAsync(
                kernel,
                new KernelArguments(new OpenAIPromptExecutionSettings() { MaxTokens = 400 }) { ["input"] = value }
            );

            yield return new(value.Name, result.ToString());
        }
    }

    public static async IAsyncEnumerable<SummarizationPayload> StoreBatch(
        this IAsyncEnumerable<SummarizationPayload> values,
        int batchSize,
        string path
    )
    {
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

    public static async IAsyncEnumerable<T> ReportProgress<T>(this IAsyncEnumerable<T> values, int totalCount)
    {
        throw new NotImplementedException();
    }
}
