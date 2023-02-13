using Unity.Networking.Transport;
using UnityEngine;

public enum OperationCode{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    REMATCH = 5
}
public class Message
{
    public OperationCode code {set; get;}

    public virtual void serialize (ref DataStreamWriter writer){
        writer.WriteByte((byte)code);
    }
    public virtual void deserialize(DataStreamReader reader){

    }
     public virtual void receivedOnClient(){
        
    }
     public virtual void receivedOnServer(NetworkConnection connection){
        
    }
}
