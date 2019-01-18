using Microsoft.EntityFrameworkCore;
using RabbitClasses.MessagingModels.Customer;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerServiceConsole.db
{
	public partial class CustomerContext : DbContext
	{
		public CustomerContext() { }
		public CustomerContext(DbContextOptions<CustomerContext> options) : base(options) { }
		public virtual DbSet<CustomerInfo> Customers { get; set; }
	}
}
