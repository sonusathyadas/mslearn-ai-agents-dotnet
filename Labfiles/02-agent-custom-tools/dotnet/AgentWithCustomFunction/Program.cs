
/// Add references


#pragma warning disable OPENAI001

// -----------------------------------------------------------------------
// Configuration
// -----------------------------------------------------------------------

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

// Connect to the project client




// Define the event function tool


// Define the observation cost function tool


// Define the observation report generation function tool



// Create a new agent with the function tools


//Create a thread for the chat session


// Get the response client for the agent and conversation



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
        
        ResponseItem request = ResponseItem.CreateUserMessageItem(userInput);
        var inputItems = new List<ResponseItem> { request };
        bool functionCalled = false;
        ResponseResult response;
        do
        {
            // Send the user input and any function tool outputs back to the agent and get the response
            


        } while (functionCalled);
        Console.WriteLine(response.GetOutputText());

    }
    catch (Exception ex)
    {
        Console.WriteLine("Error:" + ex.Message);
    }
}

// Delete the conversation when done

