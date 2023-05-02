using System;
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
        isActive = true;
        registerToEvent();
    }

    public void shutdown()
    {
        //Clean up resources on shudown
        if (isActive)
        {
            unregisterToEvent();
            driver.Dispose();
            connection = default(NetworkConnection);
            isActive = false;
        }
    }

    //Automatically called by unity if the resource is destroyed e.g. when a client crashes
    private void OnDestroy()
    {
        shutdown();
    }

    /// Update is called every frame, if the MonoBehaviour is enabled.
    private void Update()
    {
        if (!isActive)
        {
            return;
        }
        //As the driver is part of the job sheduler we ensure that has completed
        driver.ScheduleUpdate().Complete();
        checkAlive();

        processEvents();
    }

    private void checkAlive()
    {
        //If a connection was not created but the client is active
        if (!connection.IsCreated && isActive)
        {
            //Invoke the connectionDropped action
            connectionDropped?.Invoke();
            shutdown();
        }
    }

    private void processEvents()
    {
        DataStreamReader inStream;

        NetworkEvent.Type messageType;
        //Pool a single connection
        while ((messageType = connection.PopEvent(driver, out inStream)) != NetworkEvent.Type.Empty)
        {
            //If we connect send a welcome meesage to the server
            if (messageType == NetworkEvent.Type.Connect)
            {
                sendToServer(new WelcomeMsg());
            }
            else if (messageType == NetworkEvent.Type.Data)
            {
                //Handle custom messages
                NetUtility.onData(inStream, default(NetworkConnection));
            }
            //If a client disconnects clean up and shutdown
            else if (messageType == NetworkEvent.Type.Disconnect)
            {
                connection = default(NetworkConnection);
                connectionDropped?.Invoke();
                shutdown();
            }
        }
    }
    //serialize a meessage and send it to the server
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
