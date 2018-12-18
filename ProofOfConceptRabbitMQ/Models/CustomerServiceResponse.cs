using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProofOfConceptRabbitMQ.Models
{
	public class CustomerServiceResponse
	{
		public bool Success { get; set; }
		public CustomerInfo[] Customers { get; set; }
	}
}
