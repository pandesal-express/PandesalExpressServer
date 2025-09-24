namespace Shared.Dtos;

public class TransferMessageDto
{
    public string Id { get; set; }
    public string TransferRequestId { get; set; }
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string Message { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsSystemMessage { get; set; }
}