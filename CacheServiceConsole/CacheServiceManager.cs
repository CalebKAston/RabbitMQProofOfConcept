using CacheServiceConsole.Providers;
using RabbitClasses.MessagingModels.Cache;
using RabbitClasses.MessagingModels.Content;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheServiceConsole
{
	public class CacheServiceManager
	{
		private const string QUEUE_NAME = "Cache_Queue";

		private readonly IConnection connection;
		private readonly IModel channel;
		private readonly EventingBasicConsumer consumer;

		public CacheServiceManager()
		{
			var factory = new ConnectionFactory() { HostName = "rabbitmq" };
			connection = factory.CreateConnection();
			channel = connection.CreateModel();

			channel.QueueDeclare(queue: QUEUE_NAME, durable: false, exclusive: false, autoDelete: false, arguments: null);
			channel.BasicQos(0, 1, false);

			consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				var request = ea.Body;
				var props = ea.BasicProperties;
				var replyProps = channel.CreateBasicProperties();
				replyProps.CorrelationId = props.CorrelationId;
				CacheRequest deserializedRequest;

				var formatter = new BinaryFormatter();
				using (var stream = new MemoryStream(request))
					deserializedRequest = (CacheRequest)formatter.Deserialize(stream);

				var contentRequest = new ContentRequest()
				{
					ContentKey = deserializedRequest.ContentKey
				};
				var contentResponse = GetContentAsync(contentRequest).Result;

				var cacheResponse = new CacheServiceResponse()
				{
					PageTitle = contentResponse.PageTitle
				};

				Byte[] serializedResponse;
				using (var stream = new MemoryStream())
				{
					formatter.Serialize(stream, cacheResponse);
					serializedResponse = stream.ToArray();
				}
				channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: serializedResponse);

				channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
			};

			channel.BasicConsume(queue: QUEUE_NAME, autoAck: false, consumer: consumer);
		}

		private async Task<ContentServiceResponse> GetContentAsync(ContentRequest request, CancellationToken cancellationToken = default(CancellationToken))
		{
			var contentProvider = new ContentProvider();

			var response = await contentProvider.GetContentAsync(request);

			return response;
		}

		public void Close()
		{
			connection.Close();
		}
	}
}
