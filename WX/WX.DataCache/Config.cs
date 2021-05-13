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
        /// 当前项目名
        /// </summary>
        public static string Dominnmae => ConfigurationManager.AppSettings["Base:SyyParam"].ToString();
        /// <summary>
        /// redis连接语句
        /// </summary>
        public static string RedisUrl=> ConfigurationManager.AppSettings["Cache:Redis:ConnString"].ToString();
        /// <summary>
        /// 公共配置数据库连接字符串
        /// </summary>
        public static string Dbconstr => ConfigurationManager.AppSettings["DBConnStr:MySqlConnection:Sys_param"].ToString();
        /// <summary>
        /// 本地配置数据库连接字符串
        /// </summary>
        public static string DominDbconstr => ConfigurationManager.AppSettings["DBConnStr:MySqlConnection:Domin_param"].ToString();
    }
}
