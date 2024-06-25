import azure.functions as func
import datetime
import json
import logging
 
app = func.FunctionApp()
 
class TodoItem:
    # Define the TodoItem class string Id, string Task
    def __init__(self, id: str, task: str):
        self.id = id
        self.task = task
 
 
class InMemoryTodoManager:
    todos = []
 
    @staticmethod
    def add_todo(todo: TodoItem):
        InMemoryTodoManager.todos.append(todo)
        # log the todo
        logging.info(f"Added todo: {todo.task}")
        # print all todos with id and task
        for todo in InMemoryTodoManager.todos:
            logging.info(f"Id: {todo.id}, Task: {todo.task}")
 
 
    @staticmethod
    def get_todos():
        return list(InMemoryTodoManager.todos)
    
    @staticmethod
    def delete_todos():
        InMemoryTodoManager.todos.clear()
        
    
@app.function_name(name="AddToDoManager")
#@app.route(route="invoke/{appId}/{methodName}", auth_level=app.auth_level.ANONYMOUS)
@app.dapr_service_invocation_trigger(arg_name="payload", method_name="AddToDoManager")
def AddToDoManager(payload: str) :
    """
    See https://aka.ms/azure-functions-dapr for more information about using this binding
    
    These tasks should be completed prior to running :
         1. Install Dapr
    Run the app with below steps
         1. Start function app with Dapr: dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 -- func host start
         2. Invoke function app by dapr cli: dapr invoke --app-id functionapp --method {yourFunctionName}  --data '{ "data": {"value": { "orderId": "41" } } }'
    """
    logging.info('Azure function triggered by Dapr Service Invocation Trigger.')
    logging.info("Dapr service invocation trigger AddToDoManager payload: %s", payload)
 
    # add payload to InMemoryTodoManager by creating a guid for id and payload as task
    todo = TodoItem(id=str(datetime.datetime.now()), task=payload)
    InMemoryTodoManager.add_todo(todo)
 
# create a http trigger function to get the list of todos
 
 

@app.function_name(name="ChatAssitant")
@app.route(route="invoke/{appId}/{methodName}", auth_level=app.auth_level.ANONYMOUS)
@app.dapr_invoke_output(arg_name = "payload", app_id = "{appId}", method_name = "{methodName}", http_verb = "post")
def main(req: func.HttpRequest, payload: func.Out[str] ) -> str:
    """
    Sample to use a Dapr Invoke Output Binding to perform a Dapr Server Invocation operation hosted in another Darp'd app.
    Here this function acts like a proxy
    Invoke Dapr Service invocation trigger using Windows PowerShell with below request
 
    Invoke-RestMethod -Uri 'http://localhost:7071/api/invoke/functionapp/AddToDoManager' -Method POST -Headers @{
    'Content-Type' = 'application/json'
     } -Body 'I need to pickup my kids from school.'
    """
    logging.info('Python HTTP trigger function processed a request..')
    logging.info(req.params)
    data = req.params.get('data')
    if not data:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            data = req_body.get('data')
 
    if data:
        logging.info(f"Url: {req.url}, Data: {data}")
        payload.set(json.dumps({"body": data}).encode('utf-8'))
        return 'Successfully performed service invocation using Dapr invoke output binding.'
    else:
        return func.HttpResponse(
            "Please pass a data on the query string or in the request body",
            status_code=400
        )
    
      

@app.function_name(name="GetTodoManager") 
@app.route(route="GetTodoManager", auth_level=func.AuthLevel.ANONYMOUS)
def GetTodos(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python GetTodoManager HTTP trigger function processed a request.')
 
    # print all todos with id and task
    for todo in InMemoryTodoManager.todos:
        logging.info(f"Id: {todo.id}, Task: {todo.task}")
 
    # return http resonse with string message
    todos = InMemoryTodoManager.get_todos()
    todos_json = [todo.__dict__ for todo in todos]
    return func.HttpResponse(
        json.dumps(todos_json),
        mimetype="application/json",
        status_code=200
    )

#http trigger function to delete the todos
@app.function_name(name="DeleteTodoManager") 
@app.route(route="DeleteTodoManager", auth_level=func.AuthLevel.ANONYMOUS)
def DeleteTodos(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python DeleteTodoManager HTTP trigger function processed a request.')
 

 
    # return http resonse with string message
    InMemoryTodoManager.delete_todos()
    
    # Return a success response
    return func.HttpResponse(
        "Todo deleted successfully",
         status_code=204
    )