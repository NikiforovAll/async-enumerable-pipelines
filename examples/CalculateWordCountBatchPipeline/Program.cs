using Pipelines.Core;
using Shared;
using static Shared.Steps;

var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Data");

const int batchSize = 2;

var pipeline = Directory
    .EnumerateFiles(path)
    .ToAsyncEnumerable()
    .Batch<string, FilePayload>(batchSize)
    .ProcessEachAsync(ReadFile)
    .Where(IsValidFileForProcessing)
    .Select(CalculateWordCount)
    .OrderByDescending(x => x.WordCount)
    .ForEachAsync(Console.WriteLine);

await pipeline;
