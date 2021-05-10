using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WX.SCRM.Uilt;

namespace WX.SCRM.Controllers
{
 
    public class TestController : BaseController
    {
        /// <summary>
        /// 测试swagger
        /// </summary>
        /// <returns></returns>
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
