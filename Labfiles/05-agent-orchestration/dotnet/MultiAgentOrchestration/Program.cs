// Add references



var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

foreach (var setting in configuration.AsEnumerable())
{
    if (!string.IsNullOrEmpty(setting.Value))
        Environment.SetEnvironmentVariable(setting.Key, setting.Value);
}

var endpoint = Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT");
var model = Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME");


if (string.IsNullOrEmpty(endpoint))
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


// Create agents

// Initialize the current feedback


// Build sequential workflow: summarizer → classifier → action



// Execute with streaming and collect outputs


// Send the turn token to trigger the agents





// Cleanup agents
await persistentAgentsClient.Administration.DeleteAgentAsync(summarizerId);
await persistentAgentsClient.Administration.DeleteAgentAsync(classifierId);
await persistentAgentsClient.Administration.DeleteAgentAsync(actionId);


/// <summary>
/// Creates a persistent Azure Foundry agent with the given name and instructions.
/// </summary>
static async Task<(AIAgent Agent, string AgentId)> CreateAgentAsync(
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
    var agentId = agentMetadata.Value.Id;
    return (client.AsIChatClient(agentId).AsAIAgent(), agentId);
    //return await client.GetAIAgentAsync(agentMetadata.Value.Id);
}
