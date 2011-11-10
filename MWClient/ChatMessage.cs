namespace MineWorld
{
    public class ChatMessage
    {
        public string Author;
        public string Message;
        public float TimeStamp;
        public ChatMessageType Type;

        public ChatMessage(string message, ChatMessageType type, string author)
        {
            Message = message;
            Type = type;
            //Set default to 10 seconds
            TimeStamp = 10;
            Author = author;
        }
    }
}