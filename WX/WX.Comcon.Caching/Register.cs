using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using WX.Comcon.Caching.Redis;
using WX.DB.Dapper;

namespace WX.Comcon.Caching
{
    public static class Register
    {
		public static IServiceCollection AddDistributedRedisCache(this IServiceCollection services, RedisCacheOptions setupOption)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}
			if (setupOption == null)
			{
				throw new ArgumentNullException(nameof(setupOption));
			}

			RedisCache.Instance.Init(setupOption);
			return services;
		}
		
	}
    /// <summary>
    /// 将本地配置写入缓存
    /// </summary>
    public class ConfigureCache
    {
        public IMemoryCache _memoryCache;
        public ConfigureCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public  void Configureinjected()
        {
            var sys_param = WX.DB.Config.Sys_Param.GetList<DB.Entity.Sys_Param>("*");
            var domin_sys_param = WX.DB.Config.DominSys_Param.GetList<DB.Entity.Sys_Param>("*");
            sys_param.AddRange(domin_sys_param);
            _memoryCache.Set(DataCache.Config.Dominnmae + ".CacheParam", sys_param, TimeSpan.FromDays(1));
        }
    }
}
