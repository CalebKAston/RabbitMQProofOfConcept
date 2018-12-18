using ProofOfConceptRabbitMQ.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProofOfConceptRabbitMQ.ServiceProviders
{
	public class CustomerProvider
	{
		private const string QUEUE_NAME = "Customer_Queue";

		private readonly IConnection connection;
		private readonly IModel channel;
		private readonly string replyQueueName = "amq.rabbitmq.reply-to";
		private readonly EventingBasicConsumer consumer;
		private readonly ConcurrentDictionary<string, TaskCompletionSource<CustomerServiceResponse>> callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<CustomerServiceResponse>>();

		public CustomerProvider()
		{
			var factory = new ConnectionFactory() { HostName = "localhost" };
			connection = factory.CreateConnection();
			channel = connection.CreateModel();
			consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<CustomerServiceResponse> tcs))
					return;
				var body = ea.Body;
				CustomerServiceResponse response;
				var formatter = new BinaryFormatter();
				using (var stream = new MemoryStream(body))
				{
					response = (CustomerServiceResponse)formatter.Deserialize(stream);
				}
				tcs.TrySetResult(response);
			};
		}
		
		//public Task<CustomerServiceResponse> GetCustomerAsync(CustomerRequest request, CancellationToken cancellationToken = default(CancellationToken))
		//{
		//	var props = channel.CreateBasicProperties();
		//	var correlationId = Guid.NewGuid().ToString();
		//	props.CorrelationId = correlationId;
		//	props.ReplyTo = replyQueueName;
		//	var tcs = new TaskCompletionSource<CustomerServiceResponse>();
		//	var formatter = new BinaryFormatter();
		//	Byte[] messageBytes;
		//	using (var stream = new MemoryStream())
		//	{
		//		formatter.Serialize(stream, request);
		//		messageBytes = stream.ToArray();
		//	}
		//	callbackMapper.TryAdd(correlationId, tcs);

		//	channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, mandatory: false, basicProperties: props, body: messageBytes);

		//	channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);

		//	cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
		//	return tcs.Task;
		//}
		
		//public Task<CustomerServiceResponse> CreateCustomerAsync(CustomerRequest request, CancellationToken cancellationToken = default(CancellationToken))
		//{
		//	var props = channel.CreateBasicProperties();
		//	var correlationId = Guid.NewGuid().ToString();
		//	props.CorrelationId = correlationId;
		//	props.ReplyTo = replyQueueName;
		//	var tcs = new TaskCompletionSource<CustomerServiceResponse>();
		//	var formatter = new BinaryFormatter();
		//	Byte[] messageBytes;
		//	using (var stream = new MemoryStream())
		//	{
		//		formatter.Serialize(stream, request);
		//		messageBytes = stream.ToArray();
		//	}
		//	callbackMapper.TryAdd(correlationId, tcs);

		//	channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, mandatory: false, basicProperties: props, body: messageBytes);

		//	channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);

		//	cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
		//	return tcs.Task;
		//}
		
		//public Task<CustomerServiceResponse> DeleteCustomerAsync(CustomerRequest request, CancellationToken cancellationToken = default(CancellationToken))
		//{
		//	var props = channel.CreateBasicProperties();
		//	var correlationId = Guid.NewGuid().ToString();
		//	props.CorrelationId = correlationId;
		//	props.ReplyTo = replyQueueName;
		//	var tcs = new TaskCompletionSource<CustomerServiceResponse>();
		//	var formatter = new BinaryFormatter();
		//	Byte[] messageBytes;
		//	using (var stream = new MemoryStream())
		//	{
		//		formatter.Serialize(stream, request);
		//		messageBytes = stream.ToArray();
		//	}
		//	callbackMapper.TryAdd(correlationId, tcs);

		//	channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, mandatory: false, basicProperties: props, body: messageBytes);

		//	channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);

		//	cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
		//	return tcs.Task;
		//}
		
		//public Task<CustomerServiceResponse> UpdateCustomerAsync(CustomerRequest request, CancellationToken cancellationToken = default(CancellationToken))
		//{
		//	var props = channel.CreateBasicProperties();
		//	var correlationId = Guid.NewGuid().ToString();
		//	props.CorrelationId = correlationId;
		//	props.ReplyTo = replyQueueName;
		//	var tcs = new TaskCompletionSource<CustomerServiceResponse>();
		//	var formatter = new BinaryFormatter();
		//	Byte[] messageBytes;
		//	using (var stream = new MemoryStream())
		//	{
		//		formatter.Serialize(stream, request);
		//		messageBytes = stream.ToArray();
		//	}
		//	callbackMapper.TryAdd(correlationId, tcs);

		//	channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, mandatory: false, basicProperties: props, body: messageBytes);

		//	channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);

		//	cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
		//	return tcs.Task;
		//}

		public Task<CustomerServiceResponse> SendRequestAsync(CustomerRequest request, RestActions restAction, CancellationToken cancellationToken = default(CancellationToken))
		{
			var props = channel.CreateBasicProperties();
			var correlationId = Guid.NewGuid().ToString();
			props.CorrelationId = correlationId;
			props.ReplyTo = replyQueueName;
			request.Action = restAction; 
			var tcs = new TaskCompletionSource<CustomerServiceResponse>();
			var formatter = new BinaryFormatter();
			Byte[] messageBytes;
			using (var stream = new MemoryStream())
			{
				formatter.Serialize(stream, request);
				messageBytes = stream.ToArray();
			}
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
