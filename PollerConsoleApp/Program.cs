using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using PollerConsoleApp.Data;

namespace PollerConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pollingPeriod = TimeSpan.FromSeconds(1);

            var greetings = Task.Run(async() =>
            {
                Console.WriteLine("Welcome to useless polling application!");

                // display latest previous batch of polled units here (loaded from database)
                DBService dBService = new DBService();
                dBService.readLastRecord();

                Console.WriteLine("Select polling interval measured in seconds and press Enter");

                pollingPeriod = await SelectPollingPeriod(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));

                Console.WriteLine("Press ESC to exit or any other key to log it\n");
            });
            await greetings;

            var cancelSource = new CancellationTokenSource();
            var transmitter = new Subject<Data.PollerUnit>();           

            var poller = new PolledConsumerWrapper(transmitter, pollingPeriod, onNewBatchObserved, cancelSource);
            
            var polleach = Task.Run(() =>
            {
                while (!cancelSource.IsCancellationRequested)
                {                    
                    var key = Console.ReadKey();          

                    if (key.Key == ConsoleKey.Escape)
                    {
                        cancelSource.Cancel();
                        break;
                    }
                    
                    transmitter.OnNext(new Data.PollerUnit()
                    {
                        Timestamp = DateTimeOffset.Now,
                        Content = key.KeyChar.ToString()
                    });
                }
            });

            await polleach;

            cancelSource.Cancel();
        }

        private static async Task<TimeSpan> SelectPollingPeriod(TimeSpan defaultPeriod, TimeSpan awaitUserInput)
        {
            var pollingPeriod = defaultPeriod;

            var intervalSelectionCanclellation = new CancellationTokenSource();

            var waitIntervalSelection = Task.Run(() =>
            {
                var parsedSucessfully = false;
                while (!parsedSucessfully & !intervalSelectionCanclellation.IsCancellationRequested)
                {
                    var intervalAsString = Console.ReadLine();
                    parsedSucessfully = Int32.TryParse(intervalAsString, out var parsedInterval);
                    if (parsedSucessfully)
                        pollingPeriod = TimeSpan.FromSeconds(parsedInterval);
                }
                intervalSelectionCanclellation.Cancel();
            });
          
            var delayForUserEnter = Task.Delay(awaitUserInput, intervalSelectionCanclellation.Token);          

            await Task.WhenAny(waitIntervalSelection, delayForUserEnter);
            intervalSelectionCanclellation.Cancel();
                       
            Console.WriteLine($"Polling period is {pollingPeriod.Seconds} seconds");

            if (delayForUserEnter.IsCompleted)
                Console.WriteLine("Press Enter to continue");
                return pollingPeriod;
        }

        private static void onNewBatchObserved(List<Data.IPollerUnit> polled)
        {
            string keyValue = ($"{polled.Aggregate(new StringBuilder(), (sb, item) => sb.Append($"{item.Timestamp.ToString("HH:mm:ss:ffff")} {item.Content} \n"))}");
            DBService dbService = new DBService();
            dbService.insertNewValue(keyValue);
            
        }

    }
}
