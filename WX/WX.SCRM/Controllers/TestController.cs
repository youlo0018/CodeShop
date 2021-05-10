using Common.Uilt;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WX.SCRM.Uilt;
using static WX.AdvancedTools.config;

namespace WX.SCRM.Controllers
{
    /// <summary>
    /// 测试控制器
    /// </summary>
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
        [HiddenApi]
        [HttpPost]
        public string TestPostSwagger(MovieModel movie)
        {
            return "1";
        }
        /// <summary>
        /// 这是电影实体类
        /// </summary>
        public class MovieModel
        {
            /// <summary>
            /// Id
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// 影片名称
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 类型
            /// </summary>
            public string Type { get; set; }
        }
    }
}
