using System.IO;
using System.Net;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.Embeddings;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.Search;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace sample.demo
{
    public class Upload
    {
        private readonly ILogger<Upload> _logger;

        public Upload(ILogger<Upload> logger)
        {
            _logger = logger;
        }

        public class QueueHttpResponse
        {
            [QueueOutput("filequeue", Connection = "queueConnection")]
            public QueuePayload[]? QueueMessage { get; set; }
            public HttpResponseData? HttpResponse { get; set; }
        }

        public class QueuePayload
        {
            public string? FileName { get; set; }
        }

        /// <summary>
        /// Uploads the file Azure Files and adds the file location to the queue message.
        /// The file location is then retrieved by the queue trigger to embed the content by the EmbedContent function.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("upload")]
        public static async Task<QueueHttpResponse> UploadFile(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = "upload")] HttpRequestData req
        )
        {
            var fileShare = Environment.GetEnvironmentVariable("fileShare");
            // Read file from request
            var parsedFormBody = await MultipartFormDataParser.ParseAsync(req.Body);
            QueuePayload[] payload = new QueuePayload[] { };

            // Save file to Azure Files and add file location to queue message
            foreach (var file in parsedFormBody.Files)
            {
                var reader = new StreamReader(file.Data);
                var fileStream = File.Create(Path.Combine(fileShare, file.FileName));
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                reader.BaseStream.CopyTo(fileStream);
                fileStream.Close();
                var queueMessage = new QueuePayload
                {
                    FileName = Path.Combine(fileShare, file.FileName)
                };
                payload = payload.Append(queueMessage).ToArray();
            }

            var responseData = req.CreateResponse(HttpStatusCode.OK);
            var result = "{\"success\":True,\"message\":\"Files processed successfully.\"}";
            await responseData.WriteAsJsonAsync(result, HttpStatusCode.OK);

            // Return queue message and response as output
            return new QueueHttpResponse { QueueMessage = payload, HttpResponse = responseData };
        }

        public class EmbeddingsStoreOutputResponse
        {
            [EmbeddingsStoreOutput(
                "{FileName}",
                InputType.FilePath,
                "AISearchEndpoint",
                "openai-index",
                Model = "%EMBEDDING_MODEL_DEPLOYMENT_NAME%"
            )]
            public required SearchableDocument SearchableDocument { get; init; }
        }

        [Function("EmbedContent")]
        public static async Task<EmbeddingsStoreOutputResponse> EmbedContent(
            [QueueTrigger("filequeue", Connection = "queueConnection")] QueuePayload queueItem
        )
        {
            return new EmbeddingsStoreOutputResponse
            {
                SearchableDocument = new SearchableDocument(queueItem.FileName)
            };
        }
    }
}
