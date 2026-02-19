using PublisherService;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Please enter your name: ");
        var name = Console.ReadLine();

        var message = new MessagePublisher();
        message.SendMessage(new MessageFactory(), name ?? "");
        Console.ReadKey();
    }
}