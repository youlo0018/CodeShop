using System;
using System.Collections.Generic;
using System.Text;

namespace WX.Comcon.Caching.Abstractions
{
   public class DistributedCacheEntryOptions
    {
        private DateTimeOffset? _absoluteExpiration;
        private TimeSpan? _absoluteExpirationRelativeToNow;
        private TimeSpan? _slidingExpiration;

        /// <summary>
        /// 获取或设置绝对过期时间用于Cache entry
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration
        {
            get
            {
                return _absoluteExpiration;
            }
            set
            {
                _absoluteExpiration = value;
            }
        }

        /// <summary>
        /// 获取绝对过期时间相对当前时间的时间差值
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get
            {
                return _absoluteExpirationRelativeToNow;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(AbsoluteExpirationRelativeToNow),
                        value,
                        "AbsoluteExpirationRelativeToNow应该大于0");
                }

                _absoluteExpirationRelativeToNow = value;
            }
        }

        /// <summary>
        /// 用于设置一个缓存项在多久一直处于未活动状态（比如一直没被访问）之后将被删除的时间
        /// 注意SlidingExpiration将不会延长一个缓存项的绝对过期时间
        /// </summary>
        public TimeSpan? SlidingExpiration
        {
            get
            {
                return _slidingExpiration;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(SlidingExpiration),
                        value,
                        "SlidingExpiration应该大于0");
                }
                _slidingExpiration = value;
            }
        }
    }
}
