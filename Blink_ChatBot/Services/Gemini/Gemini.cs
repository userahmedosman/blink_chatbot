using Blink_ChatBot.DTOs;
using Microsoft.Extensions.AI;

namespace Blink_ChatBot.Services.Gemini
{
    public class Gemini : IChatbot
    {
        private const string SystemPrompt = """
            You are an assistant that makes very short and concise responses.
            You must answer in a single sentence and avoid unnecessary details.
            If a tool fails, read the error message and try to fix the issue in your next response.
            """;

        private readonly List<ChatMessage> messages = [];
        private readonly List<ChatMessageUI> conversation = [];
        private readonly IChatClient chat;
        private readonly ChatOptions chatOptions;
        public Gemini(IChatClient chatClient, ChatOptions chatOptions)
        {
            this.chat = chatClient;
            this.chatOptions = chatOptions;
            messages.Add(new ChatMessage(ChatRole.System, SystemPrompt));
        }

        public List<ChatMessageUI> Conversation => conversation;

        public bool IsProcessing { get; private set; }

        public ApprovalRequestUI? PendingApproval { get; private set; }

        public event Action? OnChange;

        public void StartNewConversation()
        {
            if (IsProcessing)
            {
                throw new InvalidOperationException("A new conversation cannot start while a response is being generated.");
            }

            conversation.Clear();
            messages.Clear();
            messages.Add(new ChatMessage(ChatRole.System, SystemPrompt));
            NotifyChange();
        }

        public void CancelCurrentResponse()
        {
            throw new NotImplementedException();
        }

        public async Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
        {
            if (PendingApproval is null || IsProcessing)
            {
                return;
            }

            IsProcessing = true;
            ChatMessageUI? assistantMessage = null;

            try
            {
                var approvalResponse = PendingApproval.ApprovalRequest.CreateResponse(approved);
                messages.Add(new ChatMessage(ChatRole.User, [approvalResponse]));
                PendingApproval = null;

                Conversation.Add(new ChatMessageUI
                {
                    Role = MessageRole.System,
                    Text = $"Tool approval {(approved ? "approved" : "denied")}."
                });

                assistantMessage = new ChatMessageUI { Role = MessageRole.AI, Text = string.Empty };
                Conversation.Add(assistantMessage);
                NotifyChange();

                await SendMessagesToAssistant(assistantMessage, cancellationToken);
            }
            catch
            {
                if (assistantMessage is not null && string.IsNullOrEmpty(assistantMessage.Text))
                {
                    conversation.Remove(assistantMessage);
                }

                NotifyChange();
                throw;
            }
            finally
            {
                IsProcessing = false;
                NotifyChange();
            }
        }

        public async Task SendMessageAsync(string userText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userText))
            {
                throw new ArgumentException("A message cannot be empty.", nameof(userText));
            }

            if (IsProcessing || PendingApproval != null)
            {
                throw new InvalidOperationException("A response is already being generated.");
            }

            IsProcessing = true;
            var assistantMessage = new ChatMessageUI { Role = MessageRole.AI };
            conversation.Add(new ChatMessageUI { Role = MessageRole.User, Text = userText });
            conversation.Add(assistantMessage);
            messages.Add(new ChatMessage(ChatRole.User, userText));
            NotifyChange();

            try
            {
                await SendMessagesToAssistant(assistantMessage, cancellationToken);
            }
            catch
            {
                if (string.IsNullOrEmpty(assistantMessage.Text))
                {
                    conversation.Remove(assistantMessage);
                }

                NotifyChange();
                throw;
            }
            finally
            {
                IsProcessing = false;
                NotifyChange();
            }
        }

        private async Task SendMessagesToAssistant(ChatMessageUI assistantMessage, CancellationToken cancellationToken)
        {
            var updates = new List<ChatResponseUpdate>();

            await foreach (var update in chat.GetStreamingResponseAsync(messages, chatOptions, cancellationToken))
            {
                updates.Add(update);

                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        assistantMessage.Text += textContent.Text;
                        NotifyChange();
                    }
                }
            }

            var response = updates.ToChatResponse();
            messages.AddMessages(response);

            var approvalRequest = response.Messages
                .SelectMany(m => m.Contents)
                .OfType<ToolApprovalRequestContent>()
                .FirstOrDefault();

            if(approvalRequest is not null)
            {
                if(approvalRequest.ToolCall is FunctionCallContent functionCall)
                {
                    PendingApproval = new ApprovalRequestUI
                    {
                        ApprovalRequest = approvalRequest,
                        ToolName = ConvertToolName(functionCall.Name),
                        Arguments = functionCall.Arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object?>()
                    };
                }

                if (string.IsNullOrWhiteSpace(conversation[^1].Text))
                {
                    conversation.RemoveAt(conversation.Count - 1);
                }
                NotifyChange();
                return;
            }
        }

        private void NotifyChange() => OnChange?.Invoke();

        private static string ConvertToolName(string toolName)
        {
            return toolName switch
            {
                "get_weather" => "Get Weather",
                "evaluate_weather" => "Evaluate Weather",
                "send_email" => "Send Email",
                _ => toolName
            };
        }
    }
}
