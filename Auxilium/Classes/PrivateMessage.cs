using System;
using System.Text;
using System.Collections.Generic;

namespace Auxilium.Classes
{
    public class PrivateMessage
    {
        public DateTime Time;
        public ushort Id;
        public string Sender;
        public string Recipient;
        public string Subject;
        public List<Message> Messages = new List<Message>(); 

        public PrivateMessage(ushort id, string subject, string sender, string recipient, DateTime time, string message = null)
        {
            Id = id;
            Subject = subject;
            Sender = sender;
            Time = time;
            Recipient = recipient;
            if (message != null)
                Messages.Add(new Message(sender, message));
        }
        public string GetMessages()
        {
            StringBuilder ret = new StringBuilder();

            string spacer = "_____________________________________________________";
            foreach (Message m in Messages)
            {
                ret.AppendFormat("{0}\n{1}\n{2}\n", "From: " + m.Sender, m.MessageText, "_____________________________________________________");
            }
            ret.Remove(ret.Length - (spacer.Length + 2), (spacer.Length + 2));
            return ret.ToString();
        }

        
    }
    public class Message
    {
        public string Sender;
        public string MessageText;
        public Message(string sender, string messageText)
        {
            Sender = sender;
            MessageText = messageText;
        }
    }
}
