
using Unity.Networking.Transport;
using UnityEngine;

public class MakeMoveMsg : Message
{
    public int originalX;
    public int originalY;
    public int goalX;
    public int goalY;
    public int team;
    public int player {set; get;}
    public MakeMoveMsg(){
        code = OperationCode.MAKE_MOVE;
    }
     public MakeMoveMsg(DataStreamReader reader){
        code = OperationCode.MAKE_MOVE;
        deserialize(reader);
    }

    public override void serialize(ref DataStreamWriter writer)
    {
       writer.WriteByte((byte)code);
       writer.WriteInt(originalX);
       writer.WriteInt(originalY);
       writer.WriteInt(goalX);
       writer.WriteInt(goalY);
       writer.WriteInt(team);
    }
    public override void deserialize(DataStreamReader reader)
    {
        originalX = reader.ReadInt();
        originalY = reader.ReadInt();
        goalX = reader.ReadInt();
        goalY = reader.ReadInt();
        team = reader.ReadInt();
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
