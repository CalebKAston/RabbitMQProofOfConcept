using System;
using System.Threading.Tasks;

namespace ContentServiceConsole
{
	static class Program
	{
		static void Main(string[] args)
		{
			RunService().Wait();
		}

		static Task RunService()
		{
			var service = new ContentServiceManager();

			var task = new TaskCompletionSource<int>();
			return task.Task;
		}
	}
}
