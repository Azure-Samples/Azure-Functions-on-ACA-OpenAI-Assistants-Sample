using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.Assistants;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace sample.demo
{
    public class Chat
    {
        private readonly ILogger<Chat> _logger;

        public Chat(ILogger<Chat> logger)
        {
            _logger = logger;
        }

        public class CreateChatBotOutput
        {
            [AssistantCreateOutput()]
            public AssistantCreateRequest? ChatBotCreateRequest { get; set; }

            public HttpResponseData? HttpResponse { get; set; }
        }

        [Function("chat")]
        public static async Task<CreateChatBotOutput> CreateAssistant(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "chat/{assistantId}")]
                HttpRequestData req,
            string assistantId
        )
        {
            var responseJson = new { assistantId };

            string instructions = """
                Don't make assumptions about what values to plug into functions.
                Ask for clarification if a user request is ambiguous.
                """;

            HttpResponseData response = req.CreateResponse();

            await response.WriteAsJsonAsync(responseJson, HttpStatusCode.Created);

            return new CreateChatBotOutput
            {
                HttpResponse = response,
                ChatBotCreateRequest = new AssistantCreateRequest(assistantId, instructions),
            };
        }

        public class PostResponseOutput
        {
            public HttpResponseData? HttpResponse { get; set; }
        }

        [Function("chatQuery")]
        public static async Task<PostResponseOutput> ChatQuery(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "chat/{assistantId}")]
                HttpRequestData req,
            string assistantId,
            [AssistantPostInput(
                "{assistantId}",
                "{prompt}",
                Model = "%CHAT_MODEL_DEPLOYMENT_NAME%"
            )]
                AssistantState state
        )
        {
            // Send response to client in expected format, including assistantId
            HttpResponseData responseData = req.CreateResponse(HttpStatusCode.OK);
            var result =
                "{\"data_points\":[],\"answer\":"
                + JsonConvert.ToString(
                    state.RecentMessages.LastOrDefault()?.Content ?? "No response returned."
                )
                + ",\"thoughts\":null}";
            await responseData.WriteAsJsonAsync(result, HttpStatusCode.OK);

            return new PostResponseOutput { HttpResponse = responseData, };
        }

        [Function(nameof(GetChatState))]
        public static async Task<HttpResponseData> GetChatState(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat/{assistantId}")]
                HttpRequestData req,
            string assistantId,
            [AssistantQueryInput("{assistantId}", TimestampUtc = "{Query.timestampUTC}")]
                AssistantState state
        )
        {
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            // Returns the last message from the history table which will be the latest answer to the last question
            var result =
                "{\"data_points\":[],\"answer\":"
                + JsonConvert.ToString(
                    state.RecentMessages.LastOrDefault()?.Content ?? "No response returned."
                )
                + ",\"thoughts\":null}";
            await response.WriteAsJsonAsync(result);
            return response;
        }
    }
}
