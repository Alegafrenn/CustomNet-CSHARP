namespace CustomNet
{
    public enum Connected
    {
        Success,
        Server_Full,
        No_Connection,
        Already_Connected
    }
    public enum Disconnected
    {
        By_Client,
        By_Server,
        Lost_Connection,
        Server_Closed
    }
}