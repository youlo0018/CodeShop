using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace WX.Comcon.Caching.Redis
{
   public class RedisCacheOptions:IOptions<RedisCacheOptions>
    {
        /// <summary>
        /// 用于设置连接redis的连接字符串
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// 用于设置连接redis
        /// 如果Configuration和ConfigurationOptions同时设置了，将会优先使用ConfigurationOptions
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }

        /// <summary>
        /// redis 实例的名字
        /// </summary>
        public string InstanceName { get; set; }

        RedisCacheOptions IOptions<RedisCacheOptions>.Value
        {
            get { return this; }
        }
    }
}
