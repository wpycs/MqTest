using System.Threading.Tasks;
using System.Web.Http;
using MoneyTest.Config;
using MoneyTest.Message;

namespace MoneyTest.Web.Controllers
{
    public class SetMoneyController : ApiController
    {
        //到时候使用post
        [HttpGet]
        public async Task<bool> Get(string id)
        {
            //获得活动创建，修改消息，发送到mq
            await MqConfig.Bus.Publish(new ActivityChangedMessage { ActivityId = id });
            return true;
        }
    }
}
