using Microsoft.Playwright;

public class TwitterMotor
{
    IPage page;
    public IPage Page => page;
    private IBrowser browser;
    public IBrowser Browser => browser;
    private IPlaywright playwright;
    public SocailMediaUser twitterUser;
    public bool useTagOnPrompt { get; set; } = false;


    private TwitterFollowActions twitterFollowActions;
    public TwitterFollowActions TwitterFollowAction { get { return twitterFollowActions; } }

    private TwitterLogin twitterLogin;
    public TwitterLogin TwitterLogin => twitterLogin;
    private TwitterTweetActions twitterTweetActions;
    public TwitterTweetActions TwitterTweetAction { get { return twitterTweetActions; } }
    
    public async Task<IPage> Setup(SocailMediaUser twitterUser)
    {
        this.twitterUser = twitterUser;
        playwright = await Playwright.CreateAsync();
        BrowserTypeLaunchOptions launchOptions;
        twitterFollowActions = new TwitterFollowActions(this);
        twitterLogin = new TwitterLogin(this);
        
        if (twitterUser.proxy != null)
        {
            launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = true,
                Proxy = new Proxy
                {
                    Server = twitterUser.userProxy.GetServerString(),
                    Username = twitterUser.userProxy.username,
                    Password = twitterUser.userProxy.password
                }
            };
        }
        else
        {
            launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = true
            };
        }

        browser = await playwright.Chromium.LaunchAsync(launchOptions);
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
            Locale = "en-US",
            TimezoneId = "Europe/Istanbul",
            HasTouch = false,
            IsMobile = false,
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Light
        });

        page = await context.NewPageAsync();
        await page.SetupFingerprint();

        await twitterLogin.LoginAndSaveStateAsync();
        return page;
    }

   

    public async Task<IPage> GetPage()
    {
        return page;
    }



    public async Task<bool> GoHomePage()
    {
        return await GoURL("https://x.com/home");
    }

    public async Task<bool> GoProfile()
    {
        return await GoURL($"https://x.com/{twitterUser.username}");
    }

    public async Task<bool> GoProfileByButton()
    {
        var profileLink = page.Locator("a[data-testid='AppTabBar_Profile_Link']");
        await page.HumanLikeClick(profileLink);
        var loaded = await page.PageLoaded();
        await page.RandomDelay(1000, 2000);
        return loaded;
    }

    public async Task<bool> GoURL(string url)
    {
        await page.GotoAsync(url);
        var loaded = await page.PageLoaded();
        await page.RandomDelay(1000, 2000); // Sayfa yüklendikten sonra biraz bekle
        return loaded;
    }

    public void SetPage(IPage page)
    {
        this.page = page;
    }
}
