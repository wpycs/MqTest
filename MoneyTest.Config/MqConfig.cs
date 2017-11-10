using System;
using Newtonsoft.Json;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Serialization.Json;

namespace MoneyTest.Config
{
    public static class MqConfig
    {
        static IBus Start()
        {
            return Configure.With(Activator)
                .Logging(c => c.ColoredConsole())
                .Transport(c => c.UseRabbitMqAsOneWayClient("amqp://guest:guest@10.0.75.1/dev"))
                .Serialization(c =>
                    c.UseNewtonsoftJson(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }))
                .Start();
        }

        public static IBus Bus { get { return Lazybus.Value; } }

        private static readonly Lazy<IBus> Lazybus = new Lazy<IBus>(Start);

        public static void Stop()
        {
            Bus.Dispose();
            if (Activator != null)
            {
                Activator.Dispose();
            }
        }

        public static BuiltinHandlerActivator Activator
        {
            get { return new Lazy<BuiltinHandlerActivator>(() => new BuiltinHandlerActivator()).Value; }
        }
    }
}