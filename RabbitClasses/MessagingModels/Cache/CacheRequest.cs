using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitClasses.MessagingModels.Cache
{
	[Serializable]
	public class CacheRequest
	{
		public string ContentKey { get; set; }
	}
}
