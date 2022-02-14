using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Messaging;

public static class Server
{
    public delegate void OnTick();

    public delegate void OnClientConnect(int id);

    private static TcpListener? TcpListener;
    private static Dictionary<int, MessageStream>? Clients;
    private static Dictionary<Message.MessageType, MessageStream.MessageHandler>? MessageHandlers;
    
    public static int TickRate;

    private static MessageStream.OnDisconnect? OnDisconnectCallback;
    private static OnClientConnect? OnClientConnectCallback;
    private static OnTick? OnTickCallback;

    private static int LastId;
    
    public static void StartServer(string ip, Dictionary<Message.MessageType, MessageStream.MessageHandler>? messageHandlers, int tickRate, OnTick? onTick, MessageStream.OnDisconnect? onDisconnect, OnClientConnect? onClientConnect, bool integrated = false)
    {
        MessageHandlers = messageHandlers;
        Clients = new Dictionary<int, MessageStream>();
        TickRate = tickRate;

        OnDisconnectCallback = onDisconnect;
        OnTickCallback = onTick;
        OnClientConnectCallback = onClientConnect;
            
        TcpListener = new TcpListener(IPAddress.Parse(ip), 8052);
        TcpListener.Start();
        TcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

        Task.Run(() =>
        {
            Stopwatch tickWatch = new();
            tickWatch.Start();

            for (;;)
            {
                if (tickWatch.ElapsedMilliseconds > TickRate)
                {
                    tickWatch.Restart();
                    Tick();
                }
            }
            // ReSharper disable once FunctionNeverReturns
        });

        if (!integrated)
        {
            SpinWait.SpinUntil(() => false);
        }
    }

    private static void Tick()
    {
        OnTickCallback?.Invoke();
    }
    
    private static void TcpConnectCallback(IAsyncResult result)
    {
        if (TcpListener == null) throw new Exception("Can't finish connecting, missing TcpListener!");
        if (Clients == null) throw new Exception("Can't finish connecting, client list is null!");
        if (MessageHandlers == null) throw new Exception("Can't finish connecting, message handlers are null!");
        
        TcpClient client = TcpListener.EndAcceptTcpClient(result); // Finish accepting client
        TcpListener.BeginAcceptTcpClient(TcpConnectCallback, null); // Begin accepting new clients
        Console.WriteLine($"Connection from: {client.Client.RemoteEndPoint}...");

        int newClientId = LastId++;
        Clients.Add(newClientId, new MessageStream(client, newClientId, MessageHandlers, OnDisconnect));
        Clients[newClientId].StartReading();

        InitializeData initData = new(newClientId);
        
        Clients[newClientId].SendMessage(Message.MessageType.Initialize, initData);
        OnClientConnectCallback?.Invoke(newClientId);
    }

    private static void OnDisconnect(int id)
    {
        Clients?.Remove(id);
        OnDisconnectCallback?.Invoke(id);
    }

    public static void SendMessage(int id, Message.MessageType type, Data data)
    {
        if (Clients == null) return;
        
        if (!Clients.ContainsKey(id)) return;
        Clients[id].SendMessage(type, data);
    }

    public static void SendMessageToAll(Message.MessageType type, Data data)
    {
        if (Clients == null) return;
        
        foreach (KeyValuePair<int, MessageStream> client in Clients)
        {
            SendMessage(client.Key, type, data);
        }
    }

    public static void SendMessageToAllExcluding(int excludedId, Message.MessageType type, Data data)
    {
        if (Clients == null) return;
        
        foreach (KeyValuePair<int, MessageStream> client in Clients)
        {
            if (client.Key == excludedId) continue;
            SendMessage(client.Key, type, data);
        }
    }
}