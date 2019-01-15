using RabbitClasses.MessagingModels.Content;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ContentServiceConsole
{
	public class ContentServiceManager
	{
		private const string QUEUE_NAME = "Content_Queue";

		private readonly IConnection connection;
		private readonly IModel channel;
		private readonly EventingBasicConsumer consumer;

		public ContentServiceManager()
		{
			var factory = new ConnectionFactory() { HostName = "rabbitmq" };
			connection = factory.CreateConnection();
			channel = connection.CreateModel();

			channel.QueueDeclare(queue: QUEUE_NAME, durable: false, exclusive: false, autoDelete: false, arguments: null);
			channel.BasicQos(0, 1, false);

			consumer = new EventingBasicConsumer(channel);
			channel.BasicConsume(queue: QUEUE_NAME, autoAck: false, consumer: consumer);

			consumer.Received += (model, ea) =>
			{
				var response = new ContentServiceResponse();

				var body = ea.Body;
				var props = ea.BasicProperties;
				var replyProps = channel.CreateBasicProperties();
				replyProps.CorrelationId = props.CorrelationId;
				ContentRequest request;
				var formatter = new BinaryFormatter();
				
				using (var stream = new MemoryStream(body))
					request = (ContentRequest)formatter.Deserialize(stream);

				response.PageTitle = $"{request.ContentKey}: Example";

				Byte[] serializedResponse;
				using (var stream = new MemoryStream())
				{
					formatter.Serialize(stream, response);
					serializedResponse = stream.ToArray();
				}
				channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: serializedResponse);
				channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
			};
		}

		public void Close()
		{
			connection.Close();
		}
	}
}
