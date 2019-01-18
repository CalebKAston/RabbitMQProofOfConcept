using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Threading;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using RabbitClasses.MessagingModels.Content;

namespace CacheServiceConsole.Providers
{
	public class ContentProvider
	{
		private const string QUEUE_NAME = "Content_Queue";

		private readonly IConnection connection;
		private readonly IModel channel;
		private readonly string replyQueueName = "amq.rabbitmq.reply-to";
		private readonly EventingBasicConsumer consumer;
		private readonly ConcurrentDictionary<string, TaskCompletionSource<ContentServiceResponse>> callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<ContentServiceResponse>>();

		public ContentProvider()
		{
			var factory = new ConnectionFactory() { HostName = "rabbitmq" };

			connection = factory.CreateConnection();
			channel = connection.CreateModel();
			consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<ContentServiceResponse> tcs))
					return;
				var body = ea.Body;
				ContentServiceResponse response;
				var formatter = new BinaryFormatter();
				using (var stream = new MemoryStream(body))
					response = (ContentServiceResponse)formatter.Deserialize(stream);
				
				tcs.TrySetResult(response);
			};
		}

		public Task<ContentServiceResponse> GetContentAsync(ContentRequest request, CancellationToken cancellationToken = default(CancellationToken))
		{
			IBasicProperties props = channel.CreateBasicProperties();
			var correlationId = Guid.NewGuid().ToString();
			props.CorrelationId = correlationId;
			props.ReplyTo = replyQueueName;
			Byte[] messageBytes;
			var formatter = new BinaryFormatter();
			using (var stream = new MemoryStream())
			{
				formatter.Serialize(stream, request);
				messageBytes = stream.ToArray();

			}
			var tcs = new TaskCompletionSource<ContentServiceResponse>();
			callbackMapper.TryAdd(correlationId, tcs);

			channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);
			channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, mandatory: false, basicProperties: props, body: messageBytes);

			cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
			return tcs.Task;
		}

		public void Close()
		{
			connection.Close();
		}
	}
}

