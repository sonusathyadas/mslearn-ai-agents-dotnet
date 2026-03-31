
# Build AI agents with portal and VS Code

In this exercise, you'll build a complete AI agent solution using both the Microsoft Foundry portal and the Microsoft Foundry VS Code extension. You'll start by creating a basic agent in the portal with grounding data and built-in tools, then interact with it programmatically using VS Code to leverage advanced capabilities like code interpreter for data analysis.

This exercise takes approximately **45** minutes.

> **Note**: Some of the technologies used in this exercise are in preview or in active development. You may experience some unexpected behavior, warnings, or errors.

## Learning Objectives

By the end of this exercise, you'll be able to:

1. Create and configure an AI agent in the Microsoft Foundry portal
2. Add grounding data and enable built-in tools (file search, code interpreter)
3. Use the Microsoft Foundry VS Code extension to work with agents programmatically
4. Leverage code interpreter to analyze data and generate insights
5. Understand when to use portal-based vs code-based approaches for agent development

## Prerequisites

Before starting this exercise, ensure you have:

- An [Azure subscription](https://azure.microsoft.com/free/) with sufficient permissions and quota to provision Azure AI resources
- [Visual Studio Code](https://code.visualstudio.com/) installed on your local machine
- [.NET 8 or later](https://dotnet.microsoft.com/en-us/download/dotnet) or later installed
- [Git](https://git-scm.com/downloads) installed on your local machine
- Basic familiarity with Azure AI services and Python programming

## Scenario

You'll build an **IT Support Agent** that helps employees with common technical issues. The agent will:

- Answer questions based on IT policy documentation (grounding data)
- Use built-in tools like file search to find relevant information
- Analyze system performance data using code interpreter to identify trends and issues

---

## Create an AI agent in Microsoft Foundry portal

Let's start by creating a Foundry project and a basic agent using the portal.

### Create a Foundry project

1. In a web browser, open the [Foundry portal](https://ai.azure.com) at `https://ai.azure.com` and sign in using your Azure credentials. Close any tips or quick start panes that are opened the first time you sign in, and if necessary use the **Foundry** logo at the top left to navigate to the home page.

    > **Important**: For this lab, you're using the **New** Foundry experience.

1. In the top banner, select **Start building** to try the new Microsoft Foundry Experience.

1. When prompted, create a **new** project, and enter a valid name for your project (e.g., `it-support-agent-project`).

1. Expand **Advanced options** and specify the following settings:
    - **Microsoft Foundry resource**: *A valid name for your Foundry resource*
    - **Region**: *Select one available near you*\**
    - **Subscription**: *Your Azure subscription*
    - **Resource group**: *Select your resource group, or create a new one*

    > \* Some Azure AI resources are constrained by regional model quotas. In the event of a quota limit being exceeded later in the exercise, there's a possibility you may need to create another resource in a different region.

1. Select **Create** and wait for your project to be created.

1. When your project is created, select **Start building**, and select **Create agent** from the drop-down menu.

1. Set the **Agent name** to `it-support-agent` and create the agent.

The playground will open for your newly created agent. You'll see that an available deployed model is already selected for you.

### Configure your agent with instructions and grounding data

Now that you have an agent created, let's configure it with instructions and add grounding data.

1. In the agent playground, set the **Instructions** to:

    ```prompt
    You are an IT Support Agent for Contoso Corporation.
    You help employees with technical issues and IT policy questions.
    
    Guidelines:
    - Always be professional and helpful
    - Use the IT policy documentation to answer questions accurately
    - If you don't know the answer, admit it and suggest contacting IT support directly
    - When creating tickets, collect all necessary information before proceeding
    ```

1. Download the IT policy document from the lab repository. Open a new browser tab and navigate to:

    ```
    Labfiles/01-build-agent-portal-and-vscode/IT_Policy.txt
    ```

    Save the file to your local machine.

    > **Note**: This document contains sample IT policies for password resets, software installation requests, and hardware troubleshooting.

1. Return to the agent playground. In the **Tools** section, enable both **File search** and **Code interpreter**.

1. Under **File search**, select **Upload files** and upload the `IT_Policy.txt` file you just downloaded.

1. Wait for the file to be indexed. You'll see a confirmation when it's ready.

1. Now let's add some performance data for the code interpreter to analyze. Download the system performance data file from:

    ```
    Labfiles/01-build-agent-portal-and-vscode/system_performance.csv
    ```

    Save this file to your local machine.

1. Under **Code interpreter**, select **Upload files** and upload the `system_performance.csv` file you just downloaded.

    > **Note**: This CSV file contains simulated system metrics (CPU, memory, disk usage) over time that the agent can analyze.

### Test your agent

Let's test the agent to see how it responds using the grounding data.

1. In the chat interface on the right side of the playground, enter the following prompt:

    ```
    What's the policy for password resets?
    ```

1. Review the response. The agent should reference the IT policy document and provide accurate information about password reset procedures.

1. Try another prompt:

    ```
    How do I request new software?
    ```

1. Again, review the response and observe how the agent uses the grounding data.

1. Now test the code interpreter with a data analysis request:

    ```
    Can you analyze the system performance data and tell me if there are any concerning trends?
    ```

1. The agent should use the code interpreter to analyze the CSV file and provide insights about system performance.

1. Try asking for a visualization:

    ```
    Create a chart showing CPU usage over time from the performance data
    ```

1. The agent will use code interpreter to generate visualizations and analysis.

Great! You've created an agent with grounding data, file search, and code interpreter capabilities. In the next section, you'll interact with this agent programmatically using VS Code.

---

## Interact with your agent using VS Code

Now you'll use the Microsoft Foundry VS Code extension to work with your agent programmatically and see how to interact with it from code.

### Install and configure the VS Code extension

If you already have installed the extension for Foundry, you can skip this section.

1. Open Visual Studio Code on your local machine.

1. Select **Extensions** from the left pane (or press **Ctrl+Shift+X**).

1. In the search bar, type **Microsoft Foundry** and press Enter.

1. Select the **Microsoft Foundry** extension from Microsoft and click **Install**.

1. After installation is complete, verify the extension appears in the primary navigation bar on the left side.

### Connect to your Foundry project

1. In the VS Code sidebar, select the **Microsoft Foundry** extension icon.

1. In the Resources view, select **Sign in to Azure...** and follow the authentication prompts.

    > **Note**: You won't see this option if you're already signed in.

1. After signing in, expand your subscription in the Resources view.

1. Locate and expand your Foundry resource, then find the project you created earlier (`it-support-agent-project`).

1. Right-click on your project and select **Set as active project**.

1. Expand your project in the Resources view and verify you can see your `it-support-agent` listed under **Prompt agents**.

### Test your agent in VS Code

Before writing any code, you can interact with your agent directly in the extension interface.

1. In the Resources view, expand **Declarative agents** under your project and double-click **it-support-agent** to open it in the VS Code agent playground.

1. In the chat pane, type a question such as:

    ```
    What is the policy for reporting a lost or stolen device?
    ```

1. Review the agent's response. It should use the grounding data you uploaded earlier to provide relevant IT policy information.

    > **Tip**: You can use this built-in playground to quickly test your agent's instructions and knowledge without writing any code.

### Create a .NET application

Now let's create a .NET application that interacts with your agent programmatically.

1. In VS Code, open the Command Palette (**Ctrl+Shift+P** or **View > Command Palette**).

1. Type **Git: Clone** and select it from the list.

1. Enter the repository URL:

    ```
    https://github.com/sonusathyadas/mslearn-ai-agents-dotnet.git
    ```

1. Choose a location on your local machine to clone the repository.

1. When prompted, select **Open** to open the cloned repository in VS Code.

1. Once the repository opens, select **File > Open Folder** and navigate to `mslearn-ai-agents-dotnet/Labfiles/01-build-agent-portal-and-vscode/dotnet`, then click **Select Folder**.

1. In the Explorer pane, open the `agent_with_functions.py` file. You'll see it's currently empty.

1. Add the following code to the file:

    ```csharp
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
    var agentVersion = Environment.GetEnvironmentVariable("AGENT_VERSION") ?? "1";
    
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
    
    OpenAI.Files.OpenAIFileClient fileClient = projectClient.OpenAI.GetOpenAIFileClient();
    
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
    ```

### Configure environment and run the application

1. In the Explorer pane, you'll see `appsettings.json` file already present in the folder.

1. In the `appsettings.json` file, replace `your_project_endpoint_here` with your actual project endpoint:

    ```
    PROJECT_ENDPOINT=<your_project_endpoint>
    AGENT_NAME=it-support-agent
    ```

    **To get your project endpoint:** In VS Code, open the **Microsoft Foundry** extension, right-click on your active project, and select **Copy Endpoint**.

1. Save the `appsettings.json` file (**Ctrl+S** or **File > Save**).

1. Open a terminal in VS Code (**Terminal > New Terminal**).

1. Install the required packages:

    ```bash
    dotnet build
    ```

1. Run the application:

    ```bash
    dotnet run
    ```

### Test the agent with code interpreter

When the agent starts, try these prompts to test different capabilities:

1. Test policy search with file search:

    ```
    What's the policy for password resets?
    ```

2. Request data analysis with code interpreter:

    ```
    Analyze the system performance data and identify any periods where CPU usage exceeded 80%
    ```
3. Ask for statistical analysis:

    ```
    What are the average, minimum, and maximum values for disk usage in the performance data?
    ```

---

## Portal vs Code: When to use each approach

Now that you've worked with both approaches, here's guidance on when to use each:

### Use the Portal when

- Rapid prototyping and testing agent configurations
- Quick adjustments to instructions and system prompts
- Testing with grounding data and built-in tools
- Demonstrating concepts to stakeholders
- You need a quick agent without writing code

### Use VS Code / SDK when

- Building production applications
- Integrating agents with existing code and systems
- Managing conversations and responses programmatically
- Version control and CI/CD pipelines
- Advanced orchestration and multi-agent scenarios
- Programmatic agent management at scale

### Hybrid Approach (Best Practice)

1. **Prototype** in the portal to validate concepts
2. **Develop** in VS Code for production implementation
3. **Monitor and iterate** using both tools

---

## Cleanup

To avoid unnecessary Azure charges, delete the resources you created:

1. In the Foundry portal, navigate to your project
1. Select **Settings** > **Delete project**
1. Alternatively, delete the entire resource group from the Azure portal

---

## Troubleshooting

### Common Issues

**Issue**: "Project endpoint invalid"

- **Solution**: Ensure you copied the full project endpoint from the portal. It should start with `https://` and include your project details.

**Issue**: "Agent not found"

- **Solution**: Make sure you set the correct project as active in the VS Code extension.

**Issue**: "Code interpreter not generating visualizations"

- **Solution**: Ensure the CSV file was properly uploaded to the agent and that code interpreter is enabled in the agent settings.

---

## Summary

In this exercise, you:

Created an AI agent in the Microsoft Foundry portal with grounding data  
Enabled built-in tools like file search and code interpreter  
Connected to your project using the VS Code extension  
Interacted with the agent programmatically using Python  
Leveraged code interpreter for data analysis and visualization  
Learned when to use portal vs code-based approaches  

You now have the foundational skills to build AI agents using both visual and code-based workflows!

## Next Steps

Ready to take your agent development skills to the next level? Continue with:

- **Lab 2: Advanced Tool Calling** - Learn to use advanced tool calling for dynamic data processing, implement advanced async function patterns, and master file operations with batch processing.

### Additional Resources

- [Azure AI Agent Service Documentation](https://learn.microsoft.com/azure/ai-services/agents/)
- [Microsoft Foundry VS Code Extension](https://marketplace.visualstudio.com/items?itemName=ms-toolsai.vscode-ai)
- [Azure AI Projects SDK](https://learn.microsoft.com/python/api/overview/azure/ai-projects-readme)
