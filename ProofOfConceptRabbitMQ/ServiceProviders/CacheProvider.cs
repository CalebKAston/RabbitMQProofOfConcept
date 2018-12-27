using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Threading;
using System.Text;
using ProofOfConceptRabbitMQ.Models;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using RabbitClasses.MessagingModels.Content;
using RabbitClasses.MessagingModels.Cache;

namespace ProofOfConceptRabbitMQ.ServiceProviders
{
	public class CacheProvider
	{
		private const string QUEUE_NAME = "Cache_Queue";

		private readonly IConnection connection;
		private readonly IModel channel;
		private readonly string replyQueueName = "amq.rabbitmq.reply-to";
		private readonly EventingBasicConsumer consumer;
		private readonly ConcurrentDictionary<string, TaskCompletionSource<CacheServiceResponse>> callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<CacheServiceResponse>>();

		public CacheProvider()
		{
			var factory = new ConnectionFactory() { HostName = "localhost" };

			connection = factory.CreateConnection();
			channel = connection.CreateModel();
			consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<CacheServiceResponse> tcs))
					return;
				var body = ea.Body;
				var formatter = new BinaryFormatter();
				CacheServiceResponse deserializedResponse;
				using (var stream = new MemoryStream(body))
					deserializedResponse = (CacheServiceResponse)formatter.Deserialize(stream);

				tcs.TrySetResult(deserializedResponse);
			};
		}

		public Task<CacheServiceResponse> GetContentAsync(CacheRequest request, CancellationToken cancellationToken = default(CancellationToken))
		{
			IBasicProperties props = channel.CreateBasicProperties();
			var correlationId = Guid.NewGuid().ToString();
			props.CorrelationId = correlationId;
			props.ReplyTo = replyQueueName;

			Byte[] message;
			var formatter = new BinaryFormatter();
			using (var stream = new MemoryStream())
			{
				formatter.Serialize(stream, request);
				message = stream.ToArray();
			}
			var tcs = new TaskCompletionSource<CacheServiceResponse>();
			callbackMapper.TryAdd(correlationId, tcs);

			channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);
			channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, mandatory: false, basicProperties: props, body: message);


			cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
			return tcs.Task;
		}

		public void Close()
		{
			connection.Close();
		}
	}
}
