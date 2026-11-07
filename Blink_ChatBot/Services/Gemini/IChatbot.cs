using Blink_ChatBot.DTOs;
using Microsoft.Extensions.AI;

namespace Blink_ChatBot.Services.Gemini
{
    public interface IChatbot
    {
        List<ChatMessageUI> Conversation { get; }
        bool IsProcessing { get; }

        ApprovalRequestUI? PendingApproval { get; }
        event Action? OnChange;

        void StartNewConversation();
        void CancelCurrentResponse();
        Task SendMessageAsync(string userText, CancellationToken cancellationToken = default);
        Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default);
    }
}
