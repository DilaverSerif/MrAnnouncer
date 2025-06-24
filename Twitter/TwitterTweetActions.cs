using Microsoft.Playwright;

public class TwitterTweetActions
{
    private TwitterMotor twitterMotor;
    public TwitterTweetActions(TwitterMotor twitterMotor)
    {
        this.twitterMotor = twitterMotor;
    }


     public async Task<bool> SendTweetAsync(string message)
    {
        try
        {
            await twitterMotor.GoHomePage();
            await twitterMotor.Page.RandomDelay(1000, 2000);

            // Tweet kutusunu bul ve tıkla
            var tweetBox = twitterMotor.Page.Locator("[data-testid='tweetTextarea_0']");
            await tweetBox.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await twitterMotor.Page.HumanLikeClick(tweetBox);

            // İnsan gibi yaz
            await twitterMotor.Page.TypeLikeHuman(tweetBox, message);
            await twitterMotor.Page.RandomDelay(500, 1000);

            // "Post" butonunu bul ve tıkla
            var postButton = twitterMotor.Page.Locator("[data-testid='tweetButtonInline']");
            await postButton.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 5000 });

            // Butona tıklama işlemini 3 kez dene
            for (int i = 0; i < 3; i++)
            {
                if (await postButton.IsEnabledAsync())
                {
                    await twitterMotor.Page.HumanLikeClick(postButton);
                    await twitterMotor.Page.RandomDelay(2000, 3000); // Tweet'in yüklenmesi için bekle

                    // Tweet'in timeline'da görünüp görünmediğini kontrol et
                    try
                    {
                        // Timeline'daki son tweet'i bul
                        var timelineTweet = twitterMotor.Page.Locator("article[data-testid='tweet']").First;
                        await timelineTweet.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

                        // Tweet içeriğini kontrol et
                        var tweetText = await timelineTweet.Locator("[data-testid='tweetText']").TextContentAsync();
                        if (tweetText != null && tweetText.Contains(message))
                        {
                            Console.WriteLine("Tweet başarıyla gönderildi ve timeline'da görünüyor.");
                            await twitterMotor.Page.RandomDelay(2000, 4000);
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"Tweet gönderimi başarısız oldu. Deneme {i + 1}/3");
                            await twitterMotor.Page.RandomDelay(1000, 2000);
                            continue;
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"Tweet timeline'da görünmedi. Deneme {i + 1}/3");
                        await twitterMotor.Page.RandomDelay(1000, 2000);
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"Tweet butonu aktif değil. Deneme {i + 1}/3");
                    await twitterMotor.Page.RandomDelay(1000, 2000);
                }
            }

            Console.WriteLine("Tweet gönderimi başarısız oldu. Maksimum deneme sayısına ulaşıldı.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Tweet atma sırasında bir hata oluştu: {ex.Message}");
            return false;
        }
    }
    
}