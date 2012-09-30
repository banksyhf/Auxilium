using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auxilium.Classes
{
    public class PrivateMessage
    {
        public DateTime Time;
        public string Sender;
        public string Subject;
        public string Message;
        public PrivateMessage(string subject, string sender, DateTime time, string message)
        {
            Subject = subject;
            Sender = sender;
            Time = time;
            Message = message;
        }
    }
}
