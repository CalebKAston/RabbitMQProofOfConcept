using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerService.Models
{
	public class CustomerServiceResponse
	{
		public bool Success { get; set; }
		public CustomerInfo[] Customers { get; set; }
	}
}
