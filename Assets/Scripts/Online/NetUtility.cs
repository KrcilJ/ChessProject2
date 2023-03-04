using System;
using Unity.Networking.Transport;
using UnityEngine;

public static class NetUtility

{
    //Handles the different kinds of messages
    public static void onData(DataStreamReader reader, NetworkConnection connection, Server server = null)
    {
        Message msg = null;
        //The first byte of the message if the code which determine which kind of message it is
        var operationCode = (OperationCode)reader.ReadByte();

        //Create a new message according to its type
        switch (operationCode)
        {
            case OperationCode.KEEP_ALIVE: msg = new KeepAliveMsg(reader); break;
            case OperationCode.WELCOME: msg = new WelcomeMsg(reader); break;
            case OperationCode.START_GAME: msg = new StartGameMsg(reader); break;
            case OperationCode.MAKE_MOVE: msg = new MakeMoveMsg(reader); break;
            case OperationCode.GAME_OVER: msg = new GameOverMsg(reader); break;
            default:
                Debug.Log("Message had an unrecognized operation code");
                break;
        }
        //Debug.Log(msg);
        if (server != null)
        {
            msg.receivedOnServer(connection);
        }
        else
        {
            msg.receivedOnClient();
        }

    }
    //Messages (will be used to identify messages)
    public static Action<Message> C_KEEP_ALIVE;

    public static Action<Message> C_WELCOME;

    public static Action<Message> C_START_GAME;

    public static Action<Message> C_MAKE_MOVE;

    public static Action<Message> C_GAME_OVER;

    public static Action<Message, NetworkConnection> S_KEEP_ALIVE;

    public static Action<Message, NetworkConnection> S_WELCOME;
    public static Action<Message, NetworkConnection> S_MAKE_MOVE;
    public static Action<Message, NetworkConnection> S_START_GAME;
    public static Action<Message, NetworkConnection> S_GAME_OVER;
}
