using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.Search;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace sample.demo
{
    public class Ask
    {
        private readonly ILogger<Ask> _logger;

        public Ask(ILogger<Ask> logger)
        {
            _logger = logger;
        }

        [Function("ask")]
        public HttpResponseData AskData(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = "ask")] HttpRequestData req,
            [SemanticSearchInput(
                "AISearchEndpoint",
                "openai-index",
                Query = "{question}",
                ChatModel = "%CHAT_MODEL_DEPLOYMENT_NAME%",
                EmbeddingsModel = "%EMBEDDING_MODEL_DEPLOYMENT_NAME%",
                SystemPrompt = "%SYSTEM_PROMPT%"
            )]
                SemanticSearchContext result
        )
        {
            _logger.LogInformation("Ask function called...");
            HttpResponseData responseData = req.CreateResponse(HttpStatusCode.OK);

            var answer =
                "{\"data_points\":[],\"answer\":"
                + JsonConvert.ToString(result.Response)
                + ",\"thoughts\":null}";
            responseData.WriteAsJsonAsync(answer, HttpStatusCode.OK);

            return responseData;
        }
    }
}
