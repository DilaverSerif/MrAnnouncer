using Microsoft.Playwright;

public class TwitterLogin
{
    private TwitterMotor twitterMotor;
    public TwitterLogin(TwitterMotor twitterMotor)
    {
        this.twitterMotor = twitterMotor;
    }

    public async Task LoginAndSaveStateAsync()
    {
        // Klasör yolu
        string cookieFolder = Path.Combine(Directory.GetCurrentDirectory(), "CookieSave");

        // Klasör yoksa oluştur
        if (!Directory.Exists(cookieFolder))
        {
            Directory.CreateDirectory(cookieFolder);
        }

        var saveJson = Path.Combine(cookieFolder, $"{twitterMotor.twitterUser.username}.json");

        if (File.Exists(saveJson))
        {
            // State dosyasının içeriğini oku
            var stateJson = await File.ReadAllTextAsync(saveJson);
            
            // Mevcut context'i kapat
            if (twitterMotor.Page != null)
            {
                await twitterMotor.Page.CloseAsync();
                await twitterMotor.Page.Context.CloseAsync();
            }

            // Yeni context oluştur ve state'i yükle
            var context = await twitterMotor.Browser.NewContextAsync(new() { StorageState = stateJson });
            twitterMotor.SetPage(await context.NewPageAsync());
            
            // Twitter'a git
            await twitterMotor.Page.GotoAsync("https://x.com/home");
            await twitterMotor.Page.PageLoaded();
            await twitterMotor.Page.RandomDelay(1000, 2000);
        }
        else
        {
            await LoginAsync(twitterMotor.Page);
            await twitterMotor.Page.Context.StorageStateAsync(new() { Path = saveJson });
        }

        Console.WriteLine($"Oturum durumu şuraya kaydedildi: {cookieFolder}/{twitterMotor.twitterUser.username}.json");
    }

    public async Task LoginAsync(IPage page)
    {
        await page.GotoAsync("https://x.com/login");
        await page.PageLoaded();
        await page.RandomDelay(1000, 2000);

        // Kullanıcı adı/email
        var usernameInput = page.Locator("input[name='text']");
        await usernameInput.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        Console.WriteLine($"Mail Adresi Giriliyor: {twitterMotor.twitterUser.email}");
        await page.TypeLikeHuman(usernameInput, twitterMotor.twitterUser.email);
        await page.RandomDelay(500, 1000);

        // Next / İleri butonu
        var nextButton = page.Locator("button:has-text('İleri'), button:has-text('Next')");
        await page.HumanLikeClick(nextButton);
        await page.RandomDelay(1000, 2000);

        // Şimdi hangi alanın geldiğini kontrol et
        var passwordInput = page.Locator("input[name='password']");
        var usernameInput2 = page.Locator("input[name='text']");

        try
        {
            // Önce şifre alanının gelip gelmediğini kontrol et
            var isPasswordField = await passwordInput.IsVisibleAsync(new() { Timeout = 5000 });
            
            if (isPasswordField)
            {
                // Direkt şifre alanı geldi
                Console.WriteLine($"Şifre Giriliyor: {twitterMotor.twitterUser.password}");
                await page.TypeLikeHuman(passwordInput, twitterMotor.twitterUser.password);
            }
            else
            {
                // Kullanıcı adı alanı geldi
                Console.WriteLine($"Kullanıcı Adı Giriliyor: {twitterMotor.twitterUser.username}");
                await page.TypeLikeHuman(usernameInput2, twitterMotor.twitterUser.username);
                await page.RandomDelay(500, 1000);

                // Tekrar ileri butonuna tıkla
                nextButton = page.Locator("button:has-text('İleri'), button:has-text('Next')");
                await page.HumanLikeClick(nextButton);
                await page.RandomDelay(1000, 2000);

                // Şimdi şifre alanını bekle
                await passwordInput.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
                Console.WriteLine($"Şifre Giriliyor: {twitterMotor.twitterUser.password}");
                await page.TypeLikeHuman(passwordInput, twitterMotor.twitterUser.password);
            }

            await page.RandomDelay(500, 1000);

            // Giriş yap
            var loginButton = page.Locator("button[data-testid='LoginForm_Login_Button']");
            await page.HumanLikeClick(loginButton);

            await page.PageLoaded();
            await page.RandomDelay(2000, 4000); // Giriş sonrası biraz bekle
        }
        catch (Exception ex)
        {
            _ = Program.telegramBrain.LogMessage($"Login sırasında bir hata oluştu: {ex.Message}",Telegram.TelegramBrain.LogLevel.Error);
            throw;
        }
    }
    
}