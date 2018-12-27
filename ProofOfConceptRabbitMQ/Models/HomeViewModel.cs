using RabbitClasses.MessagingModels.Cache;
using RabbitClasses.MessagingModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProofOfConceptRabbitMQ.Models
{
	public class HomeViewModel
	{
		public CacheServiceResponse Content { get; set; }
		public CustomerInfo Customer { get; set; }
	}
}
