using System;
using Unity.Networking.Transport;
using UnityEngine;

//Codes for different messages
public enum OperationCode
{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    GAME_OVER = 5
}
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
            case OperationCode.KEEP_ALIVE:
                msg = new KeepAliveMsg(reader);
                break;
            case OperationCode.WELCOME:
                msg = new WelcomeMsg(reader);
                break;
            case OperationCode.START_GAME:
                msg = new StartGameMsg(reader);
                break;
            case OperationCode.MAKE_MOVE:
                msg = new MakeMoveMsg(reader);
                break;
            case OperationCode.GAME_OVER:
                msg = new GameOverMsg(reader);
                break;
            default:
                Debug.Log("unrecognized message");
                break;
        }
        //Server is nulled by default so only not null when assigned as the optional parameter
        if (server != null)
        {
            msg.receivedOnServer(connection);
        }
        else
        {
            msg.receivedOnClient();
        }

    }
    //Events based on where the message was received C for client events S for server events
    public static Action<Message> C_KEEP_ALIVE;
    public static Action<Message> C_WELCOME;
    public static Action<Message> C_START_GAME;
    public static Action<Message> C_MAKE_MOVE;
    public static Action<Message, NetworkConnection> S_MAKE_MOVE;
    public static Action<Message> C_GAME_OVER;

    public static Action<Message, NetworkConnection> S_KEEP_ALIVE;
    public static Action<Message, NetworkConnection> S_WELCOME;
    public static Action<Message, NetworkConnection> S_START_GAME;
    public static Action<Message, NetworkConnection> S_GAME_OVER;
}
