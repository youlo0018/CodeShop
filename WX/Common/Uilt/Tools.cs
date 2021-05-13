using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace WX.Common.Uilt
{
   public class Tools
    {
        #region //ConfigurationManager

        public static class ConfigurationManager
        {
            public static ConfigurationManager_Temp AppSettings
            {
                get
                {
                    return new ConfigurationManager_Temp();
                }
            }

            /// <summary>
            /// 配置文件版本
            /// </summary>
            public static string GetConfigVersion
            {
                get
                {
                    try
                    {
                        var configuration = new ConfigurationBuilder().AddJsonFile("Config.json");
                        IConfiguration config = configuration.Build();
                        var str = config["version"];
                        return str;
                    }
                    catch (Exception ex)
                    {
                        return "";
                    }
                }
            }

            public class ConfigurationManager_Temp
            {
                public string this[string name]
                {
                    get
                    {
                        try
                        {
                            //danny 增加了 SetBasePath(Directory.GetCurrentDirectory())，解决获找不到配置文件的问题

                            var configName = "Config.json";
                            var configVersion = ConfigurationManager.GetConfigVersion;
                            if (!string.IsNullOrEmpty(configVersion))
                                configName = $"Config/{configVersion}Config.json";

                            var configuration = new ConfigurationBuilder().AddJsonFile(configName);
                            IConfiguration config = configuration.Build();
                            return config[name];
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("目标: " + name); Console.WriteLine(ex); return null;
                        }
                    }
                }

                public IConfigurationSection GetSection(string name)
                {
                    try
                    {
                        //danny 增加了 SetBasePath(Directory.GetCurrentDirectory())，解决获找不到配置文件的问题

                        var configName = "Config.json";
                        var configVersion = ConfigurationManager.GetConfigVersion;
                        if (!string.IsNullOrEmpty(configVersion))
                            configName = $"Config/{configVersion}Config.json";

                        var configuration = new ConfigurationBuilder().AddJsonFile(configName);
                        IConfiguration config = configuration.Build();
                        return config.GetSection(name);
                    }
                    catch (Exception ex) { Console.WriteLine("目标: " + name); Console.WriteLine(ex); return null; }
                }

            }
        }

        #endregion

    }
    #region //HttpContext

    /// <summary>
    /// 上下文对象,程序里都可以使用
    /// </summary>
    public static class HttpContext
    {
        private static IHttpContextAccessor _accessor;

        public static Microsoft.AspNetCore.Http.HttpContext Current
        {
            get
            {
                if (_accessor == null)
                    return null;
                return _accessor.HttpContext;
            }
        }

        internal static void Configure(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }


    }

    /// <summary>
    /// IServiceCollection 扩展方法, 用于注册HttpContext
    /// </summary>
    public static class StaticHttpContextExtensions
    {
        public static void AddHttpContextAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        //注册使用HttpContext
        public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
        {
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            HttpContext.Configure(httpContextAccessor);
            return app;
        }
    }

    #endregion

}
