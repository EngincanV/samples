using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var baseUrl = "https://api.openai.com/v1/";
var apiKey = configuration["OpenAI:ApiKey"];

var chatClient = new OpenAIClient(
    new ApiKeyCredential(apiKey!),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(baseUrl),
    }
).GetChatClient("gpt-5.2");

//1. Writer Agent
AIAgent writerAgent = chatClient.CreateAIAgent(
    instructions:
        "You are a release notes writer. " +
        "Given raw commit messages and a list of changes, " +
        "group them and produce concise technical release notes for developers."
);

//2. Reviewer Agent
AIAgent reviewerAgent = chatClient.CreateAIAgent(
    instructions:
        "You are a product marketing writer. " +
        "You receive technical release notes and turn them into " +
        "customer-friendly bullets for end-users, keeping it short and clear."
);

//3. Workflow: Writer -> Reviewer
WorkflowBuilder builder = new(writerAgent);
builder.AddEdge(writerAgent, reviewerAgent); //Writer -> Reviewer
var workflow = builder.Build();

//Example Raw Changes
string rawChanges = """
            - Fix: AgentThread serialization bug when using approval-required tools
            - Feature: Add MCP GitHub integration sample to docs
            - Change: Improve workflow event logging and tracing
            - Fix: NullReferenceException in custom executor error path
            """;

// 4. Run the workflow in streaming mode
var run = await InProcessExecution.StreamAsync(
    workflow,
    new ChatMessage(ChatRole.User,
        "Here is a list of internal changes. " +
        "First, write technical release notes. " +
        "Then refine them for customers.\n\n" + rawChanges)
);


// In this simple example we just print agent updates as they appear.
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

Console.WriteLine("\n=== Workflow output (streamed) ===");
Console.WriteLine();

await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is AgentRunUpdateEvent update)
    {
        Console.Write(update.Data);
    }
}

Console.WriteLine("\n\nDone.");