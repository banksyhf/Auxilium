namespace Auxilium_Server.Classes
{
    class ChatMessage
    {
        public readonly string Time;
        public readonly string Username;
        public readonly string Value;
        public readonly byte Rank;

        public ChatMessage(string time, string value, string username, byte rank)
        {
            Time = time;
            Value = value;
            Username = username;
            Rank = rank;
        }
    }
}
