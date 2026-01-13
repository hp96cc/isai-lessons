using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;

namespace ISAI.Lessons.WhsiperAIDemo
{

    public class QdrantVectorService
    {

        private readonly QdrantClient _qdrantClient;
        private readonly QdrantVectorStore _vectorStore;
        private readonly QdrantCollection<Guid, LessonSegment> _qdrantCollection;
        private readonly string _collectionName;

        public QdrantVectorService(string collectionName, string host = "localhost", int port = 6334)
        {

            _collectionName = collectionName;

            // 1. Create the underlying Qdrant client
            _qdrantClient = new QdrantClient(host, port);

            // 2. Create the Vector Store
            _vectorStore = new QdrantVectorStore(_qdrantClient, true);

            // 3. Get the specific collection for your LessonSegment
            _qdrantCollection = new QdrantCollection<Guid, LessonSegment>(
                _qdrantClient,
                _collectionName.ToString(),
                true);



        }

        /// <summary>
        /// Initialize the collection - call this once during setup
        /// </summary>
        public async Task InitializeAsync()
        {
            // 4. Ensure the collection exists in the database
            await _qdrantCollection.EnsureCollectionExistsAsync();
        }

        /// <summary>
        /// Update a single vector record (upsert - insert or update)
        /// </summary>
        public async Task UpsertVectorAsync(LessonSegment segment)
        {
            await _qdrantCollection.UpsertAsync(segment);
        }

        /// <summary>
        /// Update multiple vector records in batch
        /// </summary>
        public async Task UpsertVectorBatchAsync(IEnumerable<LessonSegment> segments)
        {
            await _qdrantCollection.UpsertAsync(segments);
        }

        /// <summary>
        /// Get a vector record by ID
        /// </summary>
        public async Task<LessonSegment?> GetVectorAsync(string id)
        {
            return await _qdrantCollection.GetAsync(Guid.Parse(id));
        }

        /// <summary>
        /// Delete a vector record by ID
        /// </summary>
        public async Task DeleteVectorAsync(string id)
        {
            await _qdrantCollection.DeleteAsync(Guid.Parse(id));
        }

        /// <summary>
        /// Delete multiple vector records by IDs
        /// </summary>
        public async Task DeleteVectorBatchAsync(IEnumerable<Guid> ids)
        {
            await _qdrantCollection.DeleteAsync(ids);
        }

        /// <summary>
        /// Update only the vector values for an existing point (using direct Qdrant client)
        /// This is useful when you want to update embeddings without changing other data
        /// </summary>
        public async Task UpdateVectorValuesAsync(byte[] pointIdData, float[] newVector, string vectorName = "")
        {
            if (string.IsNullOrEmpty(vectorName))
            {
                // Update unnamed/default vector
                await _qdrantClient.UpdateVectorsAsync(
                    collectionName: _collectionName,
                    points: new List<PointVectors>
                    {
                    new PointVectors
                    {
                        Id = PointId.Parser.ParseFrom(pointIdData),
                        Vectors = newVector
                    }
                    }
                );
            }
            else
            {
                // Update named vector
                var vectors = new Dictionary<string, float[]>
                {
                    [vectorName] = newVector
                };

                await _qdrantClient.UpdateVectorsAsync(
                    collectionName: _collectionName,
                    points: new List<PointVectors>
                    {
                    new PointVectors
                    {
                        Id = PointId.Parser.ParseFrom(pointIdData),
                        Vectors = vectors
                    }
                    }
                );
            }
        }

        /// <summary>
        /// Search for similar vectors
        /// </summary>
        public async Task<IAsyncEnumerable<VectorSearchResult<LessonSegment>>> SearchAsync(ReadOnlyMemory<float> searchVector, int limit = 10)
        {
            return  _qdrantCollection.SearchAsync(searchVector, 10);
        }



        /// <summary>
        /// Check if collection exists
        /// </summary>
        public async Task<bool> CollectionExistsAsync()
        {
            return await _qdrantCollection.CollectionExistsAsync();
        }

        /// <summary>
        /// Delete the entire collection
        /// </summary>
        public async Task DeleteCollectionAsync()
        {
            await _qdrantCollection.EnsureCollectionDeletedAsync();
        }

    }

}