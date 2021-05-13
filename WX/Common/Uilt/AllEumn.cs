using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WX.Common.Uilt
{
    public enum ResponseCode
    {
        #region 公共返回码
        /// <summary>
        /// 请求成功
        /// </summary>
        [Description("请求成功")]
        请求成功 = 10000,
        /// <summary>
        /// 请求处理中
        /// </summary>
        [Description("请求处理中")]
        请求处理中 = 10001,
        /// <summary>
        /// 请求已受理
        /// </summary>
        [Description("请求已受理")]
        请求已受理 = 10002,
        /// <summary>
        /// 请求失败
        /// </summary>
        [Description("请求失败")]
        请求失败 = 10003,
        /// <summary>
        /// 查询失败
        /// </summary>
        [Description("查询失败")]
        查询失败 = 10005,
        /// <summary>
        /// 交易关闭
        /// </summary>
        [Description("交易关闭")]
        交易关闭 = 10006,
        /// <summary>
        /// 记录不存在
        /// </summary>
        [Description("记录不存在")]
        记录不存在 = 10007,
        /// <summary>
        /// 用户不存在
        /// </summary>
        [Description("用户不存在")]
        用户不存在 = 10008,
        /// <summary>
        /// 系统超时
        /// </summary>
        [Description("系统超时")]
        系统超时 = 10098,
        /// <summary>
        /// 系统异常
        /// </summary>
        [Description("系统异常")]
        系统异常 = 10099,
        /// <summary>
        /// 并发异常
        /// </summary>
        [Description("并发异常")]
        并发异常 = 10097,
        /// <summary>
        /// 系统繁忙
        /// </summary>
        [Description("系统繁忙")]
        系统繁忙 = 10096,
        /// <summary>
        /// 请求参数非法
        /// </summary>
        [Description("请求参数非法")]
        请求参数非法 = 10100,
        /// <summary>
        /// 验证签名失败
        /// </summary>
        [Description("验证签名失败")]
        验证签名失败 = 10102,
        /// <summary>
        /// 无权限
        /// </summary>
        [Description("无权限")]
        无权限 = 10103,
        #endregion

        #region 私有返回码

        #endregion
    }
}
