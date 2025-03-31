using MyConcatenateLib;

namespace Task1;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string username = args[0];
            username = ConCatLib.ConcatenateLogic(username);
            Console.WriteLine($"{username}");
        }
        else
        {
            Console.WriteLine("Please provide a username as a command-line argument.");
        }
    }
}
