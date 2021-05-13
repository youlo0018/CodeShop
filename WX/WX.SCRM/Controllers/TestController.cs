using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using WX.Comcon.Caching.Redis;
using WX.DB.Dapper;
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
        [HttpGet]
        public async Task<string> TestPostRedis(MovieModel movie)
        {
            await RedisCache.Instance.SetAsync("aaa", "bbb", new Comcon.Caching.Abstractions.DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(20) });
            return  RedisCache.Instance.Get("aaa").ToString();
        }
        [HttpGet]
        public async Task<string> Testmysql()
        {
           var aa= DB.Config.DominSys_Param.GetModel<sys_Param>("*", $"id={1}");
            return aa.CreateUser;
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

      
        public class sys_Param
        {
            public int id { get; set; }
            public string sys_key { get; set; }
            public string sys_value { get; set; }
            public DateTime CreateTime { get; set; }
            public string CreateUser { get; set; }
            public bool Deleted { get; set; }
            public DateTime UpdateTime { get; set; }
        }
    }
}
