using System;
using System.Collections.Generic;
using System.Text;

namespace WX.Common.ApiResults
{
    /// <summary>
    /// 分页结果
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class PagedResult<TResult>
    {
        public PagedResult(IEnumerable<TResult> items, int totalCount, int page, int pageSize)
        {
            List = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
        /// <summary>
        /// 分页集合
        /// </summary>
        public virtual IEnumerable<TResult> List { get; private set; }
        /// <summary>
        /// 总数据量
        /// </summary>
        public virtual int TotalCount { get; private set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        public virtual int Page { get; private set; }
        /// <summary>
        /// 页码大小
        /// </summary>
        public virtual int PageSize { get; private set; }
    }
    /// <summary>
    /// 分页结果拓展类
    /// </summary>
    public static class PagedResultExtension
    {
        /// <summary>
        /// 分页结果
        /// </summary>
        /// <typeparam name="T">分页数据项类型</typeparam>
        /// <param name="items">分页数据项</param>
        /// <param name="totalCount">总数居量</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">页码大小</param>
        /// <returns></returns>
        public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            return new PagedResult<T>(items, totalCount, page, pageSize);
        }
    }
}
