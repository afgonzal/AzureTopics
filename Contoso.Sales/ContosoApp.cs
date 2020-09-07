using System;
using Contoso.Sales.Business;

namespace Contoso.Sales
{
    public class ContosoApp
    {
        private readonly IQueueManager _queueManager;
        public ContosoApp(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }
        public void Run()
        {
            Console.WriteLine("Contoso Slices");
            //_queueManager.SendMessageAsync($"$10,000 order for bicycle parts from retailer Adventure Works.").GetAwaiter().GetResult();
            //Console.WriteLine("Message was sent successfully to sales queue.");

            //_queueManager.ReceiveMessageAsync().GetAwaiter().GetResult();
            //Console.WriteLine("No more messages to receive");

            _queueManager.SendMessageAsync("Total sales for Brazil in August: $13m.", "salesmessages").GetAwaiter().GetResult();
            Console.WriteLine("Message was sent successfully to topic sales queue.");

            _queueManager.ReceiveMessageAsync("salesmessages", "Subs1").GetAwaiter().GetResult();
            Console.WriteLine("No more messages to receive");

            _queueManager.ReceiveMessageAsync("salesmessages", "Subs2").GetAwaiter().GetResult();
            Console.WriteLine("No more messages to receive");

        }
    }
}
