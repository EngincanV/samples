using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using OpenAI;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

//1. Create MCP Client for GithubServer
var mcpClient = await McpClient.CreateAsync(
    new StdioClientTransport(new()
    {
        Name = "Github MCP",
        Command = "npx",
        Arguments = new[] { "-y", "@modelcontextprotocol/server-github" },
    })
);

//2. List MCP Tools and use them as tools!
var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

Console.WriteLine($"✅ Found {mcpTools.Count} tools from MCP Server:");
foreach (var tool in mcpTools)
{
    Console.WriteLine($"   • {tool.Name}");
}

//3. Create ChatClient
var baseUrl = "https://models.inference.ai.azure.com";
var apiKey = configuration["OpenAI:ApiKey"];

var chatClient = new AzureOpenAIClient(
    new Uri(baseUrl),
    new ApiKeyCredential(apiKey!)
).GetChatClient("gpt-4o-mini");

//4. Create AI Agent
AIAgent agent = chatClient.CreateAIAgent(
                    instructions: @"You are a helpful GitHub assistant. 
                                   You can answer questions about GitHub repositories, 
                                   commits, issues, and pull requests. 
                                   Always provide clear and concise information.",
                    tools: [.. mcpTools.Cast<AITool>()]
                );

Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("                   AGENT INTERACTIONS                      ");
Console.WriteLine("═══════════════════════════════════════════════════════════\n");


try
{
    AgentThread thread = agent.GetNewThread();
    // thread = null; //makes the agent forget the conversation history!

    var result = await agent.RunAsync("Summarize the last commit of the abpframework/abp repository", thread);
    Console.WriteLine(result.Text);

    Console.WriteLine("Last commit author: ");
    var result2 = await agent.RunAsync("Who is the last commit author of the abpframework/abp repository? Answer without tool call!", thread);
    Console.WriteLine(result2.Text);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error running agent: {ex.Message}");
}

