// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Transports.RabbitMq.Tests
{
	using System;
	using Magnum.TestFramework;
	using NUnit.Framework;

    public class TestMessage : CorrelatedBy<Guid>
    {
        public TestMessage(Guid CorrelationId)
        {
            this.CorrelationId = CorrelationId;
        }

        public string Message { get; set; }
        public Guid CorrelationId
        {
            get;
            private set;
        }
    }

	[Scenario, Explicit]
	public class When_connecting_to_a_rabbit_mq_server_using_ssl
	{
		IServiceBus _sslBus;
        IServiceBus _bus;
        TestMessage _sslMessage;
        TestMessage _nosslMessage;
		[When]
		public void Connecting_to_a_rabbit_mq_server_using_ssl()
		{
            Uri inputAddress = new Uri("rabbitmq://guest:guest:ssl@localhost:5671/ssl_queue");

            _sslBus = ServiceBusFactory.New(c =>
			{
				c.ReceiveFrom(inputAddress);
                c.SetPurgeOnStartup(true);
				c.UseRabbitMqRouting();
				c.UseRabbitMq();
			});
            _sslBus.SubscribeHandler<TestMessage>(msg =>
            {
                _sslMessage = msg;
            });
            _bus = ServiceBusFactory.New(c =>
            {
                c.ReceiveFrom(new Uri("rabbitmq://localhost/normal_queue"));
                c.SetPurgeOnStartup(true);
                c.UseRabbitMqRouting();
                c.UseRabbitMq();
            });
            _bus.SubscribeHandler<TestMessage>(msg =>
            {
                _nosslMessage = msg;
            });
            _sslBus.Publish(new TestMessage(Guid.NewGuid()) { Message = "Test Message" });
            System.Threading.Thread.Sleep(1000);
            Assert.NotNull(_sslMessage, "No message recieved");
            Assert.That(_sslMessage.Message == "Test Message");
            Assert.NotNull(_nosslMessage);
            Assert.That(_sslMessage.CorrelationId.Equals(_nosslMessage.CorrelationId));
        }

		[Finally]
		public void Finally()
		{
            _sslBus.Dispose();
            _bus.Dispose();
		}

		[Then, Explicit]
		public void Should_connect_to_the_queue()
		{
			
		}

	}
}