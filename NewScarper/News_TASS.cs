using HtmlAgilityPack;

public class News_TASS:Abstract_News
{
    public override async Task<List<string>> GetNewsLinksAsync()
    {
        string url = "https://tass.com/world";
        var client = ScarperHelper.SetupHttpClient();

        try
        {
            string html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var links = doc.DocumentNode
                .SelectNodes("//a[@href]")
                ?.Select(a => a.GetAttributeValue("href", ""))
                .Where(href =>
                    href.StartsWith("/en/world/") && href.EndsWith(".html")
                )
                .Select(href => $"https://tass.com{href}")
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

        // Başlık
        var title = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']")
            ?.GetAttributeValue("content", "").Trim() ?? "";

        // Görsel
        var imageUrl = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']")
            ?.GetAttributeValue("content", "") ?? "";

        // URL
        var pageUrl = doc.DocumentNode.SelectSingleNode("//meta[@property='og:url']")
            ?.GetAttributeValue("content", "") ?? "";

        // İçerik: <div class="text-block"> altındaki tüm <p> etiketleri
        var paragraphs = doc.DocumentNode
            .SelectNodes("//div[contains(@class, 'text-block')]//p")
            ?.Select(p => p.InnerText.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList() ?? new List<string>();

        string description = string.Join("\n\n", paragraphs);

        return new NewsData
        {
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            newUrl = pageUrl
        };
    }
}