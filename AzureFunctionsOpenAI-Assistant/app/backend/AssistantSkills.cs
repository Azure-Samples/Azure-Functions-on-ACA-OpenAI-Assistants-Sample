using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Microsoft.Azure.Functions.Extensions.Dapr.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.Assistants;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AssistantSample;

public class AssistantSkills
{
    readonly ITodoManager todoManager;
    readonly ILogger<AssistantSkills> logger;

    public AssistantSkills(ITodoManager todoManager, ILogger<AssistantSkills> logger)
    {
        this.todoManager = todoManager ?? throw new ArgumentNullException(nameof(todoManager));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function(nameof(AddTodo))]
    [DaprInvokeOutput(AppId = "functionapp", MethodName = "AddToDoManager", HttpVerb = "post")]
    public async Task<InvokeMethodParameters> AddTodo(
        [AssistantSkillTrigger("Create a new todo task", Model = "%CHAT_MODEL_DEPLOYMENT_NAME%")]
            string taskDescription
    )
    {
        this.logger.LogInformation(
            "C# HTTP trigger function processed a request with Task Description {0}",
            taskDescription
        );
        var outputContent = new InvokeMethodParameters { Body = taskDescription };

        return outputContent;
    }

    private readonly HttpClient client = new HttpClient();

    [Function(nameof(GetTodos))]
    public async Task<IReadOnlyList<TodoItem>> GetTodos(
        [AssistantSkillTrigger(
            "Fetch the list of previously created todo tasks",
            Model = "%CHAT_MODEL_DEPLOYMENT_NAME%"
        )]
            object inputIgnored
    )
    {
        string responseString = await client.GetStringAsync(
            "https://tododaprfuncapp.livelypebble-0ece6897.westcentralus.azurecontainerapps.io/api/GetTodoManager"
        );
        List<TodoItem> todoItem = JsonConvert.DeserializeObject<List<TodoItem>>(responseString);

        this.logger.LogInformation(
            "C# GetToDos HTTP trigger function processed a request.{0} and {1}",
            todoItem,
            responseString
        );

        return todoItem.AsReadOnly();
        //return this.todoManager.GetTodosAsync();
    }

    [Function(nameof(SendEmail))]
    public async Task SendEmail(
        [AssistantSkillTrigger("Send an email", Model = "%CHAT_MODEL_DEPLOYMENT_NAME%")]
            object inputIgnored
    )
    {
        //Replace with your domain and modify the content, recipient details as required
        string connectionString =
            "endpoint=https://antaiskills-acs.unitedstates.communication.azure.com/;accesskey=BxPJ5Rn+wfGlETYZMW2Gv0KdbpkyYavRnZaXQdtvkyfi+iijLrFUf4MZ+Nob84uskeb0mIKMSOjIvWBdqFJShQ==";
        var emailClient = new EmailClient(connectionString);

        string responseString = await client.GetStringAsync(
            "https://tododaprfuncapp.livelypebble-0ece6897.westcentralus.azurecontainerapps.io/api/GetTodoManager"
        );
        this.logger.LogInformation(
            "C# SendEmail HTTP trigger function processed a request.{0}",
            responseString
        );
        // Deserialize the JSON string into a list of objects
        var todoItems = JsonConvert.DeserializeObject<List<TodoItem>>(responseString);

        string tasks = string.Join(
            "\n",
            todoItems
                .Select(item => item.Task)
                .Distinct()
                .Select((task, index) => $"{index + 1}. {task}")
        );
        this.logger.LogInformation(
            "C# SendEmail HTTP trigger function processed a request.{0}",
            tasks
        );

        EmailSendOperation emailSendOperation = emailClient.Send(
            WaitUntil.Completed,
            senderAddress: "DoNotReply@fb1972eb-0ed7-45a5-922d-1e4b5e0ad4a5.azurecomm.net",
            recipientAddress: "ramya.oruganti@microsoft.com",
            subject: "ToDo Task Created",
            htmlContent: $"<html><h1>Your Todo List </h1><p>{tasks.Replace("\n", "<br/>")}</p></html>",
            plainTextContent: tasks
        );
        
    }
}
