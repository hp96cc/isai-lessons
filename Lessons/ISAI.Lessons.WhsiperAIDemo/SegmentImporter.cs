using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISAI.Lessons.WhsiperAIDemo
{
    /*
     IMPORTER USAGE NOTES
     --------------------
     Purpose:
       - Reads persisted segment JSON files (the files produced by BuildRagRunner)
         and upserts them into the vector DB using QdrantVectorService.

     Quick run:
       - From __Program.cs__ (or any tool/runner) call:
           await SegmentImporter.ImportFromJsonAsync(
               @"C:\Temp\SOL RAG Test\segments",
               "6b0ac5fd-fc60-4cff-9fb1-bcee64832128"
           );
       - Ensure your Qdrant service is running and the collection name matches
         the one you used when exporting segments.

     Important details:
       - The JSON files MUST include the embedding as a primitive float array
         (e.g. "Vector": [0.123, 0.456, ...]). The importer maps that array to
         the domain type `LessonSegment.Vector` (which is a ReadOnlyMemory<float>).
       - Filenames are processed in lexical order. If you need strict ordering,
         include a sortable prefix (timestamp or sequence) in the filename.
       - The importer logs successes and failures to the console. Review output
         to identify skipped / invalid files.

     Recommended hygiene:
       - Persist embedding metadata (model id/version and generation date) with
         each JSON so you can detect stale embeddings before re-importing.
       - Consider adding a "force" option or skip-if-exists behavior to avoid
         duplicate upserts when re-running import.
       - For large imports, implement batching / parallel upserts in Qdrant to
         improve throughput and reduce connection overhead.

     Troubleshooting:
       - If vectors are empty or missing, verify the exporter wrote `Vector` as
         a float[] and that the serialization options didn't omit it.
       - If Qdrant rejects upserts, confirm credentials/endpoint in QdrantVectorService
         and that the target collection exists or is createable by the service.

     TODO (optional enhancements):
       - Add CLI flags for input path, collection name, skip-existing, batch size.
       - Add retry/backoff and progress metrics when importing many files.
    */

    /// <summary>
    /// Reads persisted segment JSON files (including embeddings) and upserts them into the vector DB.
    /// </summary>
    internal static class SegmentImporter
    {
        /// <summary>
        /// Import all JSON files from <paramref name="inputDirectory"/> into the specified collection.
        /// </summary>
        public static async Task ImportFromJsonAsync(string inputDirectory, string collectionName)
        {
            if (string.IsNullOrWhiteSpace(inputDirectory)) throw new ArgumentException("Input directory required", nameof(inputDirectory));
            if (string.IsNullOrWhiteSpace(collectionName)) throw new ArgumentException("Collection name required", nameof(collectionName));

            if (!Directory.Exists(inputDirectory))
            {
                Console.WriteLine($"Input directory does not exist: {inputDirectory}");
                return;
            }

            var qdrant = new QdrantVectorService(collectionName);
            await qdrant.InitializeAsync();

            var files = Directory.EnumerateFiles(inputDirectory, "*.json").OrderBy(f => f).ToArray();
            if (files.Length == 0)
            {
                Console.WriteLine("No JSON files found to import.");
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            int success = 0;
            int failed = 0;

            foreach (var file in files)
            {
                try
                {
                    string json = await File.ReadAllTextAsync(file);
                    var dto = JsonSerializer.Deserialize<SerializableSegment>(json, options);
                    if (dto is null)
                    {
                        Console.WriteLine($"Skipping (deserialized null): {file}");
                        failed++;
                        continue;
                    }

                    // Map DTO back to your domain type
                    var segment = new LessonSegment
                    {
                        Id = dto.Id,
                        StartTime = dto.StartTime,
                        VideoId = dto.VideoId,
                        Transcript = dto.Transcript,
                        SlideDescription = dto.SlideDescription,
                        ExtractedOcrText = dto.ExtractedOcrText,
                        Vector = new ReadOnlyMemory<float>(dto.Vector ?? Array.Empty<float>())
                    };

                    // Optional sanity check on vector length (if you expect fixed size)
                    if (segment.Vector.Length == 0)
                    {
                        Console.WriteLine($"Warning: segment {segment.Id} has empty vector, skipping upsert. File: {Path.GetFileName(file)}");
                        failed++;
                        continue;
                    }

                    await qdrant.UpsertVectorAsync(segment);
                    Console.WriteLine($"Upserted: {Path.GetFileName(file)} (Id={segment.Id}, vectorLen={segment.Vector.Length})");
                    success++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to import {Path.GetFileName(file)}: {ex.Message}");
                    failed++;
                }
            }

            Console.WriteLine($"Import complete. Success: {success}, Failed: {failed}");
        }

        // Lightweight DTO to match the JSON produced by the runner.
        private class SerializableSegment
        {
            public ulong Id { get; set; }
            public double StartTime { get; set; }
            public string VideoId { get; set; }
            public string Transcript { get; set; }
            public string SlideDescription { get; set; }
            public string ExtractedOcrText { get; set; }
            public float[] Vector { get; set; }
        }
    }
}