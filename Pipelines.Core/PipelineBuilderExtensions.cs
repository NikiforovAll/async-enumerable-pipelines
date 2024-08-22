namespace Pipelines.Core;

public record PipelineOperation<T, TResult>
{
    public Func<IList<T>, ValueTask<IEnumerable<TResult>>>? Action1 { get; private set; }
    public Func<IList<T>, IEnumerable<TResult>>? Action2 { get; private set; }
    public Func<T, ValueTask<TResult>>? Action3 { get; private set; }
    public Func<T, TResult>? Action4 { get; private set; }

    public Lazy<IAsyncEnumerable<TResult>>? Result { get; set; }

    public IAsyncEnumerable<TResult> ProcessEachAsync(Func<T, ValueTask<TResult>> action)
    {
        this.Action3 = action;

        return this.Result.Value!;
    }

    public IAsyncEnumerable<TResult> ProcessEach(Func<T, TResult> action)
    {
        this.Action4 = action;

        return this.Result.Value!;
    }

    public IAsyncEnumerable<TResult> ProcessBatchAsync(Func<IList<T>, ValueTask<IEnumerable<TResult>>> action)
    {
        this.Action1 = action;

        return this.Result.Value!;
    }

    public IAsyncEnumerable<TResult> ProcessBatch(Func<IList<T>, IEnumerable<TResult>> action)
    {
        this.Action2 = action;
        return this.Result.Value!;
    }
}

public static class PipelineBuilderExtensions
{
    public static PipelineOperation<T, TResult> Batch<T, TResult>(this IAsyncEnumerable<T> pipeline, int batchSize)
    {
        var operation = new PipelineOperation<T, TResult>();

        operation.Result = new Lazy<IAsyncEnumerable<TResult>>(() =>
        {
            IAsyncEnumerable<IEnumerable<TResult>> result;

            if (operation.Action1 is not null)
            {
                result = pipeline.Buffer(batchSize).SelectAwait(operation.Action1);
            }
            else if (operation.Action2 is not null)
            {
                result = pipeline.Buffer(batchSize).Select(operation.Action2);
            }
            else if (operation.Action3 is not null)
            {
                result = pipeline
                    .Buffer(batchSize)
                    .SelectAwait(async x => await ProcessEachAsync(x, operation.Action3));
            }
            else if (operation.Action4 is not null)
            {
                result = pipeline.Buffer(batchSize).Select(x => x.Select(y => operation.Action4(y)));
            }
            else
            {
                throw new ArgumentException("Operation is not defined");
            }

            return result.SelectMany(x => x.ToAsyncEnumerable());
        });

        static async Task<IEnumerable<TResult>> ProcessEachAsync(IList<T> values, Func<T, ValueTask<TResult>> action)
        {
            var tasks = values.Select(action).Select(async x => await x).ToList();

            List<TResult> result = [];

            do
            {
                var completedTask = await Task.WhenAny(tasks);
                result.Add(await completedTask);
                tasks.Remove(completedTask);
            } while (tasks.Count > 0);

            return result;
        }

        return operation;
    }

    public static PipelineOperation<T, T> Batch<T>(this IAsyncEnumerable<T> pipeline, int batchSize)
    {
        return Batch<T, T>(pipeline, batchSize);
    }
}
