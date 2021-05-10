using Common.Uilt;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WX.SCRM.Uilt
{
    [Produces("application/json")] //返回数据的格式 直接约定为Json
    [Route("api/[controller]/[action]")]  //路由
    public class BaseController : Controller
    {
        /// <summary>
        /// 返回失败
        /// </summary>
        /// <param name="code"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        protected ResponseEntity ErrorResult(ResponseCode code = ResponseCode.请求失败, string error = "")
        {
            return new ResponseEntity { body = null, code = code.GetHashCode(), message = string.IsNullOrEmpty(error) ? code.ToString() : error };
        }

        /// <summary>
        /// 返回成功
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        protected ResponseEntity SuccessResult(object body = null, ResponseCode code = ResponseCode.请求成功, string message = "请求成功")
        {
            return new ResponseEntity { body = body, code = code.GetHashCode(), message = message };
        }
    }
}
