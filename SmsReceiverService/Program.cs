using Domain.Messages;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsReceiverService
{
	class Program
	{
		private static IBus bus;
		private static string phoneNumber;

		static void Main(string[] args)
		{
      Console.WriteLine("SmsReceiverService");
      Console.WriteLine("------------------");
      Console.WriteLine();
      Console.WriteLine("To set the current phone number type !PH [phonenumber]");
      Console.WriteLine("Then any text you type will be sent as an SMS from this number.");
      Console.WriteLine();
      
      bus = new AzureBus();
			bus.Subscribe<SendSms>("SmsReceiverService", sendSmsHandler);

			string input = string.Empty;

			do
			{
				input = Console.ReadLine();

				if (input.StartsWith("!"))
				{
					ParseCommand(input);
				}
				else
				{
					SendSms(input);
				}

			} while (input != "!QUIT");

		}

		private static void sendSmsHandler(Domain.Messages.SendSms obj)
		{
			Console.WriteLine("Response: To {0} Body {1}", obj.PhoneNumber, obj.Body);
		}

		private static void SendSms(string input)
		{
			if (string.IsNullOrWhiteSpace(phoneNumber))
			{
				Console.WriteLine("Please set your phonenumber using !PH");
				return;
			}
			bus.Publish(new SmsReceived() { Body = input, PhoneNumber = phoneNumber });
		}

		private static void ParseCommand(string input)
		{
			if (input.StartsWith("!PH", StringComparison.InvariantCultureIgnoreCase))
			{
				phoneNumber = input.Substring(4);
				Console.WriteLine("New phonenumber: {0}", phoneNumber);
			}
		}
	}
}
