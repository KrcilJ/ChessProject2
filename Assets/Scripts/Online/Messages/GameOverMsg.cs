
using Unity.Networking.Transport;
using UnityEngine;

public class GameOverMsg : Message
{
 
    public int team;
    public int player {set; get;}
    public GameOverMsg(){
        code = OperationCode.GAME_OVER;
    }
     public GameOverMsg(DataStreamReader reader){
        code = OperationCode.GAME_OVER;
        deserialize(reader);
    }

    public override void serialize(ref DataStreamWriter writer)
    {
       writer.WriteByte((byte)code);
       writer.WriteInt(team);
    }
    public override void deserialize(DataStreamReader reader)
    {
        team = reader.ReadInt();
    }

    public override void receivedOnClient()
    {
        NetUtility.C_GAME_OVER?.Invoke(this);
    }
    public override void receivedOnServer(NetworkConnection connection)
    {
       NetUtility.S_GAME_OVER?.Invoke(this, connection);
    }
}
