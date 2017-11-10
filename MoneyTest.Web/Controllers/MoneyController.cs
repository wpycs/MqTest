using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using MoneyTest.Config;
using MoneyTest.Message;
using MoneyTest.Web.Models;

namespace MoneyTest.Web.Controllers
{
    public class MoneyController : ApiController
    {
        public async Task<string> Post([FromBody] MoneyReq req)
        {
            //判断是否已抢过，使用redis的set
            if (RedisConfig.Database.SetAdd("UserHis" + req.ActivityId, req.UserId))
            {
                //获取并删除第一个
                var money = RedisConfig.Database.ListLeftPop(req.ActivityId);
                await MqConfig.Bus.Publish(new MoneyAndUser
                {
                    ActivityId = req.ActivityId,
                    MoneyInfo = money.ToString(),
                    UserId = req.UserId
                });
                return money;
            }
            return null;
        }
    }
}
