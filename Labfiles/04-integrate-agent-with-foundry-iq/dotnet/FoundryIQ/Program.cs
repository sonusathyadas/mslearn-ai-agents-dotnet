using System.Text.Json;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

#pragma warning disable OPENAI001

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

foreach (var setting in configuration.AsEnumerable())
{
    if (!string.IsNullOrEmpty(setting.Value))
        Environment.SetEnvironmentVariable(setting.Key, setting.Value);
}

var projectEndpoint = Environment.GetEnvironmentVariable("PROJECT_ENDPOINT");
var agentName = Environment.GetEnvironmentVariable("AGENT_NAME") ?? "astronomy-agent";

if (string.IsNullOrEmpty(projectEndpoint))
{
    Console.WriteLine("Error: PROJECT_ENDPOINT environment variable not set.");
    Console.WriteLine("Please set it in appsettings.json or as an environment variable.");
    return;
}

// Connect to the project and agent



//Create a new conversation to chat with the agent.



// Get a response client for the specific agent and conversation




// Conversation history for context (client-side tracking)




async Task<string?> SendMessageToAgentAsync(string userMessage)
{
    try
    {
        // TODO : Send request to agent and get response



    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n\nError: {ex.Message}\n");
        return null;
    }
}

void DisplayConversationHistory()
{
    Console.WriteLine("\n" + new string('=', 60));
    Console.WriteLine("CONVERSATION HISTORY");
    Console.WriteLine(new string('=', 60) + "\n");

    foreach (var (role, content) in conversationHistory)
    {
        Console.WriteLine($"{role.ToUpper()}: {content}\n");
    }

    Console.WriteLine(new string('=', 60) + "\n");
}

// Main interaction loop
Console.WriteLine("Contoso Product Expert Agent");
Console.WriteLine("Ask questions about our outdoor and camping products.");
Console.WriteLine("Type 'history' to see conversation history, or 'quit' to exit.\n");

while (true)
{
    try
    {
        Console.Write("You: ");
        var userInput = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(userInput))
            continue;

        if (userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("\nEnding conversation...");
            break;
        }

        if (userInput.Equals("history", StringComparison.OrdinalIgnoreCase))
        {
            DisplayConversationHistory();
            continue;
        }

        await SendMessageToAgentAsync(userInput);
    }
    catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
    {
        Console.WriteLine("\n\nInterrupted by user.");
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nUnexpected error: {ex.Message}\n");
    }
}

Console.WriteLine("\nConversation ended.");