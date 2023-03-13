using Unity.Networking.Transport;
using UnityEngine;


//Base message class
public class Message
{
    public OperationCode code { set; get; }

    public virtual void serialize(ref DataStreamWriter writer)
    {
        //All messages will have the first byte set to the code which identifies which type of message it is
        writer.WriteByte((byte)code);
    }
    public virtual void deserialize(DataStreamReader reader)
    {

    }
    public virtual void receivedOnClient()
    {

    }
    public virtual void receivedOnServer(NetworkConnection connection)
    {

    }
}
