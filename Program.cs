using MrBladon;
using Telegram;

public class Program
{
    private static List<TwitterMotor> TwitterMotors = new();
    public static TelegramBrain telegramBrain;
    public static async Task Main(string[] args)
    {
        //await TelegramTest();

        // return;
        // PhotoEditTest();

        // var instagramMotor = new InstagramMotor();
        // var instagramLogin = new InstagramLogin(instagramMotor);
        // await instagramLogin.LoginAsync("burjuvaitr", "mod123456");
        // return;
        
        //Speech.GetSpeechTextAsync("Merhaba, bu bir test mesajıdır. Ses dosyası başarıyla oluşturuldu.").GetAwaiter().GetResult();
        //PhotoScarper.ExtractImageSources().GetAwaiter().GetResult();
    
    }

    private static void PhotoEditTest()
    {
        PhotoEdit photoEdit = new PhotoEdit();
        var newText = "Yeni uydu görüntüleri, İran'ın Natanz ve İsfahan gibi kilit nükleer tesisleri ile füze üslerinde saldırı sonrası oluşan hasarı doğrulayarak olayın boyutunu gözler önüne serdi.";
        photoEdit.EditPhotoForInstagram("Assets/photo.png", newText);
        return;
    }

    private static async Task TelegramTest()
    {
        telegramBrain = new TelegramBrain();
        await telegramBrain.StartBot();
        Console.WriteLine("Bot başlatıldı. Durdurmak için bir tuşa basın.");
        Console.ReadKey();
    }

    public static async Task<TwitterMotor> GetTwitterMotorByUsernameAsync(string username)
    {
        var first = TwitterMotors.FirstOrDefault(x => x.twitterUser.username == username);
        if (first == null)
        {
            Console.WriteLine($"Kullanıcı adı '{username}' olan TwitterMotor bulunamadı.");
            return null;
        }

        return first;
    }

    public static List<TwitterMotor> GetTwitterMotors()
    {
        return TwitterMotors;
    }

    public static void AddTwitterMotor(TwitterMotor motor)
    {
        // Eğer aynı kullanıcı adına sahip bir hesap varsa, onu kaldır
        var existingMotor = TwitterMotors.FirstOrDefault(x => x.twitterUser.username == motor.twitterUser.username);
        if (existingMotor != null)
        {
            TwitterMotors.Remove(existingMotor);
        }

        // Yeni hesabı ekle
        TwitterMotors.Add(motor);
    }
}