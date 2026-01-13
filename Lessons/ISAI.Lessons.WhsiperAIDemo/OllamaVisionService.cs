using FFMpegCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.Net.Http.Json;

namespace ISAI.Lessons.WhsiperAIDemo
{
    public class OllamaVisionService
    {

        private const int MaxDimension = 1024; // Optimal for Qwen2.5-VL balance
        public async Task<string> DescribeFrameAsync(string imagePath)
        {
            // 1. Read and validate image
            if (!File.Exists(imagePath)) return "Error: File not found";

            // 1. Load and Resize Image
            using var image = await Image.LoadAsync(imagePath);

            // Only resize if the image is larger than our threshold
            if (image.Width > MaxDimension || image.Height > MaxDimension)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(MaxDimension, MaxDimension),
                    Mode = ResizeMode.Max // Maintains aspect ratio
                }));
            }

            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms); // JPEG is smaller than PNG for faster upload
            string base64Image = Convert.ToBase64String(ms.ToArray());

            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:11434");
            client.Timeout = TimeSpan.FromSeconds(30); // Give it time to load the model


            var requestBody = new
            {
                model = "moondream", // Ensure you've run 'ollama pull moondream'
                prompt = "Extract all visible text from the image. .",
                stream = false,
                images = new[] { base64Image },
                options = new
                {
                    temperature = 0.0, // Tiny bit of warmth helps Moondream's natural language
                    num_ctx = 2048     // Moondream has a smaller context window than Qwen
                }
            };
            try
            {
                var response = await client.PostAsJsonAsync("/api/generate", requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return $"API Error: {error}";
                }

                var result = await response.Content.ReadFromJsonAsync<OllamaRawResponse>();

                // Moondream sometimes returns null/empty if the image is too complex
                return string.IsNullOrWhiteSpace(result?.Response)
                    ? "Model returned no description for this frame."
                    : result.Response;
            }
            catch (Exception ex)
            {
                return $"Client Error: {ex.Message}";
            }
        }

        // Ensure your DTO matches the Ollama "generate" schema
        public record OllamaRawResponse(string Response, bool Done);


    }
}
