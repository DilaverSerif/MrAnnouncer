using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MrBladon
{
    public class Speech
    {
        static string apiKey = "sk_7a4dd43fa016f183dbdc88668b4c3a166f032c513935b1c5";
        static string voiceId = "mnEe2Jhwlupp6oZEDi3k";
        public static async Task<string> GetSpeechTextAsync(string text)
        {
            using var client = new HttpClient();
            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Add("xi-api-key", apiKey);

            var requestBody = new
            {
                text,
                model_id = "eleven_flash_v2_5",
                language_code = "tr",
                voice_settings = new
                {
                    stability = 0.5,
                    similarity_boost = 0.5
                }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(
                $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}", content);

            if (response.IsSuccessStatusCode)
            {
                byte[] audioBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync("output.mp3", audioBytes);
                Console.WriteLine("Ses başarıyla kaydedildi: output.mp3");
                return "output.mp3";
            }

            Console.WriteLine($"Hata: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            throw new Exception($"API isteği başarısız oldu: {response.StatusCode}");
        }
    }
}