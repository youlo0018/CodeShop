using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WX.Comcon.Caching;
using WX.Comcon.Caching.Redis;
using WX.DB.Dapper;
using WX.DB.Entity;
using WX.SCRM.Uilt;
using static WX.AdvancedTools.config;

namespace WX.SCRM.Controllers
{
    /// <summary>
    /// 测试控制器
    /// </summary>
    public class TestController : BaseController
    {
       
        public ConfigureCache _ConfigureCache;
        public TestController(ConfigureCache ConfigureCache)
        {
            _ConfigureCache = ConfigureCache;
          
        }
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
            return RedisCache.Instance.Get("aaa").ToString();
        }
        /// <summary>
        /// 测试mysql
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> Testmysql()
        {
            return "aa";
        }
        /// <summary>
        /// 远程触发jiekins
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> TestJenkins(jenkins jenkins)
        {
            if (jenkins.repository.name!="wx_scrm")
            {
                return false;
            }
            
            return true;
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
      
        public class Push_data
        {
            /// <summary>
            /// 
            /// </summary>
            public string digest { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string pushed_at { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string tag { get; set; }
        }

        public class Repository
        {
            /// <summary>
            /// 
            /// </summary>
            public string date_created { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string @namespace { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string region { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string repo_authentication_type { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string repo_full_name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string repo_origin_type { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string repo_type { get; set; }
        }

        public class jenkins
        {
            /// <summary>
            /// 
            /// </summary>
            public Push_data push_data { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Repository repository { get; set; }
        }

    }
}
