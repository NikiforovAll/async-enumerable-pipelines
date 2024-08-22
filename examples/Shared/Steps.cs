namespace Shared;

public static class Steps
{
    public static async ValueTask<FilePayload> ReadFile(string file)
    {
        var content = await File.ReadAllTextAsync(file);
        var name = Path.GetFileName(file);

        return new FilePayload(name, content);
    }

    public static bool IsValidFileForProcessing(FilePayload file) =>
        file is { Content.Length: > 0, Name: [.., 't', 'x', 't'] };

    public static WordCountPayload CalculateWordCount(FilePayload payload)
    {
        var words = payload.Content.Split(' ');

        return new(payload.Name, words.Length);
    }
}

public record FilePayload(string Name, string Content);
public record SummarizationPayload(string Name, string Summary);

public record WordCountPayload(string Name, int WordCount);
