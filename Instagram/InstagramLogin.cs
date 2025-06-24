using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;

public class InstagramLogin
{
    IResult<bool> loggedIn;
    private static IInstaApi InstaApi;
    private InstagramMotor instagramMotor;
    public InstagramLogin(InstagramMotor instagramMotor)
    {
        this.instagramMotor = instagramMotor;
    }
    UserSessionData userSession;

    public async Task<bool> LoginAsync(string username, string password)
    {

            userSession = new UserSessionData
    {
        UserName = username,
        Password = password
    };

    var delay = RequestDelay.FromSeconds(2, 2);
    InstaApi = InstaApiBuilder.CreateBuilder()
        .SetUser(userSession)
        .UseLogger(new DebugLogger(LogLevel.None))
        .SetRequestDelay(delay)
        .Build();

    var loginResult = await InstaApi.LoginAsync();

    if (loginResult.Succeeded)
    {
        Console.WriteLine("Login başarılı.");
        return true;
    }

    if (loginResult.Value == InstaLoginResult.ChallengeRequired)
    {
        Console.WriteLine("Güvenlik doğrulaması (challenge) gerekli!");

        var challenge = await InstaApi.GetChallengeRequireVerifyMethodAsync();

        if (!challenge.Succeeded)
        {
            Console.WriteLine("Doğrulama yöntemi alınamadı.");
            return false;
        }

        // Telefon mu e-posta mı kullanılsın?
        if (challenge.Value.SubmitPhoneRequired)
        {
            Console.WriteLine("Telefon doğrulaması gerekiyor.");
            await InstaApi.RequestVerifyCodeToSMSForChallengeRequireAsync();
        }
        else
        {
            Console.WriteLine("E-posta doğrulaması gönderiliyor.");
            await InstaApi.RequestVerifyCodeToEmailForChallengeRequireAsync();
        }

        Console.Write("Kod: ");
        var code = Console.ReadLine();

        var verifyResult = await InstaApi.VerifyCodeForChallengeRequireAsync(code);
        if (verifyResult.Succeeded && InstaApi.IsUserAuthenticated)
        {
            Console.WriteLine("Doğrulama başarılı, giriş tamamlandı.");
            return true;
        }
        else
        {
            Console.WriteLine("Doğrulama başarısız.");
            return false;
        }
    }

    Console.WriteLine($"Giriş başarısız: {loginResult.Info.Message}");
    return false;
    }

}