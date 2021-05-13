using WX.Common;
using System;
using static WX.Common.Uilt.Tools;

namespace WX.DataCache
{
    public class Config
    {
        /// <summary>
        /// 是否隐藏所有的api接口
        /// </summary>
        public static bool HiddenApi => ConfigurationManager.AppSettings["Base:HiddenApi"].ToBool(false);
        /// <summary>
        /// 是否隐藏所有的api接口
        /// </summary>
        public static string RedisUrl=> ConfigurationManager.AppSettings["Cache:Redis:ConnString"].ToString();
        /// <summary>
        /// 配置数据库连接字符串
        /// </summary>
        public static string Dbconstr => ConfigurationManager.AppSettings["DBConnStr:Sys_parmt:MySqlConnection"].ToString();
    }
}
