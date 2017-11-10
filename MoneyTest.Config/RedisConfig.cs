using System;
using StackExchange.Redis;

namespace MoneyTest.Config
{
    public static class RedisConfig
    {
        static IDatabase Start()
        {
            return ConnectionMultiplexer.Connect("10.0.75.1").GetDatabase();
        }

        public static IDatabase Database { get { return LazyDatabase.Value; } }
        private static readonly Lazy<IDatabase> LazyDatabase = new Lazy<IDatabase>(Start);
        public static void Stop()
        {
            Database.Multiplexer.Close();
            Database.Multiplexer.Dispose();
        }
    }
}