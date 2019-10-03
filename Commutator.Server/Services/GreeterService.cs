using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Commutator.Server
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override async Task SayHello(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, 
            ServerCallContext context)
        {
            _logger.LogInformation($"{context.RequestHeaders.Last().Value} is connected");
            await context.WriteResponseHeadersAsync(new Metadata
            {
                new Metadata.Entry("Info", "Server connection established")
            });
            using var timer = new Timer(60000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            while (await requestStream.MoveNext())
            {
                _logger.LogInformation(requestStream.Current.Name);
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = $"Received {requestStream.Current.Name}"
                });
            }

            void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                responseStream.WriteAsync(new HelloReply
                {
                    Message = "Beep"
                });
            }
        }
    }
}
