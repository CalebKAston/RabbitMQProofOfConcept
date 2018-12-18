using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerService.Models
{
	[Serializable]
	public class CustomerRequest
	{
		public CustomerInfo CustomerInfo { get; set; }
		public RestActions Action { get; set; }
	}

	public class CustomerInfo
	{
		public int? Id { get; set; }
		public string Name { get; set; }
		public int? Age { get; set; }
		public bool? LikesDisney { get; set; }
	}

	public enum RestActions
	{
		Get,
		Create,
		Delete,
		Update
	}
}
