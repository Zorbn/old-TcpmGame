using System.Net.Sockets;

namespace Messaging;

public class MessageStream
{
    public const int DataBufferSize = 1024;
    public int Id;
        
    private readonly byte[] incomingDataBuffer = new byte[DataBufferSize];
    private readonly List<byte> storedData = new();
        
    private NetworkStream stream;

    private bool hasNextMessageLength;
    private int nextMessageLength;

    public delegate void OnDisconnect(int id);

    private OnDisconnect onDisconnect;
    public delegate void MessageHandler(Data data);
    private Dictionary<Message.MessageType, MessageHandler> messageHandlers;

    public MessageStream(TcpClient socket, int id, Dictionary<Message.MessageType, MessageHandler> messageHandlers, OnDisconnect onDisconnect)
    {
        Id = id;
        this.messageHandlers = messageHandlers;
        this.onDisconnect = onDisconnect;
        
        stream = socket.GetStream();
    }

    public void StartReading()
    {
        stream.BeginRead(incomingDataBuffer, 0, DataBufferSize, ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int incomingDataLength = stream.EndRead(result);
            if (incomingDataLength != 0) Read(incomingDataLength);
            stream.BeginRead(incomingDataBuffer, 0, DataBufferSize, ReceiveCallback, null);
        }
        catch
        {
            stream.Dispose();
            Console.WriteLine($"Stream closed with id of: {Id}!");
            onDisconnect(Id);
        }
    }

    public void SendMessage(Message.MessageType type, Data data)
    {
        try
        {
            // Get a stream object for writing. 			
            if (!stream.CanWrite) return;

            Message message = new(type, data);

            byte[] clientMessageAsByteArray = message.ToByteArray();

            // Write byte array to socketConnection stream.                 
            stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
        }
        catch (SocketException socketException)
        {
            Console.WriteLine("Socket exception: " + socketException);
        }
    }
        
    public void Read(int incomingDataLength)
    {
        // Read incoming stream into byte array. 						
        byte[] trimmedData = new byte[incomingDataLength]; // Incoming data without any empty space					
        Array.Copy(incomingDataBuffer, 0, trimmedData, 0, incomingDataLength);
        storedData.AddRange(trimmedData);

        while (storedData.Count > 0)
        {
            if (hasNextMessageLength)
            {
                if (storedData.Count >= nextMessageLength) // The whole message has been received
                {
                    HandleData(storedData, nextMessageLength);

                    storedData.RemoveRange(0, nextMessageLength);
                    hasNextMessageLength = false;
                    nextMessageLength = 0;
                }
                else
                {
                    break; // Wait for more data
                }
            }
            else
            {
                if (storedData.Count >=
                    sizeof(int)) // There is enough incoming data to read the next message's length
                {
                    nextMessageLength = BitConverter.ToInt32(storedData.ToArray(), 0);
                    hasNextMessageLength = true;
                }
                else
                {
                    break; // Wait for more data
                }
            }
        }
    }
        
    public void HandleData(List<byte> dataBuffer, int length)
    {
        Message.MessageType messageType = (Message.MessageType)BitConverter.ToInt32(dataBuffer.ToArray(), sizeof(int));

        try
        {
            int offset = 2 * sizeof(int); // Length of message length and type
            Data data = (Data) ByteUtils.ByteArrayToObject( Message.ToDataType(messageType), dataBuffer.GetRange(offset, length - offset).ToArray());

            messageHandlers[messageType](data);
        }
        catch
        {
            Console.WriteLine($"Failed to receive message of type: {messageType}!");
        }
    }
}