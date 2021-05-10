using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace WX.AdvancedTools
{
    public class config
    { /// <summary> 
      /// 隐藏接口，不生成到swagger文档展示 
      /// </summary> 
        [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Class)]
        public partial class HiddenApiAttribute : System.Attribute
        {

            public HiddenApiAttribute() { }


            /// <summary>
            /// 需要在哪个运行模式隐藏
            /// 0：忽略模式，始终隐藏
            /// 1：当项目运行为ddxd模式时，隐藏
            /// 2：当项目运行为nexten模式时，隐藏
            /// </summary>
            /// <param name="_runMode"></param>
            public HiddenApiAttribute(int _runMode)
            {
                runMode = _runMode;
            }
            public int runMode { get; set; } = 0;

        }
        public class HiddenApiFilter : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                var HiddenApi = WX.DataCache.Config.HiddenApi;
                if (HiddenApi)
                {
                    foreach (var ignoreApi in context.ApiDescriptions)
                    {
                        swaggerDoc.Paths.Remove("/" + ignoreApi.RelativePath);
                    }
                }
                else
                {
                    var ignoreApis = context.ApiDescriptions.Where(x => x.CustomAttributes().Any(any => any is HiddenApiAttribute));
                    if (ignoreApis != null)
                    {
                        foreach (var ignoreApi in ignoreApis)
                        {
                            swaggerDoc.Paths.Remove("/" + ignoreApi.RelativePath);
                        }
                    }
                }




            }


        }
    }
}
