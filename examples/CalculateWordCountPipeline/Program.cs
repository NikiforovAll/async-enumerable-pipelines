using static Shared.Steps;

var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Data");

var pipeline = Directory
    .EnumerateFiles(path)
    .ToAsyncEnumerable()
    .SelectAwait(ReadFile)
    .Where(IsValidFileForProcessing)
    .Select(CalculateWordCount)
    .OrderByDescending(x => x.WordCount)
    .ForEachAsync(Console.WriteLine);

await pipeline;
