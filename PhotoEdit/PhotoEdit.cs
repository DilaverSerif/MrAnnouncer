using System;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Collections.Generic;
using System.Linq;

public class PhotoEdit
{



    public void EditPhotoForInstagram(string imagePath, string text)
    {
        string framePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/frame.png");
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/output.png");
        string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/font.ttf"); // Font dosyasını buraya koy

        using (Image<Rgba32> frame = Image.Load<Rgba32>(framePath))
        using (Image<Rgba32> photo = Image.Load<Rgba32>(imagePath))
        using (Image<Rgba32> canvas = new Image<Rgba32>(frame.Width, frame.Height))
        {
            // Fotoğrafı çerçeve boyutuna göre yeniden boyutlandır
            Image<Rgba32> resizedPhoto = photo.Clone(ctx => ctx.Resize(frame.Width, frame.Height));

            // Önce arka plana haber fotoğrafı
            canvas.Mutate(ctx => ctx.DrawImage(resizedPhoto, 1f));

            // Üzerine çerçeveyi çiz
            canvas.Mutate(ctx => ctx.DrawImage(frame, 1f));

            // Font yükle
            FontCollection fonts = new FontCollection();
            FontFamily family = fonts.Add(fontPath);
            
            // Başlangıç font boyutu
            float fontSize = 30;
            Font font = family.CreateFont(fontSize, FontStyle.Bold);

            // Metin ayarları
            var startX = frame.Width / 6f;
            var startY = frame.Height - 190; // Başlangıç noktasını yukarı çek
            var maxTextWidth = frame.Width - startX * 2; // Sağdan da eşit boşluk bırakmak için
            var maxTextHeight = 200; // Maksimum yüksekliği artır

            // Metni satırlara böl
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = currentLine + (currentLine == "" ? "" : " ") + word;
                var size = TextMeasurer.MeasureBounds(testLine, new TextOptions(font));
                
                if (size.Width > maxTextWidth)
                {
                    if (currentLine != "")
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }
                    else
                    {
                        lines.Add(word);
                        currentLine = "";
                    }
                }
                else
                {
                    currentLine = testLine;
                }
            }
            if (currentLine != "")
            {
                lines.Add(currentLine);
            }

            // Font boyutunu ayarla
            while (true)
            {
                var totalHeight = lines.Sum(line => TextMeasurer.MeasureBounds(line, new TextOptions(font)).Height);
                if (totalHeight <= maxTextHeight && fontSize > 12)
                {
                    break;
                }
                fontSize -= 1;
                font = family.CreateFont(fontSize, FontStyle.Bold);
            }

            // Her satırı çiz
            float currentY = startY;
            foreach (var line in lines)
            {
                var textOptions = new TextOptions(font)
                {
                    Origin = new PointF(frame.Width/6, currentY),
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                canvas.Mutate(ctx => ctx.DrawText(
                    line,
                    font,
                    Color.White,
                    textOptions.Origin
                ));

                currentY += TextMeasurer.MeasureBounds(line, new TextOptions(font)).Height + 5; // 5 piksel satır aralığı
            }


            // (İsteğe bağlı) yazı eklenecekse buraya eklenebilir

            // Kaydet
            canvas.Save(outputPath);
        }

        Console.WriteLine("Görseller birleştirildi, orijinal nesneler korunarak.");
    }


}