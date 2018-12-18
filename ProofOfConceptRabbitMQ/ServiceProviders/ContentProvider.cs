using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Threading;
using System.Text;

namespace ProofOfConceptRabbitMQ.ServiceProviders
{
	public class ContentProvider
	{
		private const string QUEUE_NAME = "Content_Queue";

		private readonly IConnection connection;
		private readonly IModel channel;
		private readonly string replyQueueName = "amq.rabbitmq.reply-to";
		private readonly EventingBasicConsumer consumer;
		private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

		public ContentProvider()
		{
			var factory = new ConnectionFactory() { HostName = "localhost" };

			connection = factory.CreateConnection();
			channel = connection.CreateModel();
			consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<string> tcs))
					return;
				var body = ea.Body;
				var response = Encoding.UTF8.GetString(body);
				tcs.TrySetResult(response);
			};
		}

		public Task<string> GetContentAsync(string message, CancellationToken cancellationToken = default(CancellationToken))
		{
			IBasicProperties props = channel.CreateBasicProperties();
			var correlationId = Guid.NewGuid().ToString();
			props.CorrelationId = correlationId;
			props.ReplyTo = replyQueueName;
			var messageBytes = Encoding.UTF8.GetBytes(message);
			var tcs = new TaskCompletionSource<string>();
			callbackMapper.TryAdd(correlationId, tcs);

			channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, mandatory: false, basicProperties: props, body: messageBytes);

			channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);

			cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
			return tcs.Task;
		}

		public void Close()
		{
			connection.Close();
		}
	}
}
