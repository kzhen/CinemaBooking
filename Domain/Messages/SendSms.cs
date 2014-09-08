using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Messages
{
	public class SendSms
	{
		public string PhoneNumber { get; set; }
		public string Body { get; set; }
	}
}
