
using Unity.Networking.Transport;

public class WelcomeMsg : Message
{
    public int player {set; get;}
    public WelcomeMsg(){ //Creating a message
        code = OperationCode.WELCOME;
    }
     public WelcomeMsg(DataStreamReader reader){ //Receiving a message
        code = OperationCode.WELCOME;
        deserialize(reader);
    }

    public override void serialize(ref DataStreamWriter writer)
    {
       writer.WriteByte((byte)code);
       writer.WriteInt(player);
    }
    public override void deserialize(DataStreamReader reader)
    {
        player = reader.ReadInt();
    }

    public override void receivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }
    public override void receivedOnServer(NetworkConnection connection)
    {
       NetUtility.S_WELCOME?.Invoke(this, connection);
    }
}
