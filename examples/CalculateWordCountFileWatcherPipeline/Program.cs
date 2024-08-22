using System.Reactive.Linq;
using static Shared.Steps;

var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Data");

var fileWatcher = CreateFileObservable(path);

var pipeline = fileWatcher
    .TakeUntil(DateTimeOffset.Now.AddMinutes(1))
    .ToAsyncEnumerable()
    .SelectAwait(ReadFile)
    .Where(IsValidFileForProcessing)
    .Select(CalculateWordCount)
    .ForEachAsync(Console.WriteLine);

await pipeline;

static IObservable<string> CreateFileObservable(string path) =>
    Observable.Create<string>(observer =>
    {
        var watcher = new FileSystemWatcher(path)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
            EnableRaisingEvents = true
        };

        void onChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                observer.OnNext(e.FullPath);
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        }

        watcher.Created += onChanged;
        watcher.Changed += onChanged;

        return () =>
        {
            watcher.Created -= onChanged;
            watcher.Changed -= onChanged;
            watcher.Dispose();
        };
    });
