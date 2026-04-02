using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;


var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

foreach (var setting in configuration.AsEnumerable())
{
    if (!string.IsNullOrEmpty(setting.Value))
        Environment.SetEnvironmentVariable(setting.Key, setting.Value);
}

var projectEndpoint = Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT");
var modelDeployment = Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME");


if (string.IsNullOrEmpty(projectEndpoint))
{
    Console.WriteLine("Error: PROJECT_ENDPOINT environment variable not set.");
    Console.WriteLine("Please set it in appsettings.json or as an environment variable.");
    return;
}


// Agent instructions
string summarizerInstructions = """
            Summarize the customer's feedback in one short sentence. Keep it neutral and concise.
            Example output:
            App crashes during photo upload.
            User praises dark mode feature.
            """;

string classifierInstructions = """
            Classify the feedback as one of the following: Positive, Negative, or Feature request.
            """;

string actionInstructions = """
            Based on the summary and classification, suggest the next action in one short sentence.
            Example output:
            Escalate as a high-priority bug for the mobile team.
            Log as positive feedback to share with design and marketing.
            Log as enhancement request for product backlog.
            """;

// Set up the Azure Foundry client
var endpoint = Environment.GetEnvironmentVariable("AZURE_FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new Exception("AZURE_FOUNDRY_PROJECT_ENDPOINT is not set.");
var model = Environment.GetEnvironmentVariable("AZURE_FOUNDRY_PROJECT_MODEL_ID") ?? "gpt-4.1";

var persistentAgentsClient = new PersistentAgentsClient(endpoint, new AzureCliCredential());

// Create agents
AIAgent summarizer = await CreateAgentAsync(persistentAgentsClient, model, "summarizer", summarizerInstructions);
AIAgent classifier = await CreateAgentAsync(persistentAgentsClient, model, "classifier", classifierInstructions);
AIAgent action = await CreateAgentAsync(persistentAgentsClient, model, "action", actionInstructions);

// Customer feedback input
string feedback = """
            I use the dashboard every day to monitor metrics, and it works well overall.
            But when I'm working late at night, the bright screen is really harsh on my eyes.
            If you added a dark mode option, it would make the experience much more comfortable.
            """;

// Build sequential workflow: summarizer → classifier → action
var workflow = new WorkflowBuilder(summarizer)
    .AddEdge(summarizer, classifier)
    .AddEdge(classifier, action)
    .Build();

// Execute with streaming and collect outputs
var outputs = new List<string>();

await using StreamingRun run = await InProcessExecution.StreamAsync(
    workflow,
    new ChatMessage(ChatRole.User, $"Customer feedback: {feedback}")
);

// Send the turn token to trigger the agents
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

string? lastExecutorId = null;

await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
{
    if (evt is AgentResponseUpdateEvent streamingUpdate)
    {
        // Print agent name header when the active agent changes
        if (streamingUpdate.ExecutorId != lastExecutorId)
        {
            if (lastExecutorId != null)
                Console.WriteLine(); // newline after previous agent's output
            Console.WriteLine(new string('-', 60));
            Console.Write($"[{streamingUpdate.ExecutorId}] ");
            lastExecutorId = streamingUpdate.ExecutorId;
        }
        Console.Write(streamingUpdate.Data);
        outputs.Add(streamingUpdate.Data?.ToString() ?? string.Empty);
    }
}

Console.WriteLine(); // final newline

// Cleanup agents
await persistentAgentsClient.Administration.DeleteAgentAsync(summarizer.Id);
await persistentAgentsClient.Administration.DeleteAgentAsync(classifier.Id);
await persistentAgentsClient.Administration.DeleteAgentAsync(action.Id);


/// <summary>
/// Creates a persistent Azure Foundry agent with the given name and instructions.
/// </summary>
static async Task<AIAgent> CreateAgentAsync(
    PersistentAgentsClient client,
    string model,
    string name,
    string instructions)
{
    var agentMetadata = await client.Administration.CreateAgentAsync(
        model: model,
        name: name,
        instructions: instructions
    );
    return client.AsIChatClient(agentMetadata.Value.Id).AsAIAgent();
    //return await client.GetAIAgentAsync(agentMetadata.Value.Id);
}
