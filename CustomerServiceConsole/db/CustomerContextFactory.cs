using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerServiceConsole.db
{
	public class CustomerContextFactory : IDesignTimeDbContextFactory<CustomerContext>
	{
		private static string _connectionString;

		public CustomerContext CreateDbContext()
		{
			return CreateDbContext(null);
		}

		public CustomerContext CreateDbContext(string[] args)
		{
			if (string.IsNullOrEmpty(_connectionString))
			{
				LoadConnectionString();
			}

			var builder = new DbContextOptionsBuilder<CustomerContext>();
			builder.UseSqlServer(_connectionString);

			return new CustomerContext(builder.Options);
		}

		private static void LoadConnectionString()
		{
			var builder = new ConfigurationBuilder();
			builder.AddJsonFile("appsettings.json", optional: false);

			var configuration = builder.Build();

			_connectionString = configuration.GetConnectionString("DefaultConnection");
		}
	}
}
