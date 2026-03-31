
# Develop an AI agent with Model Context Protocol (MCP) tools

In this exercise, you'll use the Microsoft Foundry VS Code extension to create an agent that can use Model Context Protocol (MCP) server tools to access external data sources and APIs. The agent will be able to retrieve up-to-date information and interact with custom services through MCP tools.

This exercise should take approximately **60** minutes to complete.

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

For this exercise, you'll use starter code that will help you connect to your Foundry project and create an agent that uses MCP server tools.

1. Navigate to the **Welcome** tab in VS Code (you can open it by selecting **Help > Welcome** from the menu bar).

1. Select **Clone git repository** and enter the URL of the starter code repository: `https://github.com/sonusathyadas/mslearn-ai-agents-dotnet.git`

1. Create a new folder and choose **Select as Repository Destination**, then open the cloned repository when prompted.

1. In the Explorer view, navigate to the **Labfiles/03-mcp-integration/dotnet/MCPIntegrationDemo** folder to find the starter code for this exercise.

1. Right-click on the **appsettings.json** file and select **Open in Integrated Terminal**.

1. In the terminal, enter the following command to restore the dotnet packages:

    ```
    dotnet restore
    ```

1. Open the **appsettings.json** file, replace the **your_project_endpoint** placeholder with the endpoint for your project (copied from the project deployment resource in the Microsoft Foundry extension) and ensure that the MODEL_DEPLOYMENT_NAME variable is set to your model deployment name. Use **Ctrl+S** to save the file after making these changes.

Now you're ready to create an AI agent that uses MCP server tools to access external data sources and APIs.

## Connect an Azure AI Agent to a remote MCP server

In this task, you'll connect to a remote MCP server, prepare the AI agent, and run a user prompt.

1. Open the **Program.cs** file in the code editor.

1. Find the comment **Add references** and add the following code to import the classes:

    ```csharp
    // Add references
    using Azure.AI.Extensions.OpenAI;
    using Azure.AI.Projects;
    using Azure.AI.Projects.Agents;
    using Azure.Identity;
    using Microsoft.Extensions.Configuration;
    using OpenAI.Responses;
    ```

1. Find the comment **Connect to the Projects client** and add the following code to connect to the Azure AI project using the current Azure credentials.

    ```csharp
    // Connect to the Projects client
    AIProjectClient projectClient = new(
        endpoint: new Uri(projectEndpoint),
        tokenProvider: new DefaultAzureCredential()
    );
    ```

1. Under the comment **Create agent with MCP tool**, add the following code:

    ```csharp
    // Create agent with MCP tool
    var agentDefinition = new PromptAgentDefinition(model: modelDeployment)
    {
        Instructions =
            "You are an astronomy observations assistant that helps users find " +
            "information about astronomical events and calculate telescope rental costs. " +
            "Use the available tools to assist users with their inquiries.",
        Tools =
        {
            ResponseTool.CreateMcpTool(
            serverLabel: "api-specs",
            serverUri: new Uri("https://learn.microsoft.com/api/mcp"),
            toolCallApprovalPolicy: new McpToolCallApprovalPolicy(GlobalMcpToolCallApprovalPolicy.AlwaysRequireApproval))
        }
    };

    AgentVersion agentVersion = projectClient.Agents.CreateAgentVersion(
        agentName: agentName,
        options: new(agentDefinition)
    );
    ```

    This code will connect to the Microsft Learn Docs remote MCP server. This is a cloud-hosted service that enables clients to access trusted and up-to-date information directly from Microsoft's official documentation.

1. Find the comment **Create a conversation thread** and add the following code:

    ```csharp
    //Create a conversation thread
    ProjectConversation conversation = projectClient.OpenAI.Conversations.CreateProjectConversation();
    ```
1. 1. Find the comment **Get the response client for the agent and conversation** and add the following code:
    ```csharp
    // Get the response client for the agent and conversation
    ProjectResponsesClient responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(
    agentVersion.Name, conversation);
    ```
    

