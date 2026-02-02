using Tesseract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ISAI.Lessons.WhsiperAIDemo
{
    /// <summary>
    /// Provides OCR text extraction using Tesseract engine.
    /// Optimized for extracting text from presentation slides with image preprocessing.
    /// </summary>
    public class TesseractOcrService : IDisposable
    {
        private readonly TesseractEngine _engine;
        private readonly string _tessdataPath;
        private readonly string _tempProcessedPath;

        public TesseractOcrService(string tessdataPath = @".\tessdata")
        {
            _tessdataPath = tessdataPath;
            _tempProcessedPath = Path.Combine(Path.GetTempPath(), "ocr_processed");
            
            // Ensure temp directory exists
            Directory.CreateDirectory(_tempProcessedPath);
            
            // Initialize Tesseract with English language
            // PSM 3 = Fully automatic page segmentation (good for slides)
            _engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
            
            // Optimize for presentation slides
            _engine.SetVariable("tessedit_char_whitelist", 
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,;:!?'-()[]{}/@#$%&* ");
        }

        /// <summary>
        /// Extracts text from an image file using Tesseract OCR.
        /// Preprocesses the image (grayscale + contrast enhancement) for better accuracy.
        /// </summary>
        public async Task<string> ExtractTextAsync(string imagePath)
        {
            return await Task.Run(() =>
            {
                string processedImagePath = null;
                
                try
                {
                    if (!File.Exists(imagePath))
                        return string.Empty;

                    // Preprocess image for better OCR accuracy
                    processedImagePath = PreprocessImageForOcr(imagePath);

                    // Process image with Tesseract
                    using var img = Pix.LoadFromFile(processedImagePath);
                    using var page = _engine.Process(img, PageSegMode.Auto);
                    
                    string text = page.GetText()?.Trim() ?? string.Empty;
                    
                    // Get confidence score
                    float confidence = page.GetMeanConfidence();
                    
                    // Log low-confidence extractions for debugging
                    if (confidence < 0.7f)
                    {
                        Console.WriteLine($"Low OCR confidence ({confidence:P0}) for: {imagePath}");
                    }

                    return CleanExtractedText(text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OCR Error for {imagePath}: {ex.Message}");
                    return string.Empty;
                }
                finally
                {
                    // Clean up temporary processed image
                    if (processedImagePath != null && File.Exists(processedImagePath))
                    {
                        try
                        {
                            File.Delete(processedImagePath);
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Preprocesses image using SixLabors.ImageSharp to improve OCR accuracy.
        /// Converts to grayscale and enhances contrast.
        /// </summary>
        private string PreprocessImageForOcr(string imagePath)
        {
            string processedPath = Path.Combine(
                _tempProcessedPath, 
                $"{Path.GetFileNameWithoutExtension(imagePath)}_processed.png"
            );

            using (var image = Image.Load<Rgb24>(imagePath))
            {
                image.Mutate(x => x
                    // Convert to grayscale - removes color noise
                    .Grayscale()
                    
                    // Enhance contrast - makes text sharper
                    .Contrast(1.5f)
                    
                    // Optional: Increase brightness slightly for better text visibility
                    .Brightness(1.1f)
                );

                // Save as PNG for lossless quality
                image.Save(processedPath);
            }

            return processedPath;
        }

        /// <summary>
        /// Cleans and normalizes extracted text
        /// </summary>
        private string CleanExtractedText(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return string.Empty;

            // Remove excessive whitespace
            return System.Text.RegularExpressions.Regex.Replace(rawText, @"\s+", " ").Trim();
        }

        public void Dispose()
        {
            _engine?.Dispose();
            
            // Clean up temp directory
            try
            {
                if (Directory.Exists(_tempProcessedPath))
                {
                    Directory.Delete(_tempProcessedPath, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}