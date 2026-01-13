
//using Microsoft.Extensions.AI;
//using Microsoft.Extensions.VectorData;

//namespace ISAI.Lessons.WhsiperAIDemo
//{
//    public class LessonPoint
//    {
//        [VectorStoreRecordKey]
//        public ulong Id { get; set; } // Qdrant uses ulong or Guid for IDs

//        [VectorStoreRecordData(IsFilterable = true)]
//        public string LessonId { get; set; }

//        [VectorStoreRecordData]
//        public int Timestamp { get; set; }

//        [VectorStoreRecordData]
//        public string Text { get; set; }

//        [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]
//        public ReadOnlyMemory<float> Vector { get; set; }
//    }

//    public class LessonSearchService
//    {
//        // Use the interface for Dependency Injection
//        private readonly IVectorStoreCollection<ulong, LessonPoint> _collection;

//        public LessonSearchService(IVectorStoreRecordCollection<ulong, LessonPoint> collection)
//        {
//            _collection = collection;
//        }

//        public async Task<List<LessonPoint>> SearchAsync(ReadOnlyMemory<float> vector)
//        {
//            // The search method is often renamed to 'VectorizedSearchAsync' 
//            // in the latest 2026 abstractions
//            var searchResult = await _collection.VectorizedSearchAsync(vector, new VectorSearchOptions
//            {
//                Top = 3,
//                IncludeVectors = false
//            });

//            var matches = new List<LessonPoint>();
//            await foreach (var record in searchResult.Results)
//            {
//                matches.Add(record.Record);
//            }
//            return matches;
//        }
//    }
//}
