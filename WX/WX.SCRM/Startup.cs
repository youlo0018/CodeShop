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
                    Version = "V1",   //�汾 
                    Title = $"XUnit.Core �ӿ��ĵ�-NetCore3.1",  //����
                    Description = $"XUnit.Core Http API v1",    //����
                    Contact = new OpenApiContact { Name = "Youlo0018", Email = "", Url = new Uri("http://baidu.com") },
                    License = new OpenApiLicense { Name = "Youlo0018���֤", Url = new Uri("http://baidu.com") }
                });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//��ȡӦ�ó�������Ŀ¼�����ԣ����ܹ���Ŀ¼Ӱ�죬������ô˷�����ȡ·����
                                                                                        //var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "WX.SCRM.xml");//������Ǹո����õ�xml�ļ���
                c.IncludeXmlComments(xmlPath, true);//Ĭ�ϵĵڶ���������false,�Է�����ע��
                c.DocumentFilter<HiddenApiFilter>();
                // c.IncludeXmlComments(xmlPath,true); //�����controller��ע��
            });
            #endregion
            #region Redis
            services.AddDistributedRedisCache(new Comcon.Caching.Redis.RedisCacheOptions
            {
                Configuration = DataCache.Config.RedisUrl,
                InstanceName = ""
            });
            #endregion
            #region ע�Ỻ���ʵ�����֤
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
                #region ����������ʾswagger
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/V1/swagger.json", $"XUnit.Core V1");
                    c.RoutePrefix = string.Empty;     //�����Ϊ�� ����·����Ϊ ������/index.html,ע��localhost:8001/swagger�Ƿ��ʲ�����
                                                      //·�����ã�����Ϊ�գ���ʾֱ���ڸ�������localhost:8001�����ʸ��ļ�
                                                      // c.RoutePrefix = "swagger"; // ������뻻һ��·����ֱ��д���ּ��ɣ�����ֱ��дc.RoutePrefix = "swagger"; �����·��Ϊ ������/swagger/index.html
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
