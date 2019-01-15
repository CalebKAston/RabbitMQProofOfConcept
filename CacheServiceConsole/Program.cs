using System;
using System.Threading.Tasks;

namespace CacheServiceConsole
{
	static class Program
	{
		static void Main(string[] args)
		{
			RunService().Wait();
		}

		static Task RunService()
		{
			var service = new CacheServiceManager();
			
			var task = new TaskCompletionSource<int>();
			return task.Task;
		}
	}
}
