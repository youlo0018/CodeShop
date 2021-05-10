using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Uilt
{
    /// <summary>
    /// 外部接口返回实体
    /// </summary>
    public class ResponseEntity
    {

        /// <summary>
        /// 是否成功
        /// </summary>
        public int code { get; set; }


        /// <summary>
        /// 应答描述
        /// </summary>
        public string message { get; set; }




        /// <summary>   
        /// 返回对象内容(加密的)
        /// </summary>
        public object body { get; set; }

    }

    /// <summary>
    /// 接口统一请求实体
    /// </summary>
    public class EntityRequest
    {

        /// <summary>
        /// 加密的字符串
        /// </summary>
        public string ck_value
        {
            get; set;
        }
        /// <summary>
        /// 不加密的字符串
        /// </summary>
        public object other_value
        {
            get; set;
        }
    }
}
