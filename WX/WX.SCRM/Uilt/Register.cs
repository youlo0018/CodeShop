using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WX.Comcon.Caching;

namespace WX.SCRM.Uilt
{
    public static class Register
        
    {
      

        public static void AddDominServices(this IServiceCollection service)
        {
            service.AddSingleton<ConfigureCache>();
            var serpro = service.BuildServiceProvider();
            serpro.GetService<ConfigureCache>().Configureinjected();
            
        }
        
    }

}
