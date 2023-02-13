using System;
using Unity.Networking.Transport;
using UnityEngine;

public static class NetUtility

{
    public static void onData(DataStreamReader reader, NetworkConnection connection, Server server = null){
        Message msg = null;
        var operationCode = (OperationCode)reader.ReadByte();

        switch(operationCode){
            case OperationCode.KEEP_ALIVE: new KeepAliveMsg(reader); break;
            // case OperationCode.WELCOME: new welcomeMsg(reader); break;
            // case OperationCode.START_GAME: new startGameMsg(reader); break;
            // case OperationCode.MAKE_MOVE: new makeMoveMsg(reader); break;
            // case OperationCode.REMATCH: new rematchMsg(reader); break;
            default:
                Debug.Log("Message had an unrecognized operation code");
                break;
        }

        if(server != null){
            msg.receivedOnServer(connection);
        }
        else{
            msg.receivedOnClient();
        }

    }
//Messages
public static Action<Message> C_KEEP_ALIVE;

public static Action<Message> C_WELCOME;

public static Action<Message> C_START_GAME;

public static Action<Message> C_MAKE_MOVE;

public static Action<Message> C_REMATCH;

public static Action<Message, NetworkConnection> S_KEEP_ALIVE;

public static Action<Message, NetworkConnection> S_WELCOME;
public static Action<Message, NetworkConnection> S_MAKE_MOVE;
public static Action<Message, NetworkConnection> S_STAR_GAME;
public static Action<Message, NetworkConnection> S_REMATCH;
}
