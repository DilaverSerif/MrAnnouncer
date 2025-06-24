using System.Text.Json;

public class SharedNews
{
    public string Username { get; set; }
    public string NewsUrl { get; set; }
    public DateTime SharedAt { get; set; }
}

public static class Checkers
{
    private static readonly string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "SharedNews.json");
    private static List<SharedNews> _sharedNews;

    static Checkers()
    {
        LoadSharedNews();
    }

    private static void LoadSharedNews()
    {
        if (File.Exists(SavePath))
        {
            var json = File.ReadAllText(SavePath);
            _sharedNews = JsonSerializer.Deserialize<List<SharedNews>>(json) ?? new List<SharedNews>();
        }
        else
        {
            _sharedNews = new List<SharedNews>();
            SaveSharedNews();
        }
    }

    private static void SaveSharedNews()
    {
        var json = JsonSerializer.Serialize(_sharedNews, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SavePath, json);
    }

    public static void AddSharedNews(string username, string newsUrl)
    {
        var sharedNews = new SharedNews
        {
            Username = username,
            NewsUrl = newsUrl,
            SharedAt = DateTime.Now
        };

        _sharedNews.Add(sharedNews);
        SaveSharedNews();
    }

    public static bool HasUserSharedNews(string username, string newsUrl)
    {
        return _sharedNews.Any(n => 
            n.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
            n.NewsUrl.Equals(newsUrl, StringComparison.OrdinalIgnoreCase));
    }

    public static List<SharedNews> GetUserSharedNews(string username)
    {
        return _sharedNews
            .Where(n => n.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(n => n.SharedAt)
            .ToList();
    }

    public static void ClearOldNews(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
        _sharedNews = _sharedNews.Where(n => n.SharedAt > cutoffDate).ToList();
        SaveSharedNews();
    }
}
