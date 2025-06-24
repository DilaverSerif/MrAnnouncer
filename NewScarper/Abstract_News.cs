public abstract class Abstract_News
{
    public abstract Task<List<string>> GetNewsLinksAsync();
    public abstract Task<NewsData> GetNewsAsync(string url);

    public virtual async Task<List<NewsData>> GetNewsAsync(List<string> urls)
    {
        var news = new List<NewsData>();
        foreach (var url in urls)
        {
            var newsData = await GetNewsAsync(url);
            news.Add(newsData);
        }
        return news;
    }
}