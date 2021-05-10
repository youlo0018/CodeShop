using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WX.SCRM.Controllers
{
    public class TestController : Controller
    {
        [HttpGet]
        public string TestSwagger()
        {
            return "1";
        }
        [HttpPost]
        public string TestPostSwagger()
        {
            return "1";
        }
    }
}
