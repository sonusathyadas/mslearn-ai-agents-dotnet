using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Azure.AI.Extensions.OpenAI;
using OpenAI.Responses;


// Load configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// Load values from appsettings.json into environment variables
foreach (var setting in configuration.AsEnumerable())
{
    if (!string.IsNullOrEmpty(setting.Value))
    {
        Environment.SetEnvironmentVariable(setting.Key, setting.Value);
    }
}

#pragma warning disable OPENAI001

var projectEndpoint = Environment.GetEnvironmentVariable("PROJECT_ENDPOINT");
var agentName = Environment.GetEnvironmentVariable("AGENT_NAME") ?? "it-support-agent";


if (string.IsNullOrEmpty(projectEndpoint))
{
    Console.WriteLine("Error: PROJECT_ENDPOINT environment variable not set");
    Console.WriteLine("Please set it in your appsettings.json or .env file");
    return;
}


// Connect to your project using the endpoint from your project page
AIProjectClient projectClient = new(endpoint: new Uri(projectEndpoint), tokenProvider: new DefaultAzureCredential());

//Create a new conversation to chat with the agent.
ProjectConversation conversation = projectClient.OpenAI.Conversations.CreateProjectConversation();

// Get a response client for the specific agent and conversation
ProjectResponsesClient responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(
    agentName,
    conversation);


Console.WriteLine("Chat started. Type 'exit' to quit.\n");

while (true)
{
    Console.Write("You: ");
    string? userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    if (userInput.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    try
    {
        ResponseResult response = responseClient.CreateResponse(userInput);

        // Try simple text output first
        string? outputText = response.GetOutputText();
        if (!string.IsNullOrWhiteSpace(outputText))
        {
            Console.WriteLine($"\nAgent: {outputText}\n");
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nError: {ex.Message}\n");
    }
}
