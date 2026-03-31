
# Use a custom function in an AI agent

In this exercise you'll explore creating an agent that can use custom functions as a tool to complete tasks. The agent will act as an astronomy assistant that can provide information about astronomical events and calculate the cost of telescope rentals based on user inputs. You'll define the function tools and implement the logic to process function calls made by the agent.

> **Tip**: The code used in this exercise is based on the for Microsoft Foundry SDK for .NET. You can develop similar solutions using the SDKs for Python, JavaScript, and Java. Refer to [Microsoft Foundry SDK client libraries](https://learn.microsoft.com/azure/ai-foundry/how-to/develop/sdk-overview) for details.

This exercise should take approximately **50** minutes to complete.

> **Note**: Some of the technologies used in this exercise are in preview or in active development. You may experience some unexpected behavior, warnings, or errors.

## Prerequisites

Before starting this exercise, ensure you have:

- [Visual Studio Code](https://code.visualstudio.com/) installed on your local machine
- An active [Azure subscription](https://azure.microsoft.com/free/)
- [.NET 10 or later](https://dotnet.microsoft.com/en-us/download/dotnet) or later installed
- [Git](https://git-scm.com/downloads) installed on your local machine

## Install the Microsoft Foundry VS Code extension

Let's start by installing and setting up the VS Code extension.

1. Open Visual Studio Code.

1. Select **Extensions** from the left pane (or press **Ctrl+Shift+X**).

1. In the search bar, type **Microsoft Foundry** and press Enter.

1. Select the **Microsoft Foundry** extension from Microsoft and click **Install**.

1. After installation is complete, verify the extension appears in the primary navigation bar on the left side of Visual Studio Code.

## Sign in to Azure and create a project

Now you'll connect to your Azure resources and create a new Microsoft Foundry project.

1. In the VS Code sidebar, select the **Microsoft Foundry** extension icon.

1. In the Resources view, select **Sign in to Azure...** and follow the authentication prompts.

   > **Note**: You won't see this option if you're already signed in.

1. Create a new Foundry project by selecting the **+** (plus) icon next to **Resources** in the Foundry Extension view.

1. Select your Azure subscription from the dropdown.

1. Choose whether to create a new resource group or use an existing one:

   **To create a new resource group:**
   - Select **Create new resource group** and press Enter
   - Enter a name for your resource group (e.g., "rg-ai-agents-lab") and press Enter
   - Select a location from the available options and press Enter

   **To use an existing resource group:**
   - Select the resource group you want to use from the list and press Enter

1. Enter a name for your Foundry project (e.g., "ai-agents-project") in the textbox and press Enter.

1. Wait for the project deployment to complete. A popup will appear with the message "Project deployed successfully."

## Deploy a model

In this task, you'll deploy a model from the Model Catalog to use with your agent.

1. When the "Project deployed successfully" popup appears, select the **Deploy a model** button. This opens the Model Catalog.

   > **Tip**: You can also access the Model Catalog by selecting the **+** icon next to **Models** in the Resources section, or by pressing **F1** and running the command **Microsoft Foundry: Open Model Catalog**.

1. In the Model Catalog, locate the **gpt-4.1** model (you can use the search bar to find it quickly).

    ![Screenshot of the Model Catalog in the Foundry VS Code extension.](../Media/vs-code-model.png)

1. Select **Deploy** next to the gpt-4.1 model.

1. Configure the deployment settings:
   - **Deployment name**: Enter a name like "gpt-4.1"
   - **Deployment type**: Select **Global Standard** (or **Standard** if Global Standard is not available)
   - **Model version**: Leave as default
   - **Tokens per minute**: Leave as default

1. Select **Deploy in Microsoft Foundry** in the bottom-left corner.

1. In the confirmation dialog, select **Deploy** to deploy the model.

1. Wait for the deployment to complete. Your deployed model will appear under the **Models** section in the Resources view.

1. Right-click the name project deployment and select **Copy Project Endpoint**. You'll need this URL to connect your agent to the Foundry project in the next steps.

   <img src="../Media/vs-code-endpoint.png" alt="Screenshot of copying the project endpoint in the Foundry VS Code extension." width="550">

## Clone the starter code repository

For this exercise, you'll use starter code that will help you connect to your Foundry project and create an agent that uses custom function tools.

1. Navigate to the **Welcome** tab in VS Code (you can open it by selecting **Help > Welcome** from the menu bar).

1. Select **Clone git repository** and enter the URL of the starter code repository: `https://github.com/sonusathyadas/mslearn-ai-agents-dotnet.git`

1. Create a new folder and choose **Select as Repository Destination**, then open the cloned repository when prompted.

1. In the Explorer view, navigate to the **Labfiles/02-agent-custom-tools/dotnet** folder to find the starter code for this exercise.

1. Open the **appsettings.json** file, replace the **your_project_endpoint** placeholder with the endpoint for your project (copied from the project deployment resource in the Microsoft Foundry extension) and ensure that the MODEL_DEPLOYMENT_NAME variable is set to your model deployment name. Use **Ctrl+S** to save the file after making these changes.

Now you're ready to create an AI agent that uses MCP server tools to access external data sources and APIs.

## Create a function for the agent to use

1. Open the **Functions.cs** file and review the existing code.

    This file includes several functions that you can use as tools for your agent. The functions use sample files located inthe **data** folder to retrieve information about astronomical events and locations.

1. Find the comment **/// Returns the next visible astronomical event for a location.** and add the following code:

    ```csharp    
    /// <summary>Returns the next visible astronomical event for a location.</summary>
    public static string NextVisibleEvent(string location)
    {
        int today = int.Parse(DateTime.Now.ToString("MMdd"));
        string loc = location.ToLower().Replace(" ", "_");

        foreach (var (name, type, sortKey, dateStr, locs) in Events)
        {
            if (locs.Contains(loc) && sortKey >= today)
            {
                return JsonSerializer.Serialize(new
                {
                    @event = name,
                    type,
                    date = dateStr,
                    visible_from = locs.OrderBy(l => l).ToList()
                });
            }
        }

        return JsonSerializer.Serialize(new { message = $"No upcoming events found for {location}." });
    }
    ```

    This function checks the sample events data to find the next astronomical event that is visible from a specified location, and returns the event details as a JSON string. Next, let's create an agent that can use this function.

## Connect to the Foundry project

1. Open the **Program.cs** file.

   > **Tip**: As you add code, be sure to maintain the correct indentation. Use the comment indentation levels as a guide.

1. Find the comment **Add references** and add the following code to import the classes you'll need to build an Azure AI agent that uses a function tool:

    ```csharp
    # Add references
    using Azure.AI.Projects;
    using Azure.Identity;
    using Microsoft.Extensions.Configuration;
    using Azure.AI.Extensions.OpenAI;
    using OpenAI.Responses;
    using Azure.AI.Projects.Agents;
    using System.Text.Json;
    ```

1. Find the comment **Connect to the project client** and add the following code:

    ```csharp
    // Connect to the project client
    AIProjectClient projectClient = new(
        endpoint: new Uri(projectEndpoint),
        tokenProvider: new DefaultAzureCredential()
    );
    ```

## Define the function tools

In this task, you'll define each of the function tools that the agent can use. The parameters for each function tool are defined using a JSON schema, which specifies the name, type, description, and other attributes for each parameter of the function.

1. Find the comment **Define the event function tool** and add the following code:

    ```csharp
   // Define the event function tool
    FunctionTool nextVisibleEventTool = ResponseTool.CreateFunctionTool(
        functionName: "next_visible_event",
        functionDescription: "Get the next visible event in a given location.",
        functionParameters: BinaryData.FromObjectAsJson(new
        {
            type = "object",
            properties = new
            {
                location = new
                {
                    type = "string",
                    description = "Continent to find the next visible event in (e.g. 'north_america', 'south_america', 'australia')"
                }
            },
            required = new[] { "location" },
            additionalProperties = false
        }),
        strictModeEnabled: true
    );
    ```

1. Find the comment **Define the observation cost function tool** and add the following code:

    ```csharp
    // Define the observation cost function tool
    FunctionTool observationCostTool = ResponseTool.CreateFunctionTool(
        functionName: "calculate_observation_cost",
        functionDescription: "Calculate the cost of an observation based on the telescope tier, number of hours, and priority level.",
        functionParameters: BinaryData.FromObjectAsJson(new
        {
            type = "object",
            properties = new
            {
                telescope_tier = new { type = "string", description = "The tier of the telescope (e.g. 'standard', 'advanced', 'premium')" },
                hours = new { type = "number", description = "The number of hours for the observation" },
                priority = new { type = "string", description = "The priority level of the observation (e.g. 'low', 'normal', 'high')" }
            },
            required = new[] { "telescope_tier", "hours", "priority" },
            additionalProperties = false
        }),
        strictModeEnabled: true
    );
    ```

1. Find the comment **Define the observation report generation function tool** and add the following code:

    ```csharp
    // Define the observation report generation function tool
    FunctionTool generateObservationReportTool = ResponseTool.CreateFunctionTool(
        functionName: "generate_observation_report",
        functionDescription: "Generate a report summarizing an astronomical observation.",
        functionParameters: BinaryData.FromObjectAsJson(new
        {
            type = "object",
            properties = new
            {
                event_name = new { type = "string", description = "The name of the astronomical event being observed" },
                location = new { type = "string", description = "The location of the observer" },
                telescope_tier = new { type = "string", description = "The tier of the telescope used (e.g. 'standard', 'advanced', 'premium')" },
                hours = new { type = "number", description = "The number of hours the telescope was used" },
                priority = new { type = "string", description = "The priority level of the observation (e.g. 'low', 'normal', 'high')" },
                observer_name = new { type = "string", description = "The name of the person who conducted the observation" }
            },
            required = new[] { "event_name", "location", "telescope_tier", "hours", "priority", "observer_name" },
            additionalProperties = false
        }),
        strictModeEnabled: true
    );
    ```

## Create the agent that uses the function tools

Now that you've defined the function tools, you can create an agent that can use those tools to complete tasks.

1. Find the comment **Create a new agent with the function tools** and add the following code:

    ```csharp
    // Create a new agent with the function tools
    var agentDefinition = new PromptAgentDefinition(model: modelDeployment)
    {
        Instructions =
            "You are an astronomy observations assistant that helps users find " +
            "information about astronomical events and calculate telescope rental costs. " +
            "Use the available tools to assist users with their inquiries.",
        Tools = { nextVisibleEventTool, observationCostTool, generateObservationReportTool }
    };
    
    AgentVersion agentVersion = projectClient.Agents.CreateAgentVersion(
        agentName: agentName,
        options: new(agentDefinition));
    ```

## Send a message to the agent and process the response

Now that you've created the agent with the function tools, you can send messages to the agent and process its responses.

1. Find the comment **Create a thread for the chat session** and add the following code:

    ```csharp
   //Create a thread for the chat session
    ProjectConversation conversation = projectClient.OpenAI.Conversations.CreateProjectConversation();
    ```

    This code creates the chat session with the agent.

1. Find the comment **Get the response client for the agent and conversation** and add the following code:

    ```csharp
    // Get the response client for the agent and conversation
    ProjectResponsesClient responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(
    agentVersion.Name, conversation);
   ```

1. Find the comment **Send the user input and any function tool outputs back to the agent and get the response** and add the following code:

    ```csharp
    # Send the user input and any function tool outputs back to the agent and get the response
    response = responseClient.CreateResponse(
        model: modelDeployment,
        inputItems: inputItems);
    inputItems.Clear();
    functionCalled = false;
    foreach (ResponseItem responseItem in response.OutputItems)
    {
        inputItems.Add(responseItem);
        if (responseItem is FunctionCallResponseItem functionToolCall)
        {
            Console.WriteLine($"Calling {functionToolCall.FunctionName}...");
            inputItems.Add(AstronomyFunctions.GetResolvedToolOutput(functionToolCall));
            functionCalled = true;
        }
    }
    ```

1. Find the comment  **Delete the conversation when done** and add the following code:

    ```csharp
    // Delete the conversation when done
    projectClient.Agents.DeleteAgentVersion(
        agentName: agentName,
        agentVersion: agentVersion.Version);
    Console.WriteLine("Agent deleted.");
    ```

1. Review the complete code you've added to the file. It should now include sections that:
   - Import necessary libraries
    - Connect to the Foundry project and OpenAI client
    - Define function tools for the agent to use
    - Create an agent with those function tools
    - Send a message to the agent and retrieve the response
    - Process any function calls made by the agent and send the outputs back to the agent
    - Display the agent's response
    - Delete the agent when done

1. Save the code file (*CTRL+S*) when you have finished.

## Run the agent application

1. In the integrated terminal, enter the following command to run the application:

    ```
   dotnet run
    ```

1. When prompted, enter a prompt such as:

    ```
   Find me the next event I can see from South America and give me the cost for 5 hours of premium telescope time at normal priority.
    ```

    Notice that this prompt asks the agent to use both of the function tools you defined: `next_visible_event` and `calculate_observation_cost`. The agent is able to invoke both functions in the same conversation turn, and use the outputs from those function calls to provide a helpful response to the user.

    > **Tip**: If the app fails because the rate limit is exceeded. Wait a few seconds and try again. If there is insufficient quota available in your subscription, the model may not be able to respond.

    You should see some output similar to the folloiwng:

    ```output
    AGENT: The next astronomical event you can observe from South America is the Jupiter-Venus Conjunction, taking place on May 1st.
    The cost for 5 hours of premium telescope time at normal priority for this observation will be $1,875. 
    ```

1. Enter a follow-up prompt to generate an observation report, such as:

    ```
    Generate that information in a report for Bellows College.
    ```

    You should see a response similar to the following:

    ```output
    AGENT: Here is your report for Bellows College:

    - Next visible astronomical event: Jupiter-Venus Conjunction
    - Date: May 1st
    - Visible from: South America
    - Observation details:
        - Telescope tier: Premium
        - Duration: 5 hours
        - Priority: Normal
    - Observation cost: $1,875

    A formal report has been generated for Bellows College.
    ```

1. Enter `quit` to exit the application.

## Summary

In this exercise, you created an AI agent that uses custom function tools to retrieve information and perform calculations based on user prompts. You defined the function tools with JSON schemas to specify their parameters, and implemented the logic to process function calls made by the agent. You then ran the application and interacted with the agent to see how it used the function tools to provide helpful responses. Great work!

## Clean up

When you've finished exploring the Foundry VS Code extension, you should clean up the resources to avoid incurring unnecessary Azure costs.

### Delete your model

1. In VS Code, refresh the **Azure Resources** view.

1. Expand the **Models** subsection.

1. Right-click on your deployed model and select **Delete**.

### Delete the resource group

1. Open the [Azure portal](https://portal.azure.com).

1. Navigate to the resource group containing your Microsoft Foundry resources.

1. Select **Delete resource group** and confirm the deletion.
