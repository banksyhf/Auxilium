namespace Auxilium_Server.Classes
{
    public enum ServerPacket : byte
    {
        SignIn,
        Register,
        UserList,
        UserJoin,
        UserLeave,
        ChannelList,
        Motd,
        Chatter,
        GlobalMsg,
        BanList,
        Pm,
        KeepAlive,
        WakeUp,
        RecentMessages,
        News,
        ViewProfile,
        Profile,
        EditProfile,
        ClearChat
    }

    enum ClientPacket : byte
    {
        SignIn,
        Register,
        Channel,
        ChatMessage,
        PM,
        KeepAlive,
        News,
        ViewProfile,
        EditProfile
    }
}
