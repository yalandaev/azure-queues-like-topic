using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace MessageBus.Transport.AzureStorageQueue
{
    public class AzureMessageQueue<TEvent>: IMessageQueue<TEvent>
    {
        private readonly CloudQueue _queue;

        public AzureMessageQueue(string queueName)
        {
            var connectionString = "UseDevelopmentStorage=true";
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            var client = cloudStorageAccount.CreateCloudQueueClient();
            _queue = client.GetQueueReference(queueName);
            _queue.CreateIfNotExists();
        }

        public void Clear()
        {
            _queue.Clear();
        }

        public void Enqueue(TEvent message)
        {
            EnqueueAsync(message).Wait();
        }

        private Task EnqueueAsync(TEvent message)
        {
            var serialized = JsonConvert.SerializeObject(message);
            return _queue.AddMessageAsync(new CloudQueueMessage(serialized));
        }

        public void Delete(string id)
        {
            DeleteAsync(id).Wait();
        }

        public KeyValuePair<string, TEvent>? Get()
        {
            return GetAsync().GetAwaiter().GetResult();
        }

        private async Task<KeyValuePair<string, TEvent>?> GetAsync()
        {
            var message = await _queue.GetMessageAsync().ConfigureAwait(false);
            if (message == null)
                return null;

            var deserialized = JsonConvert.DeserializeObject<TEvent>(message.AsString);
            return new KeyValuePair<string, TEvent>(GetMessageID(message), deserialized);
        }

        private Task DeleteAsync(string id)
        {
            var messageID = GetMessageID(id);
            return _queue.DeleteMessageAsync(messageID.ID, messageID.PopReceipt);
        }

        private static string GetMessageID(CloudQueueMessage message)
        {
            var messageID = new MessageID
            {
                ID = message.Id,
                PopReceipt = message.PopReceipt
            };
            var serializedID = JsonConvert.SerializeObject(messageID);
            return serializedID;
        }

        private static MessageID GetMessageID(string messageID)
        {
            return JsonConvert.DeserializeObject<MessageID>(messageID);
        }

        private class MessageID
        {
            public string ID { get; set; }

            public string PopReceipt { get; set; }
        }
    }

    
}