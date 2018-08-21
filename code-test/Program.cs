using System;
using System.Threading;
using System.Threading.Tasks;
using RingbaLibs;
using RingbaLibs.Models;
using Microsoft.Extensions.DependencyInjection;

namespace code_test
{
    class Program
    {
        // AutoResetEvent to signal when to exit the application.
        private static readonly AutoResetEvent _waitHandle = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            var services = new ServiceCollection()
            .AddTransient<ImplementMeService>()
            .BuildServiceProvider();
            
            using (var service = services.GetService<ImplementMeService>())
            {
                // Fire and forget
                Task.Run(() =>
                {
                    service.DoWork();
                });

                // Handle Control+C or Control+Break
                Console.CancelKeyPress += (o, e) =>
                {
                    service.Stop();
                    // Allow the main thread to continue and exit...
                    _waitHandle.Set();
                };

                // Wait
                _waitHandle.WaitOne();
            }

        }


      


    }
}
