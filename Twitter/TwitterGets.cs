using System.Collections;
public class TwitterGets
{
    private TwitterMotor twitterMotor;
    public TwitterGets(TwitterMotor twitterMotor)
    {
        this.twitterMotor = twitterMotor;
    }

    public async Task<List<string>> GetFollowers(string username, int count = 100)
    {
        var followers = new HashSet<string>();
        var nextPage = true;
        var refreshAttempts = 0;
        const int maxRefreshAttempts = 3;

        try
        {
            // Sayfaya git
            await twitterMotor.GoURL($"https://x.com/{username}/followers");
            await twitterMotor.Page.RandomDelay(2000, 3000);

            while (nextPage && refreshAttempts < maxRefreshAttempts)
            {
                // Sayfadaki tüm kullanıcı adlarını al
                var usernameElements = await twitterMotor.Page.Locator("a").AllInnerTextsAsync();
                var screenNames = usernameElements
                    .Where(t => t.StartsWith("@") && t.Length > 1)
                    .Select(t => t.Trim())
                    .ToList();

                // Eğer sayfa yüklendiyse ve tüm kullanıcılar zaten eklenmişse, işlemi sonlandır
                if (screenNames.Count > 0 && screenNames.All(name => followers.Contains(name)))
                {
                    Console.WriteLine("Tüm kullanıcılar zaten listede, işlem sonlandırılıyor.");
                    return followers.ToList();
                }

                // Yeni kullanıcıları listeye ekle
                foreach (var screenName in screenNames)
                {
                    if (followers.Count >= count) break;
                    followers.Add(screenName);
                }

                // Daha fazla takipçi gerekiyorsa sayfayı yenile
                if (followers.Count < count)
                {
                    refreshAttempts++;
                    Console.WriteLine($"Sayfa yenileme denemesi {refreshAttempts}/{maxRefreshAttempts}");

                    // Sayfayı yenile
                    await twitterMotor.Page.ReloadAsync();
                    await twitterMotor.Page.RandomDelay(2000, 3000);

                    // Yeni kullanıcılar yüklendi mi kontrol et
                    var newUsernameElements = await twitterMotor.Page.Locator("a").AllInnerTextsAsync();
                    var newScreenNames = newUsernameElements
                        .Where(t => t.StartsWith("@") && t.Length > 1)
                        .Select(t => t.Trim())
                        .ToList();

                    if (newScreenNames.Count == 0)
                    {
                        Console.WriteLine("Yeni kullanıcı yüklenmedi, işlem sonlandırılıyor.");
                        nextPage = false;
                    }
                }
                else
                {
                    nextPage = false;
                }
            }

            if (refreshAttempts >= maxRefreshAttempts)
            {
                Console.WriteLine($"Maksimum sayfa yenileme denemesi sayısına ulaşıldı ({maxRefreshAttempts}). Mevcut {followers.Count} takipçi döndürülüyor.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Takipçi listesi alınırken hata oluştu: {ex.Message}");
        }

        return followers.ToList();
    }

}