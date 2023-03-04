using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    //Creates a static instance of the server
    public static Server Instance { set; get; }

    //Set the instance
    private void Awake()
    {
        Instance = this;
    }

    public NetworkDriver driver;

    //List of all network connections 
    private NativeList<NetworkConnection> connections;

    private bool isActive = false;
    //Rate of every 20s, we will send a message 
    private const float keepAliveRate = 20.0f;
    private float lastKeepAlive;

    public Action connectionDropped;

    public void init(ushort port)
    {
        //Create
        driver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
        endPoint.Port = port;
        //Try to bind to a port
        if (driver.Bind(endPoint) != 0)
        {
            Debug.Log("Unable to bind on port" + endPoint.Port);
        }
        else
        {
            //Listen to incoming connection if we are able to bind to the port
            driver.Listen();
            Debug.Log("Listening on port" + endPoint.Port);
        }
        //Set the maximum amount of connections (players) in our case 2
        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        isActive = true;
    }

    public void shutdown()
    {
        if (isActive)
        {
          //Clean up when we shutdown the server
            driver.Dispose();
            connections.Dispose();
            isActive = false;
        }
    }
    //Callback which is automatically called by unity
    private void OnDestroy()
    {
        shutdown();
    }
    // Update is called every frame, if the MonoBehaviour is enabled.
    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        keepAlive();
        //As the driver is part of the job sheduler we ensure that has completed
        driver.ScheduleUpdate().Complete();

        cleanUpConnections();
        acceptNewConnections();
        updateMessagePump();
    }

    //Remove all connections
    private void cleanUpConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                i--;
            }
        }
    }
    //Add a new connection
    private void acceptNewConnections()
    {
        //If there is a connection which is waiting to be accepted we accept it
        NetworkConnection connection;
        while ((connection = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(connection);
        }
    }

    private void updateMessagePump()
    {
        DataStreamReader inStream;
        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type message;
            while ((message = driver.PopEventForConnection(connections[i], out inStream)) != NetworkEvent.Type.Empty)
            {
                //Check if the message has data
                if (message == NetworkEvent.Type.Data)
                {
                    //Handle the message
                    NetUtility.onData(inStream, connections[i], this);
                }
                else if (message == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected");
                    connections[i] = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    //If somebody disconnects we shutdown the server as it is a 2 player game
                    shutdown();
                }
            }
        }
    }

    //Server logic
    public void sendToClient(NetworkConnection connection, Message msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.serialize(ref writer);
        driver.EndSend(writer);
    }
    public void broadcast(Message msg)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].IsCreated)
            {
                sendToClient(connections[i], msg);
            }
        }
    }
    //Messages back on forth to keep the server and client running
    public void keepAlive()
    {
        if (Time.time - lastKeepAlive > keepAliveRate)
        {
            lastKeepAlive = Time.time;
            broadcast(new KeepAliveMsg());
        }
    }
}
