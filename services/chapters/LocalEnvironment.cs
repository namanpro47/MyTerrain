namespace Terra.Chapters;

/// <summary>Small local-only dotenv reader. Never evaluates or logs file contents.</summary>
public static class LocalEnvironment
{
    static readonly HashSet<string> Supported = new(StringComparer.Ordinal) {
        "GEMINI_API_KEY", "GEMINI_MODEL", "TERRA_ALLOWED_ORIGINS", "TERRA_CHAPTER_CACHE"
    };

    public static void LoadForProject(string contentRoot)
    {
        // Hosted deployments use their platform-injected environment exclusively.
        if (Environment.GetEnvironmentVariable("VERCEL") == "1") return;
        string root = File.Exists(Path.Combine(contentRoot, "Terra.Chapters.csproj"))
            ? Path.GetFullPath(Path.Combine(contentRoot, "../..")) : contentRoot;
        if (!File.Exists(Path.Combine(root, "services/chapters/Terra.Chapters.csproj"))) return;
        Load(Path.Combine(root, ".env"), Environment.GetEnvironmentVariable,
            (name, value) => Environment.SetEnvironmentVariable(name, value));
    }

    public static void Load(string path, Func<string, string> existing, Action<string, string> set)
    {
        if (!File.Exists(path)) return;
        var parsed = new Dictionary<string, string>(StringComparer.Ordinal);
        int lineNumber = 0;
        foreach (var line in File.ReadLines(path))
        {
            lineNumber++;
            var text = line.Trim();
            if (text.Length == 0 || text.StartsWith('#')) continue;
            if (text.StartsWith("export ", StringComparison.Ordinal)) text = text[7..].TrimStart();
            int separator = text.IndexOf('=');
            if (separator < 1) throw InvalidLine(lineNumber);
            string name = text[..separator].Trim();
            if (!Supported.Contains(name)) continue; // Cannot enable ASP.NET debug/binding via this file.
            string value = text[(separator + 1)..].Trim();
            if (value.StartsWith('"') || value.StartsWith('\''))
            {
                char quote = value[0];
                int end = value.IndexOf(quote, 1);
                if (end < 0) throw InvalidLine(lineNumber);
                string remainder = value[(end + 1)..].Trim();
                if (remainder.Length > 0 && !remainder.StartsWith('#')) throw InvalidLine(lineNumber);
                value = value[1..end];
            }
            else
            {
                int comment = value.IndexOf(" #", StringComparison.Ordinal);
                if (comment >= 0) value = value[..comment].TrimEnd();
            }
            parsed[name] = value;
        }
        // Parse the whole file before changing process state. Existing platform/shell values win.
        foreach (var pair in parsed)
            if (existing(pair.Key) == null) set(pair.Key, pair.Value);
    }

    static FormatException InvalidLine(int line) => new("Invalid .env syntax at line " + line + ". Use NAME=value, optionally quoted. Values are not shown.");
}
