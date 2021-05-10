using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WX.SCRM.Controllers
{
    [Produces("application/json")] //返回数据的格式 直接约定为Json
    [Route("api/[controller]/[action]")]  //路由
    public class TestController : Controller
    {
        [HttpGet]
        public string TestSwagger()
        {
            return "12";

        }
        [HttpPost]
        public string TestPostSwagger()
        {
            return "1";
        }
    }
}
