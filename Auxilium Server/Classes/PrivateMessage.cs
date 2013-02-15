using System;
using System.Collections.Generic;

namespace Auxilium_Server.Classes
{
    public class PrivateMessage
    {
        public DateTime Time;
        public ushort Id;
        public string Sender;
        public string Subject;
        public List<string> Message = new List<string>();
        public PrivateMessage(ushort id, string subject, string sender, DateTime time, string message)
        {
            Id = id;
            Subject = subject;
            Sender = sender;
            Time = time;
            Message.Add(message);
        }
    }
}
