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
    private NetworkConnection connection;

    private bool isActive = false;
    public Action connectionDropped;

    public void init(ushort port, string ip)
    {
        driver = NetworkDriver.Create();
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

        //keepAlive();
        driver.ScheduleUpdate().Complete();
        checkAlive();

        updateMessagePump();
    }

    private void checkAlive()
    {
        if (!connection.IsCreated && isActive)
        {
            Debug.Log("Lost connection to the server");
            connectionDropped?.Invoke();
            shutdown();
        }
    }

    private void updateMessagePump()
    {
        DataStreamReader inStream;

        NetworkEvent.Type message;
        while ((message = connection.PopEvent(driver, out inStream)) != NetworkEvent.Type.Empty)
        {
            if (message == NetworkEvent.Type.Connect)
            {
                //Handle the message
                sendToServer(new WelcomeMsg());
                Debug.Log("We are connected");
            }
            else if (message == NetworkEvent.Type.Data)
            {
                //Handle the message
                NetUtility.onData(inStream, default(NetworkConnection));
            }
            else if (message == NetworkEvent.Type.Disconnect)
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

    private void registerToEvent(){
        NetUtility.C_KEEP_ALIVE += onKeepAlive;
    }
      private void unregisterToEvent(){
        NetUtility.C_KEEP_ALIVE -= onKeepAlive;
    }
      private void onKeepAlive(Message msg){
        sendToServer(msg);
    }
}
