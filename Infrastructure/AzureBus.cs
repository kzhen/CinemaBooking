using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
	/// <summary>
	/// https://github.com/cmendible/EasyAzureServiceBus/blob/master/src/EasyAzureServiceBus/Bus/AzureBus.cs
	/// </summary>
	public class AzureBus : IBus
	{
		private readonly string connectionString;
		private NamespaceManager namespaceManager;
		private readonly ConcurrentDictionary<string, IEnumerable<Delegate>> subscriptionActions = new ConcurrentDictionary<string, IEnumerable<Delegate>>();
		private readonly ConcurrentDictionary<string, SubscriptionClient> subscriptionClients = new ConcurrentDictionary<string, SubscriptionClient>();
		private readonly ConcurrentDictionary<string, TopicClient> topicClients = new ConcurrentDictionary<string, TopicClient>();


		public AzureBus()
			: this(CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString"))
		{
		}

		public AzureBus(string connectionString)
		{
			this.connectionString = connectionString;
			this.namespaceManager = NamespaceManager.CreateFromConnectionString(this.connectionString);
		}


		public void Subscribe<T>(string subscriptionId, Action<T> onMessage) where T : class
		{
			string topicName = this.CreateTopicIfNotExists<T>();
			string realSubscriptionId = subscriptionId.ToLowerInvariant();

			if (!this.namespaceManager.SubscriptionExists(topicName, realSubscriptionId))
			{
				SubscriptionDescription dataCollectionTopic = this.namespaceManager.CreateSubscription(topicName, realSubscriptionId);
			}

			string descriptor = topicName + ":" + realSubscriptionId;
			subscriptionActions.AddOrUpdate(descriptor, new Delegate[] { onMessage }, (key, oldValue) => oldValue.Concat(new Delegate[] { onMessage }));

			Func<SubscriptionClient> clientSetup = () =>
			{
				SubscriptionClient client = SubscriptionClient.CreateFromConnectionString(
						this.connectionString,
						topicName,
						realSubscriptionId,
						ReceiveMode.PeekLock);

				OnMessageOptions options = new OnMessageOptions();
				options.AutoComplete = true;
				options.MaxConcurrentCalls = 1;

				client.OnMessage(envelope =>
				{
					if (!envelope.Properties.ContainsKey("Message.Type.AssemblyQualifiedName"))
					{
						envelope.DeadLetter();
						return;
					}

					string messageTypeAssemblyQualifiedName = envelope.Properties["Message.Type.AssemblyQualifiedName"].ToString();

					IEnumerable<Delegate> actions = subscriptionActions[descriptor]
							.Where(a => a.GetType().GetGenericArguments().First().AssemblyQualifiedName == messageTypeAssemblyQualifiedName);

					if (actions.Any())
					{
						Type messageType = actions.First().GetType().GetGenericArguments().First();

						foreach (Delegate action in actions)
						{
							object message = JsonConvert.DeserializeObject(envelope.GetBody<string>(), messageType);

							action.DynamicInvoke(message);
						}
					}
				},
						options);

				return client;
			};

			this.subscriptionClients.GetOrAdd(descriptor, clientSetup.Invoke());
		}

		public void Publish<T>(T message) where T : class
		{
			string topicName = this.CreateTopicIfNotExists<T>();

			if (!this.topicClients.ContainsKey(topicName))
			{
				this.topicClients.GetOrAdd(topicName, TopicClient.CreateFromConnectionString(this.connectionString, topicName));
			}

			TopicClient topicClient = this.topicClients[topicName];

			Type messageType = typeof(T);

			BrokeredMessage envelope = new BrokeredMessage(JsonConvert.SerializeObject(message));
			envelope.Properties["Message.Type.AssemblyQualifiedName"] = messageType.AssemblyQualifiedName;
			envelope.Properties["Message.Type.Assembly"] = messageType.Assembly.FullName;
			envelope.Properties["Message.Type.FullName"] = messageType.FullName;
			envelope.Properties["Message.Type.Namespace"] = messageType.Namespace;

			envelope.MessageId = message.GetHashCode().ToString();

			topicClient.Send(envelope);
		}

		private string CreateTopicIfNotExists<T>()
		{
			string topicName = typeof(T).FullName.ToLowerInvariant();

			if (!this.namespaceManager.TopicExists(topicName))
			{
				TopicDescription topicDescription = new TopicDescription(topicName)
				{
					RequiresDuplicateDetection =  false
				};

				this.namespaceManager.CreateTopic(topicDescription);
			}

			return topicName;
		}
	}
}
