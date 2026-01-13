using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using OllamaSharp;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Net;

namespace ISAI.Lessons.WhsiperAIDemo
{
    public  class ChatService
    {

        private QdrantVectorService _qdrantVectorService;
        private ITextEmbeddingGenerationService _embeddingService;

        public ChatService(QdrantVectorService qdrantVectorService, ITextEmbeddingGenerationService embeddingService)
        {
            _qdrantVectorService = qdrantVectorService;
            _embeddingService = embeddingService;
        }

        public async Task<string> AskAiAboutLesson(string userQuestion)
        {

            var relatedSegments = await SearchLessonAsync(userQuestion);

            var context = string.Join("\n\n", relatedSegments.Select(s =>
                $"[At {s.StartTime}s] Spoken: {s.Transcript}  | On-Screen: {s.SlideDescription} | VideoId: { s.VideoId}"));

            string prompt = $"""
                You are a teaching assistant. Use the following lesson context to answer the question.
                If the answer isn't in the context, use other knowledge, but provide sources if outside context. Provide link to the videos in the format http://www.isa.co.uk/videoid/starttime

                CONTEXT:
                {context}

                USER QUESTION: 
                {userQuestion}
                """;


            //var chatModelClient = new OllamaApiClient(
            //    new Uri("http://localhost:11434"),
            //    "llama3.2:3b-instruct-q2_K"
            //);

            // IChatClient _chatClient = chatModelClient;

            AzureOpenAIClient azureClient = new(
            new Uri("https://isai-foundry-resource.openai.azure.com/"),
            new ApiKeyCredential("5N8mhWmxo5Ys8XiusgtssyZYxI5V53pZeArVgUzJOHtcYiveEKx7JQQJ99BLACfhMk5XJ3w3AAAAACOGGxKZ"));
                    IChatClient _chatClient = azureClient.GetChatClient("gpt-5-nano").AsIChatClient();

            var response = await _chatClient.GetResponseAsync(prompt);
            return response.Text;
        }
        public async Task<List<LessonSegment>> SearchLessonAsync(string userQuestion)
        {

            var queryVector = await _embeddingService.GenerateEmbeddingAsync(userQuestion);

            // 2. Configure search options
            //var searchOptions = new VectorSearchOptions<LessonSegment>
            //{
            //    Top = 3, // Return the 3 most relevant segments
            //    IncludeVectors = false // We only need the text context for the final answer
            //};

            // 3. Execute the search against Qdrant
            var searchResults = await _qdrantVectorService.SearchAsync(queryVector);

            // 4. Transform results into a list of LessonSegments
            var context = new List<LessonSegment>();
            await foreach (var result in searchResults)
            {
                // Each result includes a 'Score' (how similar it is) 
                // and the 'Record' (your LessonSegment data)
                context.Add(result.Record);

                Console.WriteLine($"Found match (Score: {result.Score:P2}): {result.Record.Transcript}...");
            }

            return context;
        }

    }
}
