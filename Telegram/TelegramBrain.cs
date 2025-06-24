using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram
{
    public class TelegramBrain
    {
        string telegramBotToken = "8099844792:AAE1SA2TrNZdCuitkLtSNxnYgeSjYmwvYe0";
        private readonly ITelegramBotClient _botClient;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Dictionary<long, List<NewsData>> _userNewsCache = new();
        private Dictionary<long, int> _selectedNewsIndex = new();
        private Dictionary<long, LoginState> _loginStates = new();
        private string _currentUsername = "";

        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Debug
        }

        private class LoginState
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public int Step { get; set; }
        }

        public TelegramBrain()
        {
            _botClient = new TelegramBotClient(telegramBotToken);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartBot()
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: _cancellationTokenSource.Token
            );

            var me = await _botClient.GetMe();
            await LogMessage($"Bot başlatıldı: @{me.Username}");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;

            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;
            await LogMessage($"Yeni mesaj alındı - Chat ID: {chatId}, Mesaj: {messageText}");

            if (messageText.StartsWith("/login"))
            {
                await LogMessage($"Login işlemi başlatıldı - Chat ID: {chatId}");
                // Login işlemini başlat
                _loginStates[chatId] = new LoginState { Step = 1 };
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Lütfen Twitter kullanıcı adınızı girin:",
                    cancellationToken: cancellationToken);
            }
            else if (messageText.StartsWith("/select"))
            {
                await LogMessage($"Hesap seçimi başlatıldı - Chat ID: {chatId}");
                // Mevcut Twitter kullanıcılarını listele
                var twitterUsers = Program.GetTwitterMotors().Select(x => x.twitterUser.username).ToList();
                
                if (twitterUsers.Count == 0)
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Henüz giriş yapılmış Twitter hesabı bulunmuyor. Önce /login komutu ile giriş yapın.",
                        cancellationToken: cancellationToken);
                    return;
                }

                var userListMessage = "Giriş yapılmış Twitter hesapları:\n\n";
                for (int i = 0; i < twitterUsers.Count; i++)
                {
                    userListMessage += $"{i + 1}) @{twitterUsers[i]}\n";
                }
                userListMessage += "\nSeçmek istediğiniz hesabın numarasını yazın:";

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: userListMessage,
                    cancellationToken: cancellationToken);

                // Kullanıcı seçimini bekle
                _loginStates[chatId] = new LoginState { Step = 4 }; // Step 4: Kullanıcı seçimi
            }
            else if (_loginStates.ContainsKey(chatId))
            {
                var loginState = _loginStates[chatId];
                await LogMessage($"Login durumu işleniyor - Chat ID: {chatId}, Step: {loginState.Step}");
                
                switch (loginState.Step)
                {
                    case 1: // Kullanıcı adı
                        loginState.Username = messageText;
                        
                        // CookieSave klasöründe kayıtlı state var mı kontrol et
                        string cookieFolder = Path.Combine(Directory.GetCurrentDirectory(), "CookieSave");
                        var saveJson = Path.Combine(cookieFolder, $"{loginState.Username}.json");

                        if (File.Exists(saveJson))
                        {
                                      await _botClient.SendMessage(
                                chatId: chatId,
                                text: "Kayıtlı oturum bulundu. Giriş yapılıyor...",
                                cancellationToken: cancellationToken);
                            // Kayıtlı state varsa direkt login yap
                            var savedUser = new SocailMediaUser(loginState.Username, "", ""); // Boş email ve şifre
                            var savedMotor = new TwitterMotor();
                            await savedMotor.Setup(savedUser);

                            // TwitterMotor'u listeye ekle
                            Program.AddTwitterMotor(savedMotor);

                            await _botClient.SendMessage(
                                chatId: chatId,
                                text: "Twitter hesabınıza başarıyla giriş yapıldı!",
                                cancellationToken: cancellationToken);

                            // Login state'i temizle
                            _loginStates.Remove(chatId);
                        }
                        else
                        {
                            // Kayıtlı state yoksa normal login akışına devam et
                            loginState.Step = 2;
                            await _botClient.SendMessage(
                                chatId: chatId,
                                text: "Şimdi Twitter şifrenizi girin:",
                                cancellationToken: cancellationToken);
                        }
                        break;

                    case 2: // Şifre
                        loginState.Password = messageText;
                        loginState.Step = 3;
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "Son olarak Twitter e-posta adresinizi girin:",
                            cancellationToken: cancellationToken);
                        break;

                    case 3: // E-posta
                        loginState.Email = messageText;
                        // Login bilgilerini kullan
                        var twitterUser = new SocailMediaUser(loginState.Username, loginState.Email, loginState.Password);

                        // TwitterMotor'u başlat
                        var twitterMotor = new TwitterMotor();
                        await twitterMotor.Setup(twitterUser);

                        // TwitterMotor'u listeye ekle
                        Program.AddTwitterMotor(twitterMotor);

                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "Twitter hesabınıza başarıyla giriş yapıldı!",
                            cancellationToken: cancellationToken);

                        // Login state'i temizle
                        _loginStates.Remove(chatId);
                        break;

                    case 4: // Kullanıcı seçimi
                        if (int.TryParse(messageText, out int selectedIndex) && 
                            selectedIndex > 0 && 
                            selectedIndex <= Program.GetTwitterMotors().Count)
                        {
                            var selectedUser = Program.GetTwitterMotors()[selectedIndex - 1].twitterUser;
                            _currentUsername = selectedUser.username;
                            
                            await _botClient.SendMessage(
                                chatId: chatId,
                                text: $"@{selectedUser.username} hesabı seçildi. Artık bu hesapla tweet atabilirsiniz.",
                                cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await _botClient.SendMessage(
                                chatId: chatId,
                                text: "Lütfen geçerli bir numara girin.",
                                cancellationToken: cancellationToken);
                        }
                        _loginStates.Remove(chatId);
                        break;
                }
            }
            else if (messageText.StartsWith("/sendnew"))
            {
                await LogMessage($"Yeni haber gönderme isteği - Chat ID: {chatId}");
                var mesaage = "Bir kaynak seçiniz";
                mesaage += "\nBBC";
                mesaage += "\nCNN";
                mesaage += "\nGLOBAL";
                mesaage += "\nTASS";
                mesaage += "\nNEWYORK";

                await botClient.SendMessage(
                    chatId: chatId,
                    text: mesaage,
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("BBC"),
                        new KeyboardButton("CNN"),
                        new KeyboardButton("GLOBAL"),
                        new KeyboardButton("TASS"),
                        new KeyboardButton("NEWYORK")
                    }),
                    cancellationToken: cancellationToken);
            }
            else if (messageText == "BBC")
            {
                await HandleNewsSource(chatId, NewsSource.BBC, cancellationToken);
            }
            else if (messageText == "CNN")
            {
                await HandleNewsSource(chatId, NewsSource.CNN, cancellationToken);
            }
            else if (messageText == "GLOBAL")
            {
                await HandleNewsSource(chatId, NewsSource.GLOBAL, cancellationToken);
            }
            else if (messageText == "TASS")
            {
                await HandleNewsSource(chatId, NewsSource.TASS, cancellationToken);
            }
            else if (messageText == "NEWYORK")
            {
                await HandleNewsSource(chatId, NewsSource.NEWYORK, cancellationToken);
            }
            else if (int.TryParse(messageText, out int selectedIndex) && _userNewsCache.ContainsKey(chatId))
            {
                var news = _userNewsCache[chatId];
                if (selectedIndex > 0 && selectedIndex <= news.Count)
                {
                    _selectedNewsIndex[chatId] = selectedIndex;
                    var selectedNews = news[selectedIndex - 1];

                    // Haberin daha önce paylaşılıp paylaşılmadığını kontrol et
                    if (Checkers.HasUserSharedNews(_currentUsername, selectedNews.newUrl))
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "Bu haber daha önce paylaşılmış. Lütfen başka bir haber seçin.",
                            cancellationToken: cancellationToken);
                        return;
                    }

                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"Başlık: {selectedNews.Title}",
                        cancellationToken: cancellationToken);

                    // Tweet atılacağını belirten mesaj
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Lütfen bekleyin, tweet atılıyor...",
                        cancellationToken: cancellationToken);

                    var twitterMotor = Program.GetTwitterMotorByUsernameAsync(_currentUsername).Result;
                    var response = await Gemini.AskGeminiAsync(selectedNews.Description,twitterMotor.useTagOnPrompt);
                    if (response == "null")
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "Haber metni boş olamaz.",
                            cancellationToken: cancellationToken);
                        return;
                    }
                    if (string.IsNullOrEmpty(_currentUsername))
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "Önce /select komutu ile bir Twitter hesabı seçmelisiniz.",
                            cancellationToken: cancellationToken);
                        return;
                    }


                    if(_currentUsername == null)
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "Önce /select komutu ile bir Twitter hesabı seçmelisiniz.",
                            cancellationToken: cancellationToken);
                        return;
                    }
                    var tweet = await Program.GetTwitterMotorByUsernameAsync(_currentUsername).Result.TwitterTweetAction.SendTweetAsync(response);

                    if (tweet)
                    {
                        // Tweet başarılı olduğunda haberi kaydet
                        Checkers.AddSharedNews(_currentUsername, selectedNews.newUrl);
                        
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "Tweet atıldı.",
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "HATA!!! Tweet atılamadı.",
                            cancellationToken: cancellationToken);

                        _userNewsCache.Remove(chatId); // Seçim yapıldıktan sonra cache'i temizle
                    }
                }
                else
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Lütfen geçerli bir haber numarası girin.",
                        cancellationToken: cancellationToken);
                }
            }
            
            
            else if (messageText.StartsWith("/usetag"))
            {
                await LogMessage($"Tag kullanımı değiştiriliyor - Chat ID: {chatId}, Username: {_currentUsername}");
                if (string.IsNullOrEmpty(_currentUsername))
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Önce /select komutu ile bir Twitter hesabı seçmelisiniz.",
                        cancellationToken: cancellationToken);
                    return;
                }

                var twitterMotor = await Program.GetTwitterMotorByUsernameAsync(_currentUsername);
                twitterMotor.useTagOnPrompt = !twitterMotor.useTagOnPrompt;

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: $"Tag kullanımı {(twitterMotor.useTagOnPrompt ? "açıldı" : "kapatıldı")}.",
                    cancellationToken: cancellationToken);
            }
            else if (messageText.StartsWith("/follow"))
            {
                await LogMessage($"Takip etme isteği - Chat ID: {chatId}, Username: {_currentUsername}");
                
                if (string.IsNullOrEmpty(_currentUsername))
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Önce /select komutu ile bir Twitter hesabı seçmelisiniz.",
                        cancellationToken: cancellationToken);
                    return;
                }

                var usernameToFollow = messageText.Substring(7).Trim();
                if (string.IsNullOrEmpty(usernameToFollow))
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Lütfen takip etmek istediğiniz kullanıcı adını yazın. Örnek: /follow username",
                        cancellationToken: cancellationToken);
                    return;
                }

                try
                {
                    var twitterMotor = await Program.GetTwitterMotorByUsernameAsync(_currentUsername);
                    await twitterMotor.TwitterFollowAction.FollowUserAsync(usernameToFollow);
                    
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"@{usernameToFollow} kullanıcısı takip edildi.",
                        cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    await LogMessage($"Takip etme hatası: {ex.Message}", LogLevel.Error);
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Kullanıcı takip edilirken bir hata oluştu.",
                        cancellationToken: cancellationToken);
                }
            }
        }


        private async Task HandleNewsSource(long chatId, NewsSource source, CancellationToken cancellationToken)
        {
            await LogMessage($"Haber kaynağı işleniyor - Chat ID: {chatId}, Kaynak: {source}");
            await _botClient.SendMessage(
                chatId: chatId,
                text: $"{source} haberleri getiriliyor...",
                cancellationToken: cancellationToken);

            // Haber kaynağına göre scraper seç
            object newsScraper;
            List<string> newsLinks;
            List<NewsData> news;

            switch (source)
            {
                case NewsSource.BBC:
                    newsScraper = new News_BBC();
                    newsLinks = await ((News_BBC)newsScraper).GetNewsLinksAsync();
                    news = await ((News_BBC)newsScraper).GetNewsAsync(newsLinks);
                    break;

                case NewsSource.CNN:
                    newsScraper = new News_CNN();
                    newsLinks = await ((News_CNN)newsScraper).GetNewsLinksAsync();
                    news = await ((News_CNN)newsScraper).GetNewsAsync(newsLinks);
                    break;

                case NewsSource.GLOBAL:
                    newsScraper = new News_GLOBAL();
                    newsLinks = await ((News_GLOBAL)newsScraper).GetNewsLinksAsync();
                    news = await ((News_GLOBAL)newsScraper).GetNewsAsync(newsLinks);
                    break;

                case NewsSource.TASS:
                    newsScraper = new News_TASS();
                    newsLinks = await ((News_TASS)newsScraper).GetNewsLinksAsync();
                    news = await ((News_TASS)newsScraper).GetNewsAsync(newsLinks);
                    break;

                case NewsSource.NEWYORK:
                    newsScraper = new News_NEWYORK();
                    newsLinks = await ((News_NEWYORK)newsScraper).GetNewsLinksAsync();
                    news = await ((News_NEWYORK)newsScraper).GetNewsAsync(newsLinks);
                    break;

                default:
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Geçersiz haber kaynağı.",
                        cancellationToken: cancellationToken);
                    return;
            }

            // Haberleri cache'le
            _userNewsCache[chatId] = news;

            // Haberleri kullanıcıya gönder
            for (int i = 0; i < news.Count; i++)
            {
                var newsItem = news[i];
                var numberedTitle = $"{i + 1}) {newsItem.Title}";
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: numberedTitle,
                    cancellationToken: cancellationToken);
            }

            // Kullanıcıya seçim yapması için mesaj gönder
            await _botClient.SendMessage(
                chatId: chatId,
                text: "Haber detayını görmek için numarasını yazın (1-" + news.Count + ")",
                cancellationToken: cancellationToken);
        }

        private async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            await LogMessage($"Hata oluştu: {errorMessage}", LogLevel.Error);
        }

        public void StopBot()
        {
            _cancellationTokenSource.Cancel();
            _ = LogMessage("Bot durduruldu");
        }

        public async Task LogMessage(string message, LogLevel level = LogLevel.Info)
        {
            try
            {
                // Log klasörünü oluştur
                string logFolder = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                // Log dosyası adını oluştur (günlük olarak)
                string logFile = Path.Combine(logFolder, $"bot_log_{DateTime.Now:yyyy-MM-dd}.txt");

                // Log mesajını oluştur
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

                // Konsola yazdır
                Console.WriteLine(logEntry);

                // Dosyaya yaz
                await File.AppendAllTextAsync(logFile, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log yazma hatası: {ex.Message}");
            }
        }
    }
}