1. Find the comment **Send initial request that will trigger the MCP tool** and add the following code:

    ```csharp
    // Send initial request that will trigger the MCP tool
    var inputItems = new List<ResponseItem> {
        ResponseItem.CreateUserMessageItem("Give me the Azure CLI commands to create an Azure Container App with a managed identity.")
    };
    
    ResponseResult latestResponse = responseClient.CreateResponse(inputItems);
    ```

1. Find the comment **Process any MCP approval requests that were generated** and add the following code:

    ```csharp
    // Process any MCP approval requests that were generated
    foreach (ResponseItem responseItem in latestResponse.OutputItems)
    {
        if (responseItem is McpToolCallApprovalRequestItem mcpToolCall)
        {
            if (string.Equals(mcpToolCall.ServerLabel, "api-specs"))
            {
                Console.WriteLine($"Approving {mcpToolCall.ServerLabel}...");
                // Automatically approve the MCP request to allow the agent to proceed
                // In production, you might want to implement more sophisticated approval logic
                inputItems.Add(ResponseItem.CreateMcpApprovalResponseItem(approvalRequestId: mcpToolCall.Id, approved: true));
            }
            else
            {
                Console.WriteLine($"Rejecting unknown call {mcpToolCall.ServerLabel}...");
                inputItems.Add(ResponseItem.CreateMcpApprovalResponseItem(approvalRequestId: mcpToolCall.Id, approved: false));
            }
        }
    }
    ```

    This code listens for any MCP approval requests in the agent's response and automatically approves them.

1. Find the comment **Send the approval response back and retrieve a response** and add the following code:

    ```csharp
    // Send the approval response back and retrieve a response
    latestResponse = responseClient.CreateResponse(inputItems);
    Console.WriteLine(latestResponse.GetOutputText());
    ```

1. Find the comment **Clean up resources by deleting the agent version** and add the following code:

    ```csharp
    // Clean up resources by deleting the agent version
    projectClient.Agents.DeleteAgentVersion(agentName: agentVersion.Name, agentVersion: agentVersion.Version);
    ```

1. Save the code file (*CTRL+S*) when you're finished.

### Run the application

1. In the integrated terminal, enter the following command to run the application:

    ```
   dotnet run
    ```

1. Wait for the agent to process the prompt, using the MCP server to find a suitable tool to retrieve the requested information. You should see some output similar to the following:

    ```
    Agent created (id: MyAgent:2, name: MyAgent, version: 2)
    Created conversation (id: conv_086911ecabcbc05700BBHIeNRoPSO5tKPHiXRkgHuStYzy27BS)
    Final input:
    [{'type': 'mcp_approval_response', 'approve': True, 'approval_request_id': '{approval_request_id}'}]

    Agent response: Here are Azure CLI commands to create an Azure Container App with a managed identity:

    **1. For a System-assigned Managed Identity**
    ```sh
    az containerapp create \
    --name <CONTAINERAPP_NAME> \
    --resource-group <RESOURCE_GROUP> \
    --environment <CONTAINERAPPS_ENVIRONMENT> \
    --image <CONTAINER_IMAGE> \
    --identity 'system'
    ```

    [continued...]

    Agent deleted

    ```

    Notice that the agent was able to invoke the MCP tool to automatically fulfill the request.

1. You can update the input in the request to ask for different information. In each case, the agent will attempt to find technical documentation by using the MCP tool.


## Summary

In this exercise, you created AI agents that can use Model Context Protocol (MCP) server tools to access external data sources and APIs. You connected your agents to a remote MCP server hosted by Microsoft Learn Docs and a custom MCP server that you implemented. By integrating these tools, the agent was able to retrieve up-to-date information and provide informed responses to user prompts. This demonstrates how MCP tools can significantly enhance the capabilities of AI agents, enabling them to perform a wide range of tasks by leveraging external services and data. Great work!

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
