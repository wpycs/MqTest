using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MoneyTest.ReqTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var http = new HttpClient())
            {
                for (int i = 0; i < 10; i++)
                {
                    var res = Task.WhenAll(Enumerable.Range(0, 3000).Select(c => http.PostAsync("http://localhost:7639/api/Money",
                        new StringContent(JsonConvert.SerializeObject(new
                        {
                            UserId = Guid.NewGuid(),
                            ActivityId = "1111111"
                        }), Encoding.UTF8, "application/json")))).Result;
                }

            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
