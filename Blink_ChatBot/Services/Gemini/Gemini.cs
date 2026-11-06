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

        public Gemini(IChatClient chatClient)
        {
            chat = chatClient;
            messages.Add(new ChatMessage(ChatRole.System, SystemPrompt));
        }

        public List<ChatMessageUI> Conversation => conversation;

        public bool IsProcessing { get; private set; }

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

        public Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task SendMessageAsync(string userText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userText))
            {
                throw new ArgumentException("A message cannot be empty.", nameof(userText));
            }

            if (IsProcessing)
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

            await foreach (var update in chat.GetStreamingResponseAsync(messages, cancellationToken: cancellationToken))
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
        }

        private void NotifyChange() => OnChange?.Invoke();
    }
}
