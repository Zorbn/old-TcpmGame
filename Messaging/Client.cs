using System.Net;
using System.Net.Sockets;

namespace Messaging;

public static class Client
{
    public delegate void OnConnect();
    public delegate void OnConnectFailed();
    public delegate void OnTick();

    private static TcpClient? Socket;
    private static MessageStream? MessageStream;
    private static int TickRate;
    
    private static bool IsInitialized;

    private static Dictionary<Message.MessageType, MessageStream.MessageHandler>? MessageHandlers;
    private static readonly Dictionary<Message.MessageType, MessageStream.MessageHandler> DefaultMessageHandlers = new()
    {
        { Message.MessageType.Initialize, HandleInitialize }
    };

    private static MessageStream.OnDisconnect? OnDisconnectCallback;
    private static OnConnect? OnConnectCallback;
    private static OnConnectFailed? OnConnectFailedCallback;
    private static OnTick? OnTickCallback;
    
    public static void StartClient(string ip, Dictionary<Message.MessageType, MessageStream.MessageHandler> messageHandlers, int tickRate, OnTick onTick, 
        MessageStream.OnDisconnect? onDisconnect, OnConnect onConnect, OnConnectFailed onConnectFailed)
    {
        TickRate = tickRate;
        
        OnDisconnectCallback = onDisconnect;
        OnConnectCallback = onConnect;
        OnConnectFailedCallback = onConnectFailed;
        OnTickCallback = onTick;
        
        MessageHandlers = messageHandlers;

        foreach (KeyValuePair<Message.MessageType, MessageStream.MessageHandler> defaultMessageHandler in DefaultMessageHandlers)
        {
            if (!MessageHandlers.ContainsKey(defaultMessageHandler.Key))
            {
                MessageHandlers.Add(defaultMessageHandler.Key, defaultMessageHandler.Value);
            }
        }
        
        Socket = new TcpClient
        {
            ReceiveBufferSize = MessageStream.DataBufferSize,
            SendBufferSize = MessageStream.DataBufferSize
        };
        Socket.BeginConnect(IPAddress.Parse(ip), 8052, ConnectCallback, Socket);

        Tick();
    }

    private static void Tick()
    {
        if (OnTickCallback == null) throw new Exception("Can't tick, no tick callback is defined!");
        
        Task.Delay(1000 / TickRate).ContinueWith(_ => Tick());

        if (!IsInitialized) return;
        OnTickCallback();
    }
    
    private static void ConnectCallback(IAsyncResult result)
    {
        if (OnConnectCallback == null) throw new Exception("Can't connect, no connect callback is defined!");
        if (OnConnectFailedCallback == null) throw new Exception("Can't connect, no connect failed callback is defined!");
        if (Socket == null) throw new Exception("Can't connect, socket is undefined!");
        
        try
        {
            Socket.EndConnect(result);
            OnConnectCallback();
        }
        catch
        {
            OnConnectFailedCallback();
        }

        if (!Socket.Connected) return;

        if (MessageHandlers == null)
            throw new Exception("Can't initialize message stream, no message handlers are defined!");
        if (OnDisconnectCallback == null)
            throw new Exception("Can't initialize message stream, no disconnect callback is defined!");
        
        MessageStream = new MessageStream(Socket, 0, MessageHandlers, OnDisconnectCallback);
        MessageStream.StartReading();
    }

    public static void SendMessage(Message.MessageType type, Data data)
    {
        if (!IsInitialized) return;
        if (MessageStream == null) throw new Exception("Can't send message, no message stream has been initialized!"); 
        
        MessageStream.SendMessage(type, data);
    }
    
    public static void HandleInitialize(Data data)
    {
        if (data is not InitializeData initData) return;
        if (MessageStream == null) throw new Exception("Can't initialize, no message stream has been initialized!");

        IsInitialized = true;
        MessageStream.Id = initData.Id;
            
        Console.WriteLine($"Initialized with id of: {initData.Id}.");
    }

    public static int GetId()
    {
        if (IsInitialized)
        {
            if (MessageStream == null) throw new Exception("Can't get id, the client is initialized but the message stream is undefined!");
            return MessageStream.Id;
        }
        
        return -1;
    }
}