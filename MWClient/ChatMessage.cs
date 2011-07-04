using System;
using System.Collections.Generic;
using System.Text;

//TODO Code all the files like in this one
namespace MineWorld
{
    public class ChatMessage
    {
        public string Message;
        public ChatMessageType Type;
        public float TimeStamp;
        public string Author;

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
