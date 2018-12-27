using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitClasses.MessagingModels.Customer
{
	[Serializable]
	public class CustomerServiceResponse
	{
		public bool Success { get; set; }
		public CustomerInfo[] Customers { get; set; }
	}
}
