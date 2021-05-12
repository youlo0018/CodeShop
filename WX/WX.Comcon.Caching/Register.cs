using Microsoft.Extensions.DependencyInjection;
using System;
using WX.Comcon.Caching.Redis;

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
}
