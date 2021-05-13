using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using WX.Common;
using static WX.Common.Uilt.Tools;

namespace WX.DB
{
   public class Config
    {
        public static MySqlConnection HiddenApi =>new MySqlConnection(WX.DataCache.Config.Dbconstr);


        /// <summary>
        /// 检查是否是只读连接并且标记为只读的， 休眠一秒
        /// </summary>
        /// <param name="connectionString"></param>
        public static void CheckIsOnlyReadDB(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return;
            try
            {
                var http = WX.Common.Uilt.HttpContext.Current;  //httpContent对象httpContent对象  
                if (http != null && connectionString.Contains("ReadOnly"))
                {
                    var isOnlyReadDBObj = http.Items["isOnlyReadDB"];
                    if (isOnlyReadDBObj != null)
                    {
                        var isOnlyReadDB = isOnlyReadDBObj.ToBool(false);
                        if (isOnlyReadDB)
                        {
                            //List action用的是只读库，休眠一秒后在取数据
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                }

            }
            catch (Exception)
            {
            }
        }
    }
}
