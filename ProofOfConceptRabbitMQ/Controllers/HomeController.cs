using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProofOfConceptRabbitMQ.Models;
using ProofOfConceptRabbitMQ.ServiceProviders;
using RabbitClasses.MessagingModels;
using RabbitClasses.MessagingModels.Cache;
using RabbitClasses.MessagingModels.Content;
using RabbitClasses.MessagingModels.Customer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProofOfConceptRabbitMQ.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			var customerRequest = new CustomerRequest()
			{
				CustomerInfo = new CustomerInfo()
				{
					Name = "Bob"
				}
			};
			var model = new HomeViewModel
			{
				Content = GetContentAsync(new CacheRequest() { ContentKey = "pageTitle" }).Result,
				Customer = GetCustomerAsync(customerRequest).Result.Customers.FirstOrDefault()
			};

			return View(model);
		}

		public IActionResult About()
		{
			ViewData["Message"] = "Your application description page.";

			var newCustomer = new CustomerRequest()
			{
				CustomerInfo = new CustomerInfo()
				{
					Id = 10,
					Name = "Sally Frederickson",
					Age = 45,
					LikesDisney = true
				}
			};
			var response = CreateCustomerAsync(newCustomer).Result;

			var updateCustomer = new CustomerRequest()
			{
				CustomerInfo = new CustomerInfo()
				{
					Id = 10,
					Name = "Sally Benion Frederickson",
					Age = 50,
					LikesDisney = false
				}
			};
			var updateResponse = UpdateCustomerAsync(updateCustomer).Result;

			return View();
		}

		public IActionResult Contact()
		{
			var customerToDelete = new CustomerRequest()
			{
				CustomerInfo = new CustomerInfo()
				{
					Id = 10
				}
			};
			var response = DeleteCustomerAsync(customerToDelete);

			ViewData["Message"] = "Your contact page.";

			return View();
		}

		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		private static async Task<CacheServiceResponse> GetContentAsync(CacheRequest request)
		{
			var cacheProvider = new CacheProvider();

			var response = await cacheProvider.GetContentAsync(request);
			cacheProvider.Close();

			return response;
		}

		private static async Task<CustomerServiceResponse> GetCustomerAsync(CustomerRequest request)
		{
			var customerProvider = new CustomerProvider();

			var response = await customerProvider.SendRequestAsync(request, RestActions.Get);
			customerProvider.Close();

			return response;
		}

		private static async Task<CustomerServiceResponse> CreateCustomerAsync(CustomerRequest request)
		{
			var customerProvider = new CustomerProvider();

			var response = await customerProvider.SendRequestAsync(request, RestActions.Create);
			customerProvider.Close();

			return response;
		}

		private static async Task<CustomerServiceResponse> DeleteCustomerAsync(CustomerRequest request)
		{
			var customerProvider = new CustomerProvider();

			var response = await customerProvider.SendRequestAsync(request, RestActions.Delete);
			customerProvider.Close();

			return response;
		}

		private static async Task<CustomerServiceResponse> UpdateCustomerAsync(CustomerRequest request)
		{
			var customerProvider = new CustomerProvider();

			var response = await customerProvider.SendRequestAsync(request, RestActions.Update);
			customerProvider.Close();

			return response;
		}
	}
}