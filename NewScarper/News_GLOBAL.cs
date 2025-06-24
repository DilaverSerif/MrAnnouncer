using HtmlAgilityPack;

public class News_GLOBAL:Abstract_News
{
    public override async Task<List<string>> GetNewsLinksAsync()
    {
        string url = "https://www.globaltimes.cn/world/index.html";

        using var httpClient = ScarperHelper.SetupHttpClient();
        var html = await httpClient.GetStringAsync(url);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var links = htmlDoc.DocumentNode
            .SelectNodes("//a[@href]")
            ?.Select(a => a.GetAttributeValue("href", ""))
            .Where(href => href.StartsWith("https://www.globaltimes.cn/page/"))
            .Distinct()
            .ToList();

        return links ?? new List<string>();
    }

    public override async Task<NewsData> GetNewsAsync(string url)
    {
        var httpClient = new HttpClient();
        var html = await httpClient.GetStringAsync(url);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var title = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='article_title']")?.InnerText?.Trim();
        var description = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", "")?.Trim();
        var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']")?.GetAttributeValue("content", "")?.Trim();

        return new NewsData
        {
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            newUrl = url
        };
    }
}