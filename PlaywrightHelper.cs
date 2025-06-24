using Microsoft.Playwright;

public static class PlaywrightHelper
{
    public static async Task<ILocator> FindElement(this IPage page, string elementId)
    {
        var element = page.Locator(elementId).First;
        await element.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        return element;
    }

    public static async Task<bool> HumanLikeClick(this IPage page, ILocator element)
    {
        try
        {
            // Elementin görünür olmasını bekle
            await element.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Rastgele scroll simülasyonu
            await page.SimulateScroll(1, 2);

            // Fare hareketini simüle et
            await page.SimulateMouseMovement(element);

            // Tıklamadan önce rastgele bekle
            await RandomDelay(300, 800);

            // Tıklama ve kontrol
            bool clicked = false;
            int maxAttempts = 3;
            int attempt = 0;

            while (!clicked && attempt < maxAttempts)
            {
                try
                {
                    await element.ClickAsync(new() { Force = true });
                    clicked = true;
                }
                catch
                {
                    attempt++;
                    await RandomDelay(200, 400);
                }
            }

            if (!clicked)
            {
                await Program.telegramBrain.LogMessage($"Element tıklanamadı: {element}", Telegram.TelegramBrain.LogLevel.Error);
                return false;
            }

            // Tıklama sonrası kısa bekle
            await RandomDelay(200, 500);
            return true;
        }
        catch (Exception ex)
        {
            await Program.telegramBrain.LogMessage($"HumanLikeClick hatası: {ex.Message}", Telegram.TelegramBrain.LogLevel.Error);
            return false;
        }
    }

    public static async Task RandomDelay(int minMs = 500, int maxMs = 2000)
    {
        var random = new Random();
        await Task.Delay(random.Next(minMs, maxMs));
    }

    public static async Task RandomDelay(this IPage page, int minMs = 500, int maxMs = 2000)
    {
        var random = new Random();
        await Task.Delay(random.Next(minMs, maxMs));
    }


    public static async Task SetupFingerprint(this IPage page)
    {
        var random = new Random();
        // Rastgele ekran çözünürlüğü
        var screenSizes = new[]
        {
            new { Width = 1920, Height = 1080 },
            new { Width = 1366, Height = 768 },
            new { Width = 1440, Height = 900 },
            new { Width = 1536, Height = 864 }
        };
        var screenSize = screenSizes[random.Next(screenSizes.Length)];

        // Rastgele user agent
        var userAgents = new[]
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36"
        };
        var userAgent = userAgents[random.Next(userAgents.Length)];

        // Rastgele dil ve zaman dilimi
        var locales = new[] { "en-US", "en-GB", "tr-TR" };
        var timezones = new[] { "Europe/Istanbul", "Europe/London", "America/New_York" };
        var locale = locales[random.Next(locales.Length)];
        var timezone = timezones[random.Next(timezones.Length)];

        // Tarayıcı özelliklerini ayarla
        await page.AddInitScriptAsync(@"
            Object.defineProperty(navigator, 'webdriver', {
                get: () => undefined
            });
            Object.defineProperty(navigator, 'plugins', {
                get: () => [1, 2, 3, 4, 5]
            });
            Object.defineProperty(navigator, 'languages', {
                get: () => ['en-US', 'en']
            });
        ");

        // Viewport ve user agent ayarla
        await page.SetViewportSizeAsync(screenSize.Width, screenSize.Height);
        await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
        {
            ["Accept-Language"] = locale,
            ["User-Agent"] = userAgent
        });

        // JavaScript ile ek özellikler
        await page.EvaluateAsync(@"
            () => {
                // Canvas parmak izini değiştir
                const originalGetContext = HTMLCanvasElement.prototype.getContext;
                HTMLCanvasElement.prototype.getContext = function(type) {
                    const context = originalGetContext.apply(this, arguments);
                    if (type === '2d') {
                        const originalGetImageData = context.getImageData;
                        context.getImageData = function() {
                            const imageData = originalGetImageData.apply(this, arguments);
                            // Rastgele piksel değişiklikleri
                            for (let i = 0; i < imageData.data.length; i += 4) {
                                imageData.data[i] += Math.random() * 2 - 1;
                            }
                            return imageData;
                        };
                    }
                    return context;
                };
            }
        ");
    }

    public static async Task SimulateMouseMovement(this IPage page, ILocator targetElement)
    {
        try
        {
            var random = new Random();
            // Hedef elementin konumunu al
            var box = await targetElement.BoundingBoxAsync();
            if (box == null) return;

            // Başlangıç noktası (ekranın rastgele bir yerinden)
            var startX = random.Next(0, 100);
            var startY = random.Next(0, 100);

            // Hedef nokta (elementin içinde rastgele bir nokta)
            var targetX = box.X + random.Next(10, (int)box.Width - 10);
            var targetY = box.Y + random.Next(10, (int)box.Height - 10);

            // Bezier eğrisi için kontrol noktaları
            var controlX1 = startX + (targetX - startX) / 3 + random.Next(-50, 50);
            var controlY1 = startY + (targetY - startY) / 3 + random.Next(-50, 50);
            var controlX2 = startX + 2 * (targetX - startX) / 3 + random.Next(-50, 50);
            var controlY2 = startY + 2 * (targetY - startY) / 3 + random.Next(-50, 50);

            // Hareket adımları
            var steps = random.Next(20, 30);
            for (var i = 0; i <= steps; i++)
            {
                var t = i / (float)steps;
                // Bezier eğrisi formülü
                var x = Math.Pow(1 - t, 3) * startX +
                       3 * Math.Pow(1 - t, 2) * t * controlX1 +
                       3 * (1 - t) * Math.Pow(t, 2) * controlX2 +
                       Math.Pow(t, 3) * targetX;
                var y = Math.Pow(1 - t, 3) * startY +
                       3 * Math.Pow(1 - t, 2) * t * controlY1 +
                       3 * (1 - t) * Math.Pow(t, 2) * controlY2 +
                       Math.Pow(t, 3) * targetY;

                await page.Mouse.MoveAsync((float)x, (float)y);
                await RandomDelay(10, 30); // Her adımda kısa bekleme
            }
        }
        catch (Exception ex)
        {
            await Program.telegramBrain.LogMessage($"Fare hareketi simülasyonu hatası: {ex.Message}", Telegram.TelegramBrain.LogLevel.Error);
        }
    }

    public static async Task SimulateScroll(this IPage page, int minScrolls = 1, int maxScrolls = 3)
    {
        try
        {
            var random = new Random();
            var scrollCount = random.Next(minScrolls, maxScrolls + 1);
            for (int i = 0; i < scrollCount; i++)
            {
                // Rastgele scroll mesafesi (-300 ile 300 piksel arası)
                var scrollAmount = random.Next(-300, 301);

                // Scroll hızını simüle et (küçük adımlarla)
                var steps = random.Next(5, 10);
                var stepSize = scrollAmount / steps;

                for (int step = 0; step < steps; step++)
                {
                    await page.Mouse.WheelAsync(0, stepSize);
                    await RandomDelay(50, 150); // Her adımda kısa bekleme
                }

                // Scroll sonrası kısa bekleme
                await RandomDelay(500, 1500);
            }
        }
        catch (Exception ex)
        {
            await Program.telegramBrain.LogMessage($"Scroll simülasyonu hatası: {ex.Message}", Telegram.TelegramBrain.LogLevel.Error);
        }
    }

    public static async Task TypeLikeHuman(this IPage page, ILocator element, string message)
    {
        var random = new Random();
        // Elemente odaklan
        await element.FocusAsync();

        // Mesajı harf harf yaz
        foreach (char c in message)
        {
            await page.Keyboard.PressAsync(c.ToString());
            await Task.Delay(random.Next(50, 200)); // Harfler arası rastgele gecikme
        }

        // Yazma sonrası rastgele bekleme
        await Task.Delay(random.Next(500, 1500));

        // Input ve change olaylarını tetikle
        await element.EvaluateAsync(@"element => {
        element.dispatchEvent(new Event('input', { bubbles: true }));
        element.dispatchEvent(new Event('change', { bubbles: true }));
    }");
    }


    public static async Task<bool> PageLoaded(this IPage page)
    {
        try
        {
            // Ağ trafiği durana kadar bekle
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10000 });

            // Sayfa yüklendiğini gösteren önemli bir elementin gelmesini bekle
            // Örn: Profil butonu (giriş sonrası)
            return true;
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Sayfa yüklenemedi veya beklenen element bulunamadı.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sayfa yükleme kontrolünde hata: {ex.Message}");
            return false;
        }
    }

}