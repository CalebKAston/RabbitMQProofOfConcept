using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CacheService
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());

			var factory = new ConnectionFactory() { HostName = "localhost" };
			using (var connection = factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				var cacheQueueName = "Cache_Queue";

				channel.QueueDeclare(queue: cacheQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
				channel.BasicQos(0, 1, false);

				var consumer = new EventingBasicConsumer(channel);
				consumer.Received += (model, ea) =>
				{
					var request = ea.Body;
					dynamic deserializedRequest;

					var formatter = new BinaryFormatter();
					using (var stream = new MemoryStream(request))
						deserializedRequest = formatter.Deserialize(stream);

					channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
				};

				channel.BasicConsume(queue: cacheQueueName, autoAck: false, consumer: consumer);
			}
		}
	}
}
