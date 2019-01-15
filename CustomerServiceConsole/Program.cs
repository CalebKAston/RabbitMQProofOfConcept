using System;
using System.Threading.Tasks;

namespace CustomerServiceConsole
{
	static class Program
	{
		static void Main(string[] args)
		{
			RunService().Wait();
		}

		static Task RunService()
		{
			var service = new CustomerServiceManager();

			var task = new TaskCompletionSource<int>();
			return task.Task;
		}
	}
}
