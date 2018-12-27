using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitClasses.MessagingModels.Content
{
	[Serializable]
	public class ContentRequest
	{
		public string ContentKey { get; set; }
	}
}
