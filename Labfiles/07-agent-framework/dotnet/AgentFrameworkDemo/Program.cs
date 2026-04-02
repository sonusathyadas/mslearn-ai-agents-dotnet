
//Add references



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
var modelDeploymentName = Environment.GetEnvironmentVariable("MODEL_DEPLOYMENT_NAME") ?? "gpt-4.1";

if (string.IsNullOrEmpty(projectEndpoint))
{
    Console.WriteLine("Error: PROJECT_ENDPOINT environment variable not set.");
    Console.WriteLine("Please set it in appsettings.json or as an environment variable.");
    return;
}

if (string.IsNullOrEmpty(modelDeploymentName))
{
    Console.WriteLine("Error: MODEL_DEPLOYMENT_NAME environment variable not set.");
    Console.WriteLine("Please set it in appsettings.json or as an environment variable.");
    return;
}

// Clear the console
Console.Clear();

// Load the expenses data file
string scriptDir = AppContext.BaseDirectory;
string filePath = Path.Combine(scriptDir, "data.txt");
string data = await File.ReadAllTextAsync(filePath) + "\n";

// Ask for a prompt
Console.WriteLine($"Here is the expenses data in your file:\n\n{data}\nWhat would you like me to do with it?\n");
string userPrompt = Console.ReadLine() ?? string.Empty;

// Run the agent
await ProcessExpensesDataAsync(userPrompt, data);



async Task ProcessExpensesDataAsync(string prompt, string expensesData)
{


    // Create a client and initialize an agent with the tool and instructions


    // Use the agent to process the expenses data

}


// Create a tool function for the email functionality

