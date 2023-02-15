using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    public static Server Instance { set; get; }

    private void Awake()
    {
        Instance = this;
    }

    public NetworkDriver driver;
    private NativeList<NetworkConnection> connections;

    private bool isActive = false;
    private const float keepAliveRate = 20.0f;
    private float lastKeepAlive;

    public Action connectionDropped;

    public void init(ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
        endPoint.Port = port;

        if (driver.Bind(endPoint) != 0)
        {
            Debug.Log("Unable to bind on port" + endPoint.Port);
        }
        else
        {
            driver.Listen();
            Debug.Log("Listening on port" + endPoint.Port);
        }
        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        isActive = true;
    }

    public void shutdown(){
      if (isActive)
      {
        driver.Dispose();
        connections.Dispose();
        isActive = false;    
      }
    }
    private void OnDestroy() {
      shutdown();
    }
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
      if(!isActive) {
        return;
      }

      keepAlive();
      driver.ScheduleUpdate().Complete();

      cleanUpConnections();
      acceptNewConnections();
      updateMessagePump();


    }

    private void cleanUpConnections(){
      for (int i = 0; i < connections.Length; i++)
      {
        if (!connections[i].IsCreated)
        {
          connections.RemoveAtSwapBack(i);
          i--;
        }
      }
    }
    private void acceptNewConnections(){
      NetworkConnection connection;
      while ((connection = driver.Accept()) != default(NetworkConnection))
      {
        connections.Add(connection);
      }
    }

    private void updateMessagePump(){
      DataStreamReader inStream;
      for (int i = 0; i < connections.Length; i++)
      {
        NetworkEvent.Type message;
        while((message = driver.PopEventForConnection(connections[i], out inStream)) != NetworkEvent.Type.Empty) {
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
            shutdown();
          }
        }
      }
    }

    //Server logic
  public void sendToClient(NetworkConnection connection, Message msg){
    DataStreamWriter writer;
    driver.BeginSend(connection, out writer);
    msg.serialize(ref writer);
    driver.EndSend(writer);
  }
    public void broadcast (Message msg) {
       for (int i = 0; i < connections.Length; i++)
      {
        if(connections[i].IsCreated) {
         // Debug.Log($"Sending {msg.Code} to : {connections[i].InternalId}");
          sendToClient(connections[i], msg);
        }
      }
    }

    public void keepAlive(){
      if(Time.time - lastKeepAlive > keepAliveRate) {
        lastKeepAlive = Time.time;
        broadcast(new KeepAliveMsg());
      }
    }
}
