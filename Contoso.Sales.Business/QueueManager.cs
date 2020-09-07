using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Contoso.Sales.Business
{

    public interface IQueueManager
    {
        Task SendMessageAsync(string message, string subject = null);
        Task ReceiveMessageAsync(string topic = null, string subscription = null);

        Task CloseAsync();
    }
    public class QueueManager : IQueueManager
    {
        private readonly QueueClient _queueClient;
        private readonly IConfiguration _config;
        private ISubscriptionClient _subscriptionClient;

        public QueueManager(IConfiguration config)
        {
            _config = config;
            _queueClient = new QueueClient(config["serviceBus"], config["salesQueue"]);
        }

        public async Task SendMessageAsync(string message, string topic = null)
        {
            Console.WriteLine($"Sending message: {message}");
            if (string.IsNullOrEmpty(topic))
            {
                await _queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(message)));
                await _queueClient.CloseAsync();
            }
            else
            {
                var topicClient = new TopicClient(_config["serviceBus"], topic);
                await topicClient.SendAsync(new Message(Encoding.UTF8.GetBytes(message)));
                await topicClient.CloseAsync();
            }
        }

        public async Task ReceiveMessageAsync(string topic = null, string subscription = null)
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };
            if (string.IsNullOrEmpty(topic))
            {
                // Register the message handler function
                _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
                Console.Read();
                await _queueClient.CloseAsync();
            }
            else
            {
                _subscriptionClient = new SubscriptionClient(_config["serviceBus"], topic, subscription);
                _subscriptionClient.RegisterMessageHandler(ProcessTopicMessagesAsync, messageHandlerOptions);
                Console.ReadLine();
                await _subscriptionClient.CloseAsync();
            }
        }

     

        public Task CloseAsync()
        {
            return _queueClient.CloseAsync();
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private async Task ProcessTopicMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
