using OllamaSharp;
using Microsoft.SemanticKernel.Embeddings;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISAI.Lessons.WhsiperAIDemo
{
    /// <summary>
    /// Encapsulates the RAG database build workflow previously hosted in Program.cs.
    /// Keeps the logic isolated so Program remains focused on app startup / orchestration.
    /// </summary>
    internal static class BuildRagRunner
    {
        /// <summary>
        /// Builds a RAG (Retrieval-Augmented Generation) database by:
        /// - extracting frames for each SRT segment,
        /// - hashing frames to avoid redundant vision calls,
        /// - calling a vision service for slide descriptions when slides change,
        /// - extracting OCR text from slides for precise text search,
        /// - creating hybrid transcript+slide+OCR embeddings, and upserting vectors.
        /// </summary>
        public static async Task BuildRagDatabase(string videoPath, string srtPath)
        {
            var collectionName = "6b0ac5fd-fc60-4cff-9fb1-bcee64832128";

            // Ollama client used for embedding generation and vision descriptions
            var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"), "moondream");
            var _embeddingService = ollamaClient.AsTextEmbeddingGenerationService();

            var srtService = new SrtProcessingService();
            var frameService = new VideoFrameService();
            var visionService = new OllamaVisionService();
            var ocrService = new TesseractOcrService(@".\tessdata"); // Initialize OCR service
            var qdrantVectorService = new QdrantVectorService(collectionName);

            // Reset and ensure vector collection exists
            await qdrantVectorService.DeleteCollectionAsync();
            await qdrantVectorService.InitializeAsync();

            // Parse SRT into logical lesson segments
            var segments = srtService.ParseToLessonSegments(srtPath, "Lesson1");

            // Keep last processed frame hash+description so we only call vision when slide changes
            ulong lastHash = 0;
            string lastDescription = "No visual data available.";
            string lastOcrText = string.Empty;

            try
            {
                foreach (var segment in segments)
                {
                    // Extract a representative frame at the segment start time
                    string framePath = await frameService.ExtractFrameAsync(videoPath, (int)segment.StartTime, @"C:\Temp\SOL RAG Test\frames");

                    using var img = Image.Load<Rgb24>(framePath);
                    ulong currentHash = VisualHasher.ComputeDifferenceHash(img);

                    // Check similarity (threshold tuned for the chosen vision model)
                    if (VisualHasher.GetSimilarityDistance(lastHash, currentHash) > 5)
                    {
                        // Slide changed — ask vision service for a fresh description
                           lastDescription = await visionService.DescribeFrameAsync(framePath);
                        
                        // Extract OCR text immediately after vision description
                        lastOcrText = await ocrService.ExtractTextAsync(framePath);
                        
                        lastHash = currentHash;
                        
                        Console.WriteLine($"[NEW SLIDE DETECTED] OCR: {lastOcrText}");
                    }

                    // Annotate the segment with the latest slide description and OCR text
                    segment.SlideDescription = lastDescription;
                    segment.ExtractedOcrText = lastOcrText;

                    // Create a hybrid content string that mixes transcript + slide description + OCR text
                    string ragContent = $"[TRANSCRIPT]: {segment.Transcript}\n[SLIDE]: {segment.SlideDescription}\n[TEXT]: {segment.ExtractedOcrText}";
                    Console.WriteLine(ragContent);

                    // Generate and persist the embedding vector for hybrid search
                    segment.Vector = await _embeddingService.GenerateEmbeddingAsync(ragContent);
                    Console.WriteLine(segment.Vector.ToString());
                    await qdrantVectorService.UpsertVectorAsync(segment);
                }
            }
            finally
            {
                // Dispose OCR service to free resources
                ocrService.Dispose();
            }
        }
    }
}