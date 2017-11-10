using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MoneyTest.Config;
using MoneyTest.Message;
using Newtonsoft.Json;
using Rebus.Handlers;
using StackExchange.Redis;

namespace MoneyTest.Consumer
{
    public class Handler : IHandleMessages<ActivityChangedMessage>, IHandleMessages<MoneyAndUser>
    {
        public async Task Handle(ActivityChangedMessage message)
        {
            //从数据库查询此次活动的所有红包，注意活动开始后就不能改了
            //使用测试数据
            var random = new Random();
            await RedisConfig.Database.ListLeftPushAsync("1111111",
                 Enumerable.Range(0, 100000)
                     .Select(c => (RedisValue)JsonConvert.SerializeObject(new
                     {
                         Id = c,
                         Value = random.Next(0, 5000) / 100.0
                     })).ToArray());
            //用于用户验证重复、设置过期时间自动清除缓存、上面所有的红包同样可如此处理
            await RedisConfig.Database.SetAddAsync("UserHis1111111", "");
            await RedisConfig.Database.KeyExpireAsync("UserHis1111111", TimeSpan.FromDays(100));

        }

        public async Task Handle(MoneyAndUser message)
        {
            HisQueue.Queue.Enqueue(message);
        }
    }
}