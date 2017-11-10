using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using MoneyTest.Config;
using MoneyTest.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Serialization.Json;

namespace MoneyTest.Consumer
{
    public class ProcessorWinServiceBootstrap
    {
        private BuiltinHandlerActivator _activator;
        public void Start()
        {
            _activator = new BuiltinHandlerActivator();
            _activator.Register(() => new Handler());
            var bus = Configure.With(_activator)
                .Logging(c => c.None()) //日志输出先关闭
                .Transport(c => c.UseRabbitMq("amqp://guest:guest@10.0.75.1/dev", "MoneyTest"))
                .Serialization(c => c.UseNewtonsoftJson(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }))
                .Start();

            bus.Subscribe<ActivityChangedMessage>();
            bus.Subscribe<MoneyAndUser>();

            //开启新线程，消费内存队列中的数据，批量插入数据库
            Task.Run(() =>
            {
                HisQueue.SaveToDb();
            });
        }

        public void Stop()
        {
            _activator.Bus.Dispose();
            _activator.Dispose();
            RedisConfig.Stop();
        }
    }



    public static class HisQueue
    {
        public static ConcurrentQueue<MoneyAndUser> Queue = new ConcurrentQueue<MoneyAndUser>();

        public static void SaveToDb()
        {
            while (true)
            {
                var list = new List<MoneyAndUser>(1000);
                MoneyAndUser result;
                for (int i = 0; i < 1000; i++)
                {
                    if (Queue.TryDequeue(out result))
                    {
                        list.Add(result);
                    }
                    else
                    {
                        break;
                    }
                }
                if (list.Count > 0)
                {
                    using (SqlConnection conn = new SqlConnection(@"Server=(localdb)\ProjectsV13;Database=MoneyTest;Trusted_Connection=True;AttachDbFileName=E:\DB\MoneyTest.mdf"))
                    {
                        conn.Open();
                        using (SqlBulkCopy sqlbulkcopy = new SqlBulkCopy(conn))
                        {
                            var dt = new DataTable("MoneyHis");
                            dt.Columns.AddRange(new DataColumn[]{
                                new DataColumn("Id",typeof(int)),
                                new DataColumn("userId",typeof(string)),
                                new DataColumn("moneyId",typeof(string)),
                                new DataColumn("money",typeof(decimal)),
                                new DataColumn("CreateTime",typeof(DateTime))});
                            sqlbulkcopy.DestinationTableName = "MoneyHis";

                            foreach (var moneyAndUser in list)
                            {
                                var moneyInfo = JObject.Parse(moneyAndUser.MoneyInfo);
                                dt.Rows.Add(0, moneyAndUser.UserId, moneyInfo["Id"].ToString(),
                                    moneyInfo["Value"].Value<decimal>(), DateTime.Now);
                            }
                            sqlbulkcopy.WriteToServer(dt);
                        }
                        conn.Close();
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}