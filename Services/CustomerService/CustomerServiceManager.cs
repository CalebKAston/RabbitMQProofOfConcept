using RabbitClasses.MessagingModels;
using RabbitClasses.MessagingModels.Customer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CustomerService
{
	public class CustomerServiceManager
	{
		private const string QUEUE_NAME = "Customer_Queue";

		private readonly IConnection connection;
		private readonly IModel channel;
		private readonly EventingBasicConsumer consumer;

		public CustomerServiceManager()
		{
			var factory = new ConnectionFactory() { HostName = "localhost" };
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
				CustomerRequest deserializedRequest;

				var formatter = new BinaryFormatter();
				using (var stream = new MemoryStream(request))
					deserializedRequest = (CustomerRequest)formatter.Deserialize(stream);

				CustomerServiceResponse response;

				switch (deserializedRequest.Action)
				{
					case RestActions.Get:
					default:
						response = GetCustomers(deserializedRequest);
						break;
					case RestActions.Create:
						response = CreateCustomer(deserializedRequest);
						break;
					case RestActions.Delete:
						response = DeleteCustomer(deserializedRequest);
						break;
					case RestActions.Update:
						response = UpdateCustomer(deserializedRequest);
						break;
				}

				Byte[] serializedResponse;
				using (var stream = new MemoryStream())
				{
					formatter.Serialize(stream, response);
					serializedResponse = stream.ToArray();
				}
				channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: serializedResponse);

				channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
			};

			channel.BasicConsume(queue: QUEUE_NAME, autoAck: false, consumer: consumer);
		}

		private CustomerServiceResponse GetCustomers(CustomerRequest request)
		{
			var response = new CustomerServiceResponse();

			IEnumerable<CustomerInfo> customers = Customers.ToArray();

			if (request.CustomerInfo.Id.HasValue)
				customers = customers.Where(customer => customer.Id == request.CustomerInfo.Id);
			if (!string.IsNullOrEmpty(request.CustomerInfo.Name))
				customers = customers.Where(customer => customer.Name.Contains(request.CustomerInfo.Name));
			if (request.CustomerInfo.Age.HasValue)
				customers = customers.Where(customer => customer.Age == request.CustomerInfo.Age);
			if (request.CustomerInfo.LikesDisney.HasValue)
				customers = customers.Where(customer => customer.LikesDisney == request.CustomerInfo.LikesDisney);

			response.Customers = customers.ToArray();
			response.Success = true;

			return response;
		}

		private CustomerServiceResponse CreateCustomer(CustomerRequest request)
		{
			var response = new CustomerServiceResponse();

			Customers.Add(request.CustomerInfo);

			response.Success = true;
			response.Customers = new CustomerInfo[] { request.CustomerInfo };
			return response;
		}

		private CustomerServiceResponse DeleteCustomer(CustomerRequest request)
		{
			var response = new CustomerServiceResponse();

			var customer = Customers.SingleOrDefault(user => user.Id == request.CustomerInfo.Id);

			if (customer != null)
			{
				Customers.Remove(customer);

				response.Success = true;
				response.Customers = new CustomerInfo[] { customer };
			}
			else
			{
				response.Success = false;
				response.Customers = new CustomerInfo[] { request.CustomerInfo };
			}

			return response;
		}

		private CustomerServiceResponse UpdateCustomer(CustomerRequest request)
		{
			var response = new CustomerServiceResponse();

			var customerToUpdate = Customers.SingleOrDefault(customer => customer.Id == request.CustomerInfo.Id);

			if (customerToUpdate == null)
			{
				response.Success = false;
				response.Customers = new CustomerInfo[] { request.CustomerInfo };
				return response;
			}

			if (!string.IsNullOrEmpty(request.CustomerInfo.Name))
				customerToUpdate.Name = request.CustomerInfo.Name;
			if (request.CustomerInfo.Age.HasValue)
				customerToUpdate.Age = request.CustomerInfo.Age;
			if (request.CustomerInfo.LikesDisney.HasValue)
				customerToUpdate.LikesDisney = request.CustomerInfo.LikesDisney;

			response.Success = true;
			response.Customers = new CustomerInfo[] { customerToUpdate };
			return response;
		}

		private List<CustomerInfo> Customers = new List<CustomerInfo>()
		{
			new CustomerInfo() { Id = 0, Name = "Bob Marley", Age = 25, LikesDisney = true },
			new CustomerInfo() { Id = 1, Name = "Bob the Builder", Age = 26, LikesDisney = false },
			new CustomerInfo() { Id = 2, Name = "Jenn Marley", Age = 19, LikesDisney = true },
			new CustomerInfo() { Id = 3, Name = "Bugs Bunny", Age = 22, LikesDisney = true },
			new CustomerInfo() { Id = 4, Name = "Harold Lee", Age = 30, LikesDisney = false },
			new CustomerInfo() { Id = 5, Name = "Stan Lee", Age = 25, LikesDisney = true },
			new CustomerInfo() { Id = 6, Name = "Jim Bob", Age = 9, LikesDisney = false },
			new CustomerInfo() { Id = 7, Name = "Frank Sinatra", Age = 45, LikesDisney = true },
		};

		public void Close()
		{
			connection.Close();
		}
	}
}
