using WX.Common.service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WX.Comcon.Caching;
using static WX.AdvancedTools.config;
using WX.SCRM.Uilt;

namespace WX.SCRM
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            #region swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("V1", new OpenApiInfo
                {
                    Version = "V1",   //版本 
                    Title = $"XUnit.Core 接口文档-NetCore3.1",  //标题
                    Description = $"XUnit.Core Http API v1",    //描述
                    Contact = new OpenApiContact { Name = "Youlo0018", Email = "", Url = new Uri("http://baidu.com") },
                    License = new OpenApiLicense { Name = "Youlo0018许可证", Url = new Uri("http://baidu.com") }
                });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                                                                                        //var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "WX.SCRM.xml");//这个就是刚刚配置的xml文件名
                c.IncludeXmlComments(xmlPath, true);//默认的第二个参数是false,对方法的注释
                c.DocumentFilter<HiddenApiFilter>();
                // c.IncludeXmlComments(xmlPath,true); //这个是controller的注释
            });
            #endregion
            #region Redis
            services.AddDistributedRedisCache(new Comcon.Caching.Redis.RedisCacheOptions
            {
                Configuration = DataCache.Config.RedisUrl,
                InstanceName = ""
            });
            #endregion
            #region 注册缓存和实体表单验证
            services.AddPropertyAttributesAndMemoryCache();
            #endregion
            services.AddDominServices();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                #region 开发环境显示swagger
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/V1/swagger.json", $"XUnit.Core V1");
                    c.RoutePrefix = string.Empty;     //如果是为空 访问路径就为 根域名/index.html,注意localhost:8001/swagger是访问不到的
                                                      //路径配置，设置为空，表示直接在根域名（localhost:8001）访问该文件
                                                      // c.RoutePrefix = "swagger"; // 如果你想换一个路径，直接写名字即可，比如直接写c.RoutePrefix = "swagger"; 则访问路径为 根域名/swagger/index.html
                });
                #endregion
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllers();
            });

        }
    }
}
