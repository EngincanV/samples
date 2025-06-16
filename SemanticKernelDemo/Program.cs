using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using OpenAI;
using System.ClientModel;

namespace SemanticKernelAgentsDemo
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("🏢 Real-World AI Agents with Semantic Kernel Demo");
            Console.WriteLine("================================================\n");

            // Initialize Semantic Kernel
            var kernel = CreateKernel();

            // Demo 1: Email Assistant Agent
            await Demo1_EmailAssistant(kernel);

            // Demo 2: Task Management Agent
            await Demo2_TaskManager(kernel);

            // Demo 3: Customer Support Agent Pipeline
            await Demo3_CustomerSupportPipeline(kernel);

            Console.WriteLine("\n✅ Real-world demo completed! Press any key to exit...");
            Console.ReadKey();
        }

        private static Kernel CreateKernel()
        {
            var builder = Kernel.CreateBuilder();

            var openAiOptions = new OpenAIClientOptions()
            {
                Endpoint = new Uri("https://models.inference.ai.azure.com")
            };

            // Replace with your actual API key: https://youtu.be/fLeYZb8mpYI?feature=shared&t=204
            var credential = new ApiKeyCredential("<your-api-key>"); 
            var ghModelsClient = new OpenAIClient(credential, openAiOptions);

            builder.AddOpenAIChatCompletion("gpt-4o-mini", ghModelsClient);

            return builder.Build();
        }

        private static async Task Demo1_EmailAssistant(Kernel kernel)
        {
            Console.WriteLine("📧 Demo 1: Smart Email Assistant");
            Console.WriteLine("----------------------------------");

            // Add email and contact plugins to kernel
            kernel.Plugins.AddFromType<EmailPlugin>("email");
            kernel.Plugins.AddFromType<ContactPlugin>("contacts");

            // Create email assistant agent
            var emailAgent = new ChatCompletionAgent()
            {
                Instructions = @"You are a professional email assistant. You help users:
                - Compose professional emails
                - Send emails to contacts
                - Manage email communication
                - Suggest appropriate tone and content
                Always use available functions to send emails when requested.",
                Name = "EmailAssistant",
                Kernel = kernel
            };

#pragma warning disable SKEXP0110
            var chat = new AgentGroupChat();
#pragma warning restore SKEXP0110

            // Scenario: User wants to send a project update email
            Console.WriteLine("📝 Scenario: Sending project update to team");
            chat.AddChatMessage(new ChatMessageContent(
                AuthorRole.User,
                "Send an email to john.smith@company.com about the Q4 project status. " +
                "Let him know we're 85% complete and on track for December 15th deadline. " +
                "Include that we resolved the database performance issues."
            ));

            await foreach (var content in chat.InvokeAsync(emailAgent))
            {
                Console.WriteLine($"📧 {emailAgent.Name}: {content.Content}");
            }

            Console.WriteLine();
        }

        private static async Task Demo2_TaskManager(Kernel kernel)
        {
            Console.WriteLine("📋 Demo 2: Intelligent Task Manager");
            Console.WriteLine("-----------------------------------");

            // Add task management plugins
            kernel.Plugins.AddFromType<TaskPlugin>("tasks");
            kernel.Plugins.AddFromType<CalendarPlugin>("calendar");

            var taskAgent = new ChatCompletionAgent()
            {
                Instructions = @"You are a productivity assistant specializing in task management. You help users:
                - Create and organize tasks
                - Set priorities and deadlines
                - Schedule work based on availability
                - Provide productivity insights
                Use available functions to manage tasks and calendar events.",
                Name = "TaskManager",
                Kernel = kernel
            };

#pragma warning disable SKEXP0110
            var chat = new AgentGroupChat();
#pragma warning restore SKEXP0110

            // Scenario: Planning a product launch
            Console.WriteLine("🚀 Scenario: Planning product launch tasks");
            chat.AddChatMessage(new ChatMessageContent(
                AuthorRole.User,
                "I need to plan tasks for our new product launch in 3 weeks. " +
                "Create tasks for: market research, design mockups, development, testing, and marketing materials. " +
                "Set appropriate priorities and suggest a timeline."
            ));

            await foreach (var content in chat.InvokeAsync(taskAgent))
            {
                Console.WriteLine($"📋 {taskAgent.Name}: {content.Content}");
            }

            Console.WriteLine();
        }

        private static async Task Demo3_CustomerSupportPipeline(Kernel kernel)
        {
            Console.WriteLine("🎧 Demo 3: Customer Support Agent Pipeline");
            Console.WriteLine("-------------------------------------------");

            // Add support plugins
            kernel.Plugins.AddFromType<TicketPlugin>("tickets");
            kernel.Plugins.AddFromType<KnowledgeBasePlugin>("kb");

            // Create specialized support agents
            var triageAgent = new ChatCompletionAgent()
            {
                Instructions = @"You are a customer support triage specialist. You:
                - Analyze customer issues and categorize them
                - Determine priority levels (Low, Medium, High, Critical)
                - Route issues to appropriate teams
                - Create support tickets with proper classification",
                Name = "TriageAgent",
                Kernel = kernel
            };

            var technicalAgent = new ChatCompletionAgent()
            {
                Instructions = @"You are a technical support expert. You:
                - Provide detailed technical solutions
                - Search knowledge base for known issues
                - Escalate complex problems when needed
                - Document solutions for future reference",
                Name = "TechnicalExpert",
                Kernel = kernel
            };

            var followupAgent = new ChatCompletionAgent()
            {
                Instructions = @"You are a customer success specialist. You:
                - Follow up on resolved issues
                - Ensure customer satisfaction
                - Gather feedback for improvements
                - Close tickets when issues are resolved",
                Name = "FollowUpAgent",
                Kernel = kernel
            };

            // Simulate customer support workflow
            Console.WriteLine("🎫 Customer Issue: Database connection errors");

#pragma warning disable SKEXP0110
            var supportChat = new AgentGroupChat();
#pragma warning restore SKEXP0110

            // Step 1: Triage
            Console.WriteLine("\n🔍 Step 1: Issue Triage");
            supportChat.AddChatMessage(new ChatMessageContent(
                AuthorRole.User,
                "Customer reports: 'Our application keeps losing database connection every few hours. " +
                "This started yesterday after our server update. Multiple users affected.'"
            ));

            await foreach (var content in supportChat.InvokeAsync(triageAgent))
            {
                Console.WriteLine($"🎯 {triageAgent.Name}: {content.Content}");
            }

            // Step 2: Technical Analysis
            Console.WriteLine("\n🔧 Step 2: Technical Analysis");
            supportChat.AddChatMessage(new ChatMessageContent(
                AuthorRole.User,
                "Analyze the database connection issue and provide technical solutions"
            ));

            await foreach (var content in supportChat.InvokeAsync(technicalAgent))
            {
                Console.WriteLine($"⚙️ {technicalAgent.Name}: {content.Content}");
            }

            // Step 3: Follow-up
            Console.WriteLine("\n✅ Step 3: Customer Follow-up");
            supportChat.AddChatMessage(new ChatMessageContent(
                AuthorRole.User,
                "Create a follow-up plan for this resolved database issue"
            ));

            await foreach (var content in supportChat.InvokeAsync(followupAgent))
            {
                Console.WriteLine($"📞 {followupAgent.Name}: {content.Content}");
            }

            Console.WriteLine();
        }
    }

    // Real-world plugins
    public class EmailPlugin
    {
        [KernelFunction, Description("Send an email to specified recipient")]
        public async Task<string> SendEmail(
            [Description("Recipient email address")] string toEmail,
            [Description("Email subject")] string subject,
            [Description("Email body content")] string body,
            [Description("Sender name (optional)")] string fromName = "System")
        {
            try
            {
                // In a real implementation, you'd use actual SMTP settings
                Console.WriteLine($"📤 Sending Email:");
                Console.WriteLine($"   To: {toEmail}");
                Console.WriteLine($"   Subject: {subject}");
                Console.WriteLine($"   From: {fromName}");
                Console.WriteLine($"   Body: {body}");

                // Simulate email sending delay
                await Task.Delay(1000);

                var result = $"✅ Email sent successfully to {toEmail}";
                Console.WriteLine($"   {result}");
                return result;
            }
            catch (Exception ex)
            {
                var error = $"❌ Failed to send email: {ex.Message}";
                Console.WriteLine($"   {error}");
                return error;
            }
        }

        [KernelFunction, Description("Get email template for specific purpose")]
        public string GetEmailTemplate([Description("Template type: meeting, followup, announcement")] string templateType)
        {
            var templates = new Dictionary<string, string>
            {
                ["meeting"] = "Subject: Meeting Request\n\nHi [Name],\n\nI'd like to schedule a meeting to discuss [Topic]. Please let me know your availability.\n\nBest regards,\n[Your Name]",
                ["followup"] = "Subject: Following up on [Topic]\n\nHi [Name],\n\nI wanted to follow up on our previous discussion about [Topic]. Please let me know if you need any additional information.\n\nBest regards,\n[Your Name]",
                ["announcement"] = "Subject: Important Update: [Topic]\n\nTeam,\n\nI wanted to share an important update about [Topic]. [Details]\n\nPlease reach out if you have any questions.\n\nBest regards,\n[Your Name]"
            };

            return templates.GetValueOrDefault(templateType.ToLower(), "Template not found");
        }
    }

    public class ContactPlugin
    {
        private static readonly Dictionary<string, string> _contacts = new()
        {
            ["john"] = "john.smith@company.com",
            ["sarah"] = "sarah.jones@company.com",
            ["mike"] = "mike.wilson@company.com",
            ["team"] = "team@company.com"
        };

        [KernelFunction, Description("Get email address for a contact")]
        public string GetContactEmail([Description("Contact name or alias")] string contactName)
        {
            var email = _contacts.GetValueOrDefault(contactName.ToLower(), "Contact not found");
            Console.WriteLine($"📇 Contact lookup: {contactName} -> {email}");
            return email;
        }

        [KernelFunction, Description("List all available contacts")]
        public string ListContacts()
        {
            var contacts = string.Join(", ", _contacts.Select(c => $"{c.Key} ({c.Value})"));
            Console.WriteLine($"📇 Available contacts: {contacts}");
            return contacts;
        }
    }

    public class TaskPlugin
    {
        private static readonly List<TaskItem> _tasks = new();

        [KernelFunction, Description("Create a new task with priority and deadline")]
        public string CreateTask(
            [Description("Task title")] string title,
            [Description("Task description")] string description,
            [Description("Priority: Low, Medium, High, Critical")] string priority = "Medium",
            [Description("Due date (YYYY-MM-DD format)")] string dueDate = "")
        {
            var task = new TaskItem
            {
                Id = _tasks.Count + 1,
                Title = title,
                Description = description,
                Priority = priority,
                DueDate = string.IsNullOrEmpty(dueDate) ? null : DateTime.Parse(dueDate),
                CreatedAt = DateTime.Now,
                Status = "Pending"
            };

            _tasks.Add(task);

            var result = $"✅ Task created: #{task.Id} - {task.Title} (Priority: {task.Priority})";
            Console.WriteLine($"📋 {result}");
            return result;
        }

        [KernelFunction, Description("List all tasks with their status")]
        public string ListTasks()
        {
            if (!_tasks.Any())
            {
                return "No tasks found";
            }

            var taskList = _tasks.Select(t =>
                $"#{t.Id}: {t.Title} - {t.Status} (Priority: {t.Priority})" +
                (t.DueDate.HasValue ? $" - Due: {t.DueDate.Value:yyyy-MM-dd}" : "")
            );

            var result = "Current Tasks:\n" + string.Join("\n", taskList);
            Console.WriteLine($"📋 {result}");
            return result;
        }

        [KernelFunction, Description("Update task status")]
        public string UpdateTaskStatus(
            [Description("Task ID")] int taskId,
            [Description("New status: Pending, In Progress, Completed")] string status)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                return $"Task #{taskId} not found";
            }

            task.Status = status;
            var result = $"✅ Task #{taskId} status updated to: {status}";
            Console.WriteLine($"📋 {result}");
            return result;
        }
    }

    public class CalendarPlugin
    {
        [KernelFunction, Description("Check calendar availability for a specific date")]
        public string CheckAvailability([Description("Date to check (YYYY-MM-DD)")] string date)
        {
            // Simulate calendar check
            var checkDate = DateTime.Parse(date);
            var isWeekend = checkDate.DayOfWeek == DayOfWeek.Saturday || checkDate.DayOfWeek == DayOfWeek.Sunday;

            var availability = isWeekend ? "Limited availability (Weekend)" : "Available";
            var result = $"{date}: {availability}";
            Console.WriteLine($"📅 Calendar check: {result}");
            return result;
        }

        [KernelFunction, Description("Schedule a calendar event")]
        public string ScheduleEvent(
            [Description("Event title")] string title,
            [Description("Event date (YYYY-MM-DD)")] string date,
            [Description("Event time (HH:MM)")] string time,
            [Description("Duration in minutes")] int duration = 60)
        {
            var result = $"📅 Event scheduled: '{title}' on {date} at {time} ({duration} minutes)";
            Console.WriteLine(result);
            return result;
        }
    }

    public class TicketPlugin
    {
        private static readonly List<SupportTicket> _tickets = new();

        [KernelFunction, Description("Create a support ticket")]
        public string CreateTicket(
            [Description("Customer issue description")] string issue,
            [Description("Priority level: Low, Medium, High, Critical")] string priority,
            [Description("Category: Technical, Billing, General")] string category = "General")
        {
            var ticket = new SupportTicket
            {
                Id = $"TKT-{_tickets.Count + 1001}",
                Issue = issue,
                Priority = priority,
                Category = category,
                Status = "Open",
                CreatedAt = DateTime.Now
            };

            _tickets.Add(ticket);

            var result = $"🎫 Support ticket created: {ticket.Id} (Priority: {priority}, Category: {category})";
            Console.WriteLine(result);
            return result;
        }

        [KernelFunction, Description("Update ticket status")]
        public string UpdateTicketStatus(
            [Description("Ticket ID")] string ticketId,
            [Description("New status: Open, In Progress, Resolved, Closed")] string status)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null)
            {
                return $"Ticket {ticketId} not found";
            }

            ticket.Status = status;
            var result = $"🎫 Ticket {ticketId} status updated to: {status}";
            Console.WriteLine(result);
            return result;
        }
    }

    public class KnowledgeBasePlugin
    {
        private static readonly Dictionary<string, string> _solutions = new()
        {
            ["database connection"] = "Common solutions: 1) Check connection string, 2) Verify network connectivity, 3) Restart database service, 4) Check firewall settings, 5) Review connection pooling settings",
            ["login issues"] = "Troubleshooting steps: 1) Verify credentials, 2) Check account status, 3) Clear browser cache, 4) Try incognito mode, 5) Reset password if needed",
            ["performance"] = "Performance optimization: 1) Check system resources, 2) Analyze slow queries, 3) Review indexing strategy, 4) Monitor memory usage, 5) Consider caching solutions"
        };

        [KernelFunction, Description("Search knowledge base for solutions")]
        public string SearchSolutions([Description("Search term or issue description")] string query)
        {
            var matchedSolution = _solutions.FirstOrDefault(kvp =>
                query.ToLower().Contains(kvp.Key.ToLower())).Value;

            var result = matchedSolution ?? "No specific solution found. Please escalate to technical team.";
            Console.WriteLine($"📚 Knowledge Base Search: {query} -> {(matchedSolution != null ? "Solution found" : "No solution")}");
            return result;
        }
    }

    // Data models
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "Pending";
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SupportTicket
    {
        public string Id { get; set; } = "";
        public string Issue { get; set; } = "";
        public string Priority { get; set; } = "Medium";
        public string Category { get; set; } = "General";
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; }
    }
}

