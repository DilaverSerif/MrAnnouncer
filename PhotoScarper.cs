using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;

namespace MrBladon
{
    public class PhotoScarper
    {
        private static readonly IImageHash hashAlgorithm = new PerceptualHash();
        private static readonly HttpClient client = new();
        private const int MinImageWidth = 1024; // Minimum width for large images
        private const int MinImageHeight = 1024; // Minimum height for large images
        private const string SaveDirectory = "DownloadedImages"; // Directory to save images
        private const string ImageUrlsFilePath = "image_urls.txt"; // File containing image URLs
        private static List<string> imageUrls = new List<string>();

        public static async Task ExtractImageSources()
        {
            string apiKey = "AIzaSyBA4S48Kad4taNWtIsWozy9_m8k0o9vDe0";
            string searchEngineId = "9522596d2d7e645fb";
            string query = "kedi";

            string requestUri = $"https://www.googleapis.com/customsearch/v1?key={apiKey}&cx={searchEngineId}&q={query}&searchType=image";

            HttpResponseMessage response = await client.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();

                using JsonDocument jsonDoc = JsonDocument.Parse(jsonString);
                JsonElement root = jsonDoc.RootElement;

                if (root.TryGetProperty("items", out JsonElement items))
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        string imageLink = item.GetProperty("link").GetString();
                        Console.WriteLine(imageLink);
                        imageUrls.Add(imageLink);
                    }

                    // Save image URLs to a file
                    File.WriteAllLines(ImageUrlsFilePath, imageUrls);
                    Console.WriteLine($"Toplam {imageUrls.Count} görsel bulundu ve '{ImageUrlsFilePath}' dosyasına kaydedildi.");
                    // Download large images
                    await DownloadLargeImagesAsync(imageUrls.ToArray(), MinImageWidth, MinImageHeight, SaveDirectory);
                }
                else
                {
                    Console.WriteLine("Görsel bulunamadı.");
                }
            }
            else
            {
                Console.WriteLine($"API isteği başarısız oldu: {response.StatusCode}");
            }
        }

        public static async Task<ulong> GetImageHashAsync(string imagePath)
        {
            using var image = await Image.LoadAsync<Rgba32>(imagePath);
            return hashAlgorithm.Hash(image);
        }

        public static int CalculateHammingDistance(ulong hash1, ulong hash2)
        {
            ulong xorResult = hash1 ^ hash2;
            int count = 0;
            while (xorResult > 0)
            {
                count += (int)(xorResult & 1);
                xorResult >>= 1;
            }
            return count;
        }

        public static double CalculateSimilarity(ulong hash1, ulong hash2)
        {
            int distance = CalculateHammingDistance(hash1, hash2);
            return 1.0 - (distance / 64.0);  // 64 bitlik hash için
        }

        public static async Task DownloadLargeImagesAsync(string[] imageUrls, int minWidth, int minHeight, string saveDirectory)
        {
            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);

            foreach (var url in imageUrls)
            {
                try
                {
                    using var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var image = await Image.LoadAsync<Rgba32>(stream);

                    if (image.Width >= minWidth && image.Height >= minHeight)
                    {
                        string fileName = Path.Combine(saveDirectory, Path.GetFileName(new Uri(url).AbsolutePath));

                        await image.SaveAsync(fileName);

                        var imagesOnDisk = Directory.GetFiles(saveDirectory, "*.png");
                        foreach (var item in imagesOnDisk)
                        {
                            if (item != fileName)
                            {
                                ulong existingHash = await GetImageHashAsync(item);
                                ulong newHash = await GetImageHashAsync(fileName);

                                double similarity = CalculateSimilarity(existingHash, newHash);
                                if (similarity > 0.7) // 70% benzerlik
                                {
                                    Console.WriteLine($"Benzer görsel bulundu: {item} ile {fileName} ({similarity:P2})");
                                    File.Delete(fileName); // Yeni görseli sil
                                    return; // Benzer görsel bulundu, yeni görseli kaydetme
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Aynı görsel zaten var: {fileName}");
                                File.Delete(fileName); // Aynı görseli sil
                                return; // Aynı görsel zaten var, kaydetme
                            }
                        }

                        Console.WriteLine($"İndirildi: {fileName} ({image.Width}x{image.Height})");
                    }
                    else
                    {
                        Console.WriteLine($"Atlandı (küçük boyut): {url} ({image.Width}x{image.Height})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata ({url}): {ex.Message}");
                }
            }
        }
    }
}