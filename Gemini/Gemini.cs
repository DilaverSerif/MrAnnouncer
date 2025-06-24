using Mscc.GenerativeAI;

public static class Gemini
{

    public static async Task<string> AskGeminiAsync(string newText,bool useTagOnPrompt = false)
    {

        if(newText == null || newText == ""){
            Console.WriteLine("Haber metni boş olamaz");
            return "null";
        }

        string systemInstructionText = "Haberi dikkatlice oku ve haberin haber olma sebebini maksimum 250 harf olacak şekilde Türkçe olarak özetle. Sadece özeti ver, başka bir şey ekleme.\n\n" +
"# Steps\n\n" +
"1. Haberi dikkatlice ve ayrıntılı bir şekilde oku.\n" +
"2. Haberin ana fikrini ve neden haber değeri taşıdığını belirle.\n" +
"3. Haber değeri taşıyan ana noktaları seçerek en etkili ve kısa şekilde özetle.\n" +
"4. Özeti 250 harfi geçmeyecek şekilde, etkili bir dil ve yapıda oluştur.\n\n" +
"# Output Format\n\n" +
"Özet, maksimum 250 harf uzunluğunda Türkçe bir cümle olmalıdır. Boşluklar dahil karakter sınırını kesinlikle aşmamalı.\n\n" +
"# Examples\n\n" +
"**Örnek Girdi:**\n\"[Haber metni]\"\n\n" +
"**Örnek Çıktı:**\n\"[Haber özet cümlesi]\"\n\n" +
"(Örnek çıktının spesifik, dolaysız ve haber ajansı gibi sunulması gerektiğini unutma. Özellikle kelime seçiminde etkili ve kısa olunmalıdır.)";

        if(useTagOnPrompt){
            systemInstructionText +=
            "\n\nEkstra olarak Haber metninde geçen kelimeleri veya ifadeleri etiket olarak kullan 2 Adet olsun. Örnek: #Etiket1 #Etiket2 #Etiket3. Taglar dahil 280 harfi geçme.";
        }

        //Gemini API Key
        var apiKey = "AIzaSyATB6jWuqKwPMHRsaGW9oJVeoOJwxNddAM";
        var systemInstruction = new Content(systemInstructionText);
        IGenerativeAI genAi = new GoogleAI(apiKey);
        var model = genAi.GenerativeModel(Model.Gemini20FlashThinking, systemInstruction: systemInstruction);
        
        // Doğrudan string olarak gönder
        var response = await model.GenerateContent(newText);

        if (response.Text == null)
            return "null";

        Console.WriteLine(response.Text);
        return response.Text;
    }
}