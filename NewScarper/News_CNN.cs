using HtmlAgilityPack;

public class News_CNN:Abstract_News
{
    public override async Task<List<string>> GetNewsLinksAsync()
    {
        string url = "https://edition.cnn.com/world";
        var client = ScarperHelper.SetupHttpClient();

        try
        {
            var html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var links = doc.DocumentNode
                .SelectNodes("//a[@href]")
                ?.Select(a => a.GetAttributeValue("href", ""))
                .Where(href =>
                    !string.IsNullOrWhiteSpace(href) &&
                    (href.StartsWith("/202") || href.StartsWith("https://edition.cnn.com/202"))
                )
                .Select(href =>
                    href.StartsWith("/") ? $"https://edition.cnn.com{href}" : href
                )
                .Distinct()
                .ToList() ?? new List<string>();

            return links;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Hata: " + ex.Message);
            return new List<string>();
        }
    }

    public override async Task<NewsData> GetNewsAsync(string url)
    {
        var client = ScarperHelper.SetupHttpClient();

        string html = await client.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Başlık (og:title ya da <title>)
        var title = doc.DocumentNode
            .SelectSingleNode("//meta[@property='og:title']")
            ?.GetAttributeValue("content", "").Trim()
            ?? doc.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim()
            ?? "";

        // Görsel (og:image)
        var imageUrl = doc.DocumentNode
            .SelectSingleNode("//meta[@property='og:image']")
            ?.GetAttributeValue("content", "") ?? "";

        // İçerik (CNN haber gövdesi genellikle <article> içindeki <div><p>...</p></div>)
        var paragraphs = doc.DocumentNode
            .SelectNodes("//article//p")
            ?.Select(p => p.InnerText.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList() ?? new List<string>();

        string description = string.Join("\n\n", paragraphs);

        return new NewsData
        {
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            newUrl = url
        };
    }


}