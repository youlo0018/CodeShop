using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using WX.Common.Attributes;

namespace WX.Common.service
{
    public static class Register
    {
        public static IServiceCollection AddPropertyAttributesAndMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<PropertyAttributes>();
            return services;
        }
    }
}
