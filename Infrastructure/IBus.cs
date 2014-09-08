using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
	/// <summary>
	/// https://github.com/cmendible/EasyAzureServiceBus/blob/master/src/EasyAzureServiceBus/Bus/IBus.cs
	/// </summary>
	public interface IBus
	{
		void Subscribe<T>(string subscriptionId, Action<T> onMessage) where T : class;
		void Publish<T>(T message) where T : class;
	}
}
