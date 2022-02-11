namespace Messaging;

public static class ExampleNotification
{
    public static void HandleNotification(Data data)
    {
        if (data is not ExampleNotificationData notificationData) return;
            
        Console.WriteLine($"Message received: {notificationData.Text}");
    }
}