
// Add references


#pragma warning disable OPENAI001

// Load configuration from appsettings.json (equivalent to dotenv)
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
var workflowName = Environment.GetEnvironmentVariable("WORKFLOW_NAME");
var workflowVersion = Environment.GetEnvironmentVariable("WORKFLOW_VERSION") ?? "1";

if (string.IsNullOrEmpty(projectEndpoint))
{
    Console.WriteLine("Error: PROJECT_ENDPOINT environment variable not set.");
    return;
}

// Connect to the AI Project client


// Create a conversation



// Get a responses client scoped to the workflow agent


// Create a response from the workflow agent
