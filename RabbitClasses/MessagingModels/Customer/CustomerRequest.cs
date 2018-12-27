using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitClasses.MessagingModels.Customer
{
	[Serializable]
	public class CustomerRequest
	{
		public CustomerInfo CustomerInfo { get; set; }
		public RestActions Action { get; set; }
	}
}
