using System.Reactive.Linq;
using Pipelines.Core;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

var pipeline = BlobPipeline.Scan().Build().ForEachAsync(Console.WriteLine);

var pipeline2 = BlobPipeline
    .FromFiles(new FileMessage() { Name = "test" }, new FileMessage() { Name = "test2" })
    .Build()
    .ForEachAsync(Console.WriteLine);

var pipeline3 = BlobPipeline
    .FromSource(Observable.Range(1, 100).Select(x => new FileMessage() { Name = $"name-{x}" }))
    .Build()
    .ForEachAsync(Console.WriteLine);

public static class BlobPipeline
{
    public static IAsyncEnumerable<FileMessage> Scan() =>
        AsyncEnumerable.Range(1, 100).Select(x => new FileMessage() { Name = $"name-{x}" });

    public static IAsyncEnumerable<FileMessage> FromFiles(params FileMessage[] files) => files.ToAsyncEnumerable();

    public static IAsyncEnumerable<FileMessage> FromSource(this IObservable<FileMessage> source) =>
        source.ToAsyncEnumerable();

    public static IAsyncEnumerable<FileMessage.WithPayload<string>> Build(this IAsyncEnumerable<FileMessage> pipeline)
    {
        return pipeline.CreateEmbeddings().Batch(batchSize: 10).ProcessAsync(StoreEmbeddings);
    }

    public static IAsyncEnumerable<FileMessage.WithPayload<string>> CreateEmbeddings(
        this IAsyncEnumerable<FileMessage> pipeline
    )
    {
        return pipeline.SelectAwait(CreateEmbeddings);
    }

    public static ValueTask<FileMessage.WithPayload<string>> CreateEmbeddings(this FileMessage message)
    {
        return ValueTask.FromResult(new FileMessage.WithPayload<string>(message, "embedding"));
    }

    public static ValueTask<IEnumerable<FileMessage.WithPayload<string>>> StoreEmbeddings(
        this IList<FileMessage.WithPayload<string>> batch
    )
    {
        return ValueTask.FromResult(batch.AsEnumerable());
    }
}

public record FileMessage
{
    public required string Name;

    public record WithPayload<T>(FileMessage Source, T Payload);
}
