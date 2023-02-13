using Unity.Networking.Transport;

public class KeepAliveMsg : Message
{
    public KeepAliveMsg()
    {
        code = OperationCode.KEEP_ALIVE;
    }

    public KeepAliveMsg(DataStreamReader reader)
    {
        code = OperationCode.KEEP_ALIVE;
        deserialize(reader);
    }

    public override void serialize(ref DataStreamWriter writer)
    {
       writer.WriteByte((byte) code);
    }
    public override void deserialize(DataStreamReader reader)
    {
      //Keep alive msg does not contain a message
    }

    public override void receivedOnClient()
    {
       NetUtility.C_KEEP_ALIVE?.Invoke(this);
    }
      public override void receivedOnServer(NetworkConnection connection)
    {
       NetUtility.S_KEEP_ALIVE?.Invoke(this, connection);
    }
}
