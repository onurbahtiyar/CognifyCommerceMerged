using System;
using System.Threading.Tasks;
using GenerativeAI;

namespace GeminiExample
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var apiKey = "AIzaSyDYZ58z8AVjdZp4nKOAgnZR7VlecbF9hUU";

            var googleAI = new GoogleAi(apiKey);

            var model = googleAI.CreateGenerativeModel("models/gemini-1.5-flash");
            var chatSession = model.StartChat();

            Console.Write("Bir şey sor (Türkçe de kullanabilirsin): ");
            var prompt = Console.ReadLine() ?? "";

            await foreach (var chunk in model.StreamContentAsync(prompt))
            {
                Console.Write(chunk.Text);
            }
            Console.WriteLine();
            Console.ReadKey();

        }
    }
}
