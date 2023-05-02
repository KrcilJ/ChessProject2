
using Unity.Networking.Transport;
using UnityEngine;

public class MakeMoveMsg : Message
{
    public int originalX;
    public int originalY;
    public int goalX;
    public int goalY;
    public int player; // using an integer to denote the player instead of a string as it is more efficient to sned over the network and easier to serailize and deserialize
    public MakeMoveMsg()
    { // Creating a message
        code = OperationCode.MAKE_MOVE;
    }
    public MakeMoveMsg(DataStreamReader reader)
    { //Receiving a message
        code = OperationCode.MAKE_MOVE;
        deserialize(reader);
    }

    //Send the original and goal coordinates and the player 
    public override void serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)code);
        writer.WriteInt(originalX);
        writer.WriteInt(originalY);
        writer.WriteInt(goalX);
        writer.WriteInt(goalY);
        writer.WriteInt(player);
    }
    public override void deserialize(DataStreamReader reader)
    {
        originalX = reader.ReadInt();
        originalY = reader.ReadInt();
        goalX = reader.ReadInt();
        goalY = reader.ReadInt();
        player = reader.ReadInt();
    }

    public override void receivedOnClient()
    {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }
    public override void receivedOnServer(NetworkConnection connection)
    {
        NetUtility.S_MAKE_MOVE?.Invoke(this, connection);
    }
}
