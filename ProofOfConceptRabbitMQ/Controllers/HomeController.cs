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
				Content = GetContentAsync(new ContentRequest() { ContentKey = "pageTitle" }).Result,
				Customer = GetCustomerAsync(customerRequest).Result.Customers.FirstOrDefault()
			};

			return View(model);
		}

		public IActionResult About()
		{
			ViewData["Message"] = "Your application description page.";

			return View();
		}

		public IActionResult Contact()
		{
			ViewData["Message"] = "Your contact page.";

			return View();
		}

		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		private static async Task<string> GetContentAsync(ContentRequest request)
		{
			var contentProvider = new ContentProvider();

			var response = await contentProvider.GetContentAsync("");
			contentProvider.Close();

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
