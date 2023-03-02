
using Unity.Networking.Transport;
using UnityEngine;

public class StartGameMsg : Message
{
    public int player { set; get; }
    public StartGameMsg()
    {
        code = OperationCode.START_GAME;
    }
    public StartGameMsg(DataStreamReader reader)
    {
        code = OperationCode.START_GAME;
        deserialize(reader);
    }

    public override void serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)code);
    }
    public override void deserialize(DataStreamReader reader)
    {
    }

    public override void receivedOnClient()
    {
        NetUtility.C_START_GAME?.Invoke(this);
    }
    public override void receivedOnServer(NetworkConnection connection)
    {
        NetUtility.S_START_GAME?.Invoke(this, connection);
    }
}
