using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitClasses.MessagingModels.Customer
{
	[Serializable]
	public class CustomerInfo
	{
		public int? Id { get; set; }
		public string Name { get; set; }
		public int? Age { get; set; }
		public bool? LikesDisney { get; set; }
	}
}
