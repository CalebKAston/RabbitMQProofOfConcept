using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ContentService
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
				var contentQueueName = "Content_Queue";

				channel.QueueDeclare(queue: contentQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
				channel.BasicQos(0, 1, false);

				var consumer = new EventingBasicConsumer(channel);
				channel.BasicConsume(queue: contentQueueName, autoAck: false, consumer: consumer);

				consumer.Received += (model, ea) =>
				{
					string response = null;

					var body = ea.Body;
					var props = ea.BasicProperties;
					var replyProps = channel.CreateBasicProperties();
					replyProps.CorrelationId = props.CorrelationId;

					try
					{
						var message = Encoding.UTF8.GetString(body);
						object request = null; //parse from body

						response = $"Generate this based off of Request: {request}";
					}
					catch (Exception e)
					{
						response = $"error: {e}";
					}
					finally
					{
						var responseBytes = Encoding.UTF8.GetBytes(response);
						channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
						channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
					}
				};
			}
		}
	}
}
