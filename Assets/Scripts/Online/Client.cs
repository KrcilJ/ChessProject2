using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance { set; get; }

    private void Awake()
    {
        Instance = this;
    }

    public NetworkDriver driver;
    //Connection to the server
    private NetworkConnection connection;

    private bool isActive = false;
    public Action connectionDropped;

    public void init(ushort port, string ip)
    {
        driver = NetworkDriver.Create();
        //Connect to a specific ip and port
        NetworkEndPoint endPoint = NetworkEndPoint.Parse(ip, port);
        connection = driver.Connect(endPoint);
        Debug.Log("Attempting to connect" + endPoint.Address);
        isActive = true;

        registerToEvent();
    }

    public void shutdown()
    {
        if (isActive)
        {
            unregisterToEvent();
            driver.Dispose();
            connection = default(NetworkConnection);
            isActive = false;
        }
    }

    private void OnDestroy()
    {
        shutdown();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        if (!isActive)
        {
            return;
        }
        //As the driver is part of the job sheduler we ensure that has completed
        driver.ScheduleUpdate().Complete();
        checkAlive();

        updateMessagePump();
    }

    private void checkAlive()
    {
        //If a connection was not created but the client is active
        if (!connection.IsCreated && isActive)
        {
            Debug.Log("Lost connection to the server");
            //Invoke the connectionDropped action
            connectionDropped?.Invoke();
            shutdown();
        }
    }

    private void updateMessagePump()
    {
        DataStreamReader inStream;

        NetworkEvent.Type messageType;
        //Pool a single connection
        while ((messageType = connection.PopEvent(driver, out inStream)) != NetworkEvent.Type.Empty)
        {
            if (messageType == NetworkEvent.Type.Connect)
            {
                sendToServer(new WelcomeMsg());
                Debug.Log("We are connected");
            }
            else if (messageType == NetworkEvent.Type.Data)
            {
                NetUtility.onData(inStream, default(NetworkConnection));
            }
            else if (messageType == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from the server");
                connection = default(NetworkConnection);
                connectionDropped?.Invoke();
                shutdown();
            }
        }
    }

    public void sendToServer(Message msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.serialize(ref writer);
        driver.EndSend(writer);
    }

    private void registerToEvent()
    {
        //When we receive a keep alive message, call the onKeepAlive
        NetUtility.C_KEEP_ALIVE += onKeepAlive;
    }
    private void unregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= onKeepAlive;
    }
    //Send the message back to the server
    private void onKeepAlive(Message msg)
    {
        sendToServer(msg);
    }
}
