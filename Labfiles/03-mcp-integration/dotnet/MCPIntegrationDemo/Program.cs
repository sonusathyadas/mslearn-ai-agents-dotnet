
// Add references


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
var modelDeployment = Environment.GetEnvironmentVariable("MODEL_DEPLOYMENT_NAME");
var agentName = Environment.GetEnvironmentVariable("AGENT_NAME") ?? "astronomy-agent";

if (string.IsNullOrEmpty(projectEndpoint))
{
    Console.WriteLine("Error: PROJECT_ENDPOINT environment variable not set.");
    Console.WriteLine("Please set it in appsettings.json or as an environment variable.");
    return;
}

if (string.IsNullOrEmpty(modelDeployment))
{
    Console.WriteLine("Error: MODEL_DEPLOYMENT_NAME environment variable not set.");
    Console.WriteLine("Please set it in appsettings.json or as an environment variable.");
    return;
}

// Connect to the agents client


// Create agent with MCP tool


//Create a conversation thread


// Get the response client for the agent and conversation


// Send initial request that will trigger the MCP tool


// Process any MCP approval requests that were generated


// Send the approval response back and retrieve a response


// Clean up resources by deleting the agent version
projectClient.Agents.DeleteAgentVersion(agentName: agentVersion.Name, agentVersion: agentVersion.Version);

