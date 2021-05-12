using Common;
using System;
using static Common.Uilt.Tools;

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
    }
}
