using HtmlAgilityPack;
    
public class News_BBC : Abstract_News
{

    public override async Task<List<string>> GetNewsLinksAsync()
    {
        var articleLinks = new List<string>();

        using (HttpClient client = ScarperHelper.SetupHttpClient())
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            try
            {
                string html = await client.GetStringAsync("https://www.bbc.com/news");

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var hrefs = doc.DocumentNode
                    .SelectNodes("//a[@href]")
                    ?.Select(a => a.GetAttributeValue("href", ""))
                    .Where(href =>
                        !string.IsNullOrWhiteSpace(href) &&
                        (href.StartsWith("/news/articles/") || href.StartsWith("https://www.bbc.com/news/articles/"))
                    )
                    .Select(href =>
                        href.StartsWith("/") ? $"https://www.bbc.com{href}" : href
                    )
                    .Distinct();

                if (hrefs != null)
                    articleLinks = hrefs.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }
        }

        return articleLinks;
    }

    public override async Task<NewsData> GetNewsAsync(string url)
    {
        var httpClient = ScarperHelper.SetupHttpClient();

        string html = await httpClient.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Title: <meta property="og:title" content="...">
        var titleNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
        string title = titleNode?.GetAttributeValue("content", "")?.Trim() ?? "";

        // Image: <meta property="og:image" content="...">
        var imageNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
        string imageUrl = imageNode?.GetAttributeValue("content", "") ?? "";

        // Description: <article> içindeki <p> etiketlerinden
        var articleNode = doc.DocumentNode.SelectSingleNode("//article");
        var paragraphs = articleNode?
            .SelectNodes(".//p")
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
