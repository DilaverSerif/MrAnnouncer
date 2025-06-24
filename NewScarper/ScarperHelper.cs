using Microsoft.Playwright;
using System.Net;
using System.Net.Http.Headers;

public static class ScarperHelper
{
    private static readonly Random random = new Random();
    private static readonly HttpClientHandler httpClientHandler = new HttpClientHandler();
    private static readonly HttpClient httpClient;

    static ScarperHelper()
    {
        // Proxy ayarları
        httpClientHandler.UseProxy = false; // Proxy kullanmak istediğinizde true yapın
        // httpClientHandler.Proxy = new WebProxy("proxy_address", port);

        // SSL/TLS ayarları
        httpClientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        httpClientHandler.CheckCertificateRevocationList = true;

        // Otomatik yönlendirme
        httpClientHandler.AllowAutoRedirect = true;
        httpClientHandler.MaxAutomaticRedirections = 5;

        httpClient = new HttpClient(httpClientHandler);
        httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public static string[] userAgents = new[]
    {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:123.0) Gecko/20100101 Firefox/123.0",
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/122.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.0"
    };

    public static string[] referrers = new[]
    {
        "https://www.google.com/",
        "https://www.bing.com/",
        "https://www.facebook.com/",
        "https://twitter.com/",
        "https://www.reddit.com/",
        "https://www.linkedin.com/"
    };

    public static string[] acceptLanguages = new[]
    {
        "en-US,en;q=0.9",
        "en-GB,en;q=0.9",
        "tr-TR,tr;q=0.9,en;q=0.8",
        "fr-FR,fr;q=0.9,en;q=0.8",
        "de-DE,de;q=0.9,en;q=0.8",
        "es-ES,es;q=0.9,en;q=0.8"
    };

    public static HttpClient SetupHttpClient()
    {
        var httpClientHandler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };

        var client = new HttpClient(httpClientHandler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        client.DefaultRequestHeaders.Clear();

        // Rastgele User-Agent seç
        var userAgent = userAgents[random.Next(userAgents.Length)];
        client.DefaultRequestHeaders.Add("User-Agent", userAgent);

        // Rastgele Referrer seç
        var referrer = referrers[random.Next(referrers.Length)];
        client.DefaultRequestHeaders.Add("Referer", referrer);

        // Rastgele dil seç
        var acceptLanguage = acceptLanguages[random.Next(acceptLanguages.Length)];
        client.DefaultRequestHeaders.Add("Accept-Language", acceptLanguage);

        // Diğer gerekli başlıklar
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
        client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");

        return client;
    }


    public static async Task<string> GetStringWithRetryAsync(string url, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var client = SetupHttpClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(30);
                    await Task.Delay(retryAfter);
                    continue;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    Console.WriteLine($"Access forbidden. Attempt {i + 1}/{maxRetries}");
                    await Task.Delay(TimeSpan.FromSeconds(5 * (i + 1)));
                    continue;
                }

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Error (Attempt {i + 1}/{maxRetries}): {ex.Message}");
                if (i == maxRetries - 1) throw;
                await Task.Delay(TimeSpan.FromSeconds(5 * (i + 1)));
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Request timeout (Attempt {i + 1}/{maxRetries})");
                if (i == maxRetries - 1) throw;
                await Task.Delay(TimeSpan.FromSeconds(5 * (i + 1)));
            }
        }

        throw new HttpRequestException($"Failed to get response after {maxRetries} attempts");
    }

    public static async Task<byte[]> GetBytesWithRetryAsync(string url, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var client = SetupHttpClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(30);
                    await Task.Delay(retryAfter);
                    continue;
                }

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file (Attempt {i + 1}/{maxRetries}): {ex.Message}");
                if (i == maxRetries - 1) throw;
                await Task.Delay(TimeSpan.FromSeconds(5 * (i + 1)));
            }
        }

        throw new HttpRequestException($"Failed to download file after {maxRetries} attempts");
    }

    public static void EnableProxy(string proxyAddress, int port, string username = null, string password = null)
    {
        httpClientHandler.UseProxy = true;
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            httpClientHandler.Proxy = new WebProxy(proxyAddress, port)
            {
                Credentials = new NetworkCredential(username, password)
            };
        }
        else
        {
            httpClientHandler.Proxy = new WebProxy(proxyAddress, port);
        }
    }

    public static void DisableProxy()
    {
        httpClientHandler.UseProxy = false;
        httpClientHandler.Proxy = null;
    }
}