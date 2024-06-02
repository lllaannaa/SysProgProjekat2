using System;
using System.Threading;
using System.Threading.Tasks;

namespace SysProgProjekat2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Putanja do root direktorijuma
            string rootDirectory = @"C:\NoviFolder";

            Server server = new Server("127.0.0.1", 5050, rootDirectory);
            await server.StartAsync();
        }
    }
}
