using Commutator.Server;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Commutator.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;

            // The port number(5001) must match the port of the gRPC server.
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);
            using (var call = client.SayHello(headers: new Metadata { new Metadata.Entry("Client", "Commutator.Client") }))
            {
                var responseTask = Task.Run(async () =>
                {
                    var headers = await call.ResponseHeadersAsync;
                    Console.WriteLine(headers.Last().Value);

                    await foreach (var message in call.ResponseStream.ReadAllAsync())
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(message.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                });

                while (true)
                {
                    var result = Console.ReadLine();

                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                        break;
                    
                    await call.RequestStream.WriteAsync(new HelloRequest()
                    {
                        Name = result
                    });
                }

                await call.RequestStream.CompleteAsync();
                await responseTask;
            }
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
