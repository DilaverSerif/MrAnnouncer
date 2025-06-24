using HtmlAgilityPack;

public class News_NEWYORK : Abstract_News
{
    public override async Task<List<string>> GetNewsLinksAsync()
    {
        string url = "https://www.nytimes.com/section/world";
        var client = ScarperHelper.SetupHttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", ScarperHelper.userAgents[new Random().Next(0, ScarperHelper.userAgents.Length)]);

        try
        {
            var html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // HTML içeriğini görmek için bu satır kullanılır:
            Console.WriteLine("HTML Content:");
            Console.WriteLine(html);

            var links = doc.DocumentNode
                .SelectNodes("//a[@href]")
                ?.Select(a => a.GetAttributeValue("href", ""))
                .Where(href =>
                    !string.IsNullOrWhiteSpace(href) &&
                    (href.StartsWith("/202") || href.StartsWith("https://www.nytimes.com/202"))
                )
                .Select(href =>
                    href.StartsWith("/") ? $"https://www.nytimes.com{href}" : href
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

        // Başlık
        var title = doc.DocumentNode
            .SelectSingleNode("//meta[@property='og:title']")
            ?.GetAttributeValue("content", "").Trim()
            ?? doc.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim()
            ?? "";

        // Görsel
        var imageUrl = doc.DocumentNode
            .SelectSingleNode("//meta[@property='og:image']")
            ?.GetAttributeValue("content", "")
            ?? "";

        // Paragraflar (makale gövdesi <section><p>)
        var paragraphs = doc.DocumentNode
            .SelectNodes("//section//p")
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
