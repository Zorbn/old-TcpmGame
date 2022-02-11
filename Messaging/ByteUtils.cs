using System.Text.Json;

namespace Messaging;

public static class ByteUtils
{
    public static byte[] ObjectToByteArray(Type dataType, Object obj)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj, dataType);
    }
    
    public static Object ByteArrayToObject(Type dataType, byte[] arrBytes)
    {
        return JsonSerializer.Deserialize(arrBytes, dataType)!;
    }
}