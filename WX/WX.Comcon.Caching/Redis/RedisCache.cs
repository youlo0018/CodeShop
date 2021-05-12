using CSRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WX.Comcon.Caching.Abstractions;

namespace WX.Comcon.Caching.Redis
{
   public class RedisCache
    {
        private const string AbsoluteExpirationKey = "absexp"; //绝对过期key前缀
        private const string SlidingExpirationKey = "sldexp";  //变化过期key前缀
        private const string DataKey = "data";  //数据key前缀
        private const long NotPresent = -1;

        private RedisCacheOptions _options;
        private string _instanceName;
        private static readonly Lazy<RedisCache> _instance = new Lazy<RedisCache>(() => new RedisCache());

        private CSRedis.CSRedisClient redisClient;
        public static RedisCache Instance
        {
            get { return _instance.Value; }
        }

        internal void Init(RedisCacheOptions redisCacheOptions)
        {

            _options = redisCacheOptions;

            _instanceName = redisCacheOptions.InstanceName ?? string.Empty;

            string connString = redisCacheOptions.Configuration;

            redisClient = new CSRedisClient(connString);

            HoldConnectAlive();
        }
        const int WatchDelay = 4000;
        private Task HoldConnectAlive()
        {
            Task aliveConnect = new Task(async () =>
            {
                while (true)
                {
                    redisClient.Set(Guid.NewGuid().ToString(), "", TimeSpan.FromMilliseconds(1)); //心跳包保持长连接
                    await Task.Delay(WatchDelay);
                }
            }, TaskCreationOptions.LongRunning);

            aliveConnect.ConfigureAwait(false);
            aliveConnect.Start();
            return aliveConnect;
        }
        public RedisCache(IOptions<RedisCacheOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _options = optionsAccessor.Value;

            _instanceName = _options.InstanceName ?? string.Empty;
        }

        public RedisCache()
        {

        }
        public string GetRedisInfo()
        {
            var option = ConfigurationOptions.Parse(_options.Configuration);
            return $"ConnectTimeout:{option.ConnectTimeout.ToString()},AsyncTimeout:{option.AsyncTimeout},AbortOnConnectFail:{option.AbortOnConnectFail}";
        }
        public void Dispose()
        {
            if (redisClient != null)
            {
                redisClient.Dispose();
            }
        }
        public long Increment(string key, int value = 1)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return redisClient.IncrBy(key, value);
        }

        public async ValueTask<long> IncrementAsync(string key, int value = 1, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            return await redisClient.IncrByAsync(key, value).ConfigureAwait(false);
        }

        public long ZAdd(string key, string member, decimal score)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            return redisClient.ZAdd(key, (score, member));
        }

        public async ValueTask<long> ZAddAsync(string key, string member, decimal score, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }
            token.ThrowIfCancellationRequested();

            return await redisClient.ZAddAsync(key, (score, member)).ConfigureAwait(false);
        }

        public (string member, decimal score)[] SortedRangeByScoreWithScores(string key, decimal min, decimal max, long count, long skip = 0, Order order = Order.Ascending)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (count <= 0)
            {
                throw new ArgumentNullException(nameof(count));
            }

            if (skip < 0)
            {
                throw new ArgumentNullException(nameof(skip));
            }

            switch (order)
            {
                case Order.Ascending:
                default:
                    return redisClient.ZRangeByScoreWithScores(key, min, max, count, skip);
                case Order.Descending:
                    return redisClient.ZRevRangeByScoreWithScores(key, min, max, count, skip);
            }
        }

        public async Task<(string member, decimal score)[]> SortedRangeByScoreWithScoresAsync(string key, decimal min, decimal max, long count, long skip = 0, Order order = Order.Ascending, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (count <= 0)
            {
                throw new ArgumentNullException(nameof(count));
            }

            if (skip < 0)
            {
                throw new ArgumentNullException(nameof(skip));
            }

            token.ThrowIfCancellationRequested();
            switch (order)
            {
                case Order.Ascending:
                default:
                    return await redisClient.ZRangeByScoreWithScoresAsync(key, min, max, count, skip).ConfigureAwait(false);
                case Order.Descending:
                    return await redisClient.ZRevRangeByScoreWithScoresAsync(key, min, max, count, skip).ConfigureAwait(false);
            }
        }

        #region Get
        public object Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetAndRefresh(key, getData: true);
        }

        public async Task<object> GetAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            return await GetAndRefreshAsync(key, getData: true, token: token).ConfigureAwait(false);
        }
        #endregion

        #region ScanKeys

        /// <summary>
        /// ScanKeys，不适用于集群
        /// </summary>
        /// <param name="database"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<string> Keys(int? database = null, int page = 1, int pageSize = 250)
        {
            if (database < -1)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (page < 1)
            {
                throw new ArgumentNullException(nameof(page));
            }

            if (pageSize < 0)
            {
                throw new ArgumentNullException(nameof(pageSize));
            }
            int pageOffSet = (page - 1) * pageSize; //位移多少条
            return redisClient.Scan(pageOffSet, null, pageSize).Items.ToList();
        }
        #endregion

        #region Lock
        const int DefaultLockTime = 100;  //毫秒,一般锁locktime并发差值大于30ms即可，一般使用50ms，为求稳使用100ms

        /// <summary>
        /// 设置分布式锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="lockTimeOnMillisecond"></param>
        /// <returns></returns>
        public bool LockSet(string key, int lockTimeOnMillisecond = DefaultLockTime)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            try
            {
                //{0} = Key
                //{1} = 锁值
                //{2} = 锁过期时间 on Millisecond
                string lockKey = _instanceName + ".Key." + key;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                bool isLock = redisClient.Set(key, key, TimeSpan.FromMilliseconds(lockTimeOnMillisecond), RedisExistence.Nx);
                sw.Stop();
                Console.WriteLine("锁耗时:" + sw.ElapsedMilliseconds.ToString() + "ms");
                return isLock;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LockSet:" + ex.Message);
                return false;
            }
        }

        public async ValueTask<bool> LockSetAsync(string key, int lockTimeOnMillisecond = DefaultLockTime, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            string lockKey = _instanceName + ".Key." + key;
            bool result = await redisClient.SetAsync(key, key, TimeSpan.FromMilliseconds(lockTimeOnMillisecond), RedisExistence.Nx);
            return result;
        }

        // KEYS[1] = 锁key
        // ARGV[1] = 对应值
        string lockReleaseScript = (@"
                        if redis.call('get',KEYS[1]) == ARGV[1]
                          then return redis.call('del',KEYS[1])  
                        else 
                          return 0 end");
        public bool LockRelease(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            string lockKey = _instanceName + ".Key." + key;

            bool isSuccess = redisClient.Del(key) > 0;
            return isSuccess;
        }

        public async ValueTask<bool> LockReleaseAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            string lockKey = _instanceName + ".Key." + key;
            var task = redisClient.DelAsync(key);
            await task.ConfigureAwait(false);

            bool isSuccess = await task > 0;
            return isSuccess;
        }
        #endregion

        #region Set
        // KEYS[1] = = key
        // ARGV[1] = 绝对过期时间(AbsoluteExpiration) - ticks as long (-1 是没有值)
        // ARGV[2] = 变化过期时间(SlidingExpiration) - ticks as long (-1 是没有值)
        // ARGV[3] = 相对过期时间(计算得出) (seconds as long, -1 是没有值) - Min(AbsoluteExpiration - Now, SlidingExpiration)，变化过期时间和绝对过期时间的最小值
        // ARGV[4] = data - byte[]
        // LUA脚本
        private const string SetScript = (@"
                redis.call('HMSET', KEYS[1], 'absexp', ARGV[1], 'sldexp', ARGV[2], 'data', ARGV[4])
                if ARGV[3] ~= '-1' then
                  redis.call('EXPIRE', KEYS[1], ARGV[3])
                end
                return 1");
        public void Set(string key, object value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var creationTime = DateTimeOffset.UtcNow;  //获取Utc时间

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            var result = redisClient.Eval(SetScript, _instanceName + key, new object[] {
                absoluteExpiration?.Ticks ?? NotPresent,
                options.SlidingExpiration?.Ticks ?? NotPresent,
                GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
                value

            });
        }

        public async Task SetAsync(string key, object value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            token.ThrowIfCancellationRequested();

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            await redisClient.EvalAsync(SetScript, _instanceName + key, new object[] {
                absoluteExpiration?.Ticks ?? NotPresent,
                options.SlidingExpiration?.Ticks ?? NotPresent,
                GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
                value

            }).ConfigureAwait(false);
        }

        private static long? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
        {
            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                return (long)Math.Min(
                    (absoluteExpiration.Value - creationTime).TotalSeconds,
                    options.SlidingExpiration.Value.TotalSeconds);
            }
            else if (absoluteExpiration.HasValue)
            {
                return (long)(absoluteExpiration.Value - creationTime).TotalSeconds;
            }
            else if (options.SlidingExpiration.HasValue)
            {
                return (long)options.SlidingExpiration.Value.TotalSeconds;
            }
            return null;
        }

        private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(DistributedCacheEntryOptions.AbsoluteExpiration),
                    options.AbsoluteExpiration.Value,
                    "绝对过期时间必须比当前时间大。");
            }
            var absoluteExpiration = options.AbsoluteExpiration;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = creationTime + options.AbsoluteExpirationRelativeToNow;
            }

            return absoluteExpiration;
        }
        #endregion

        #region 过期时间
        /// <summary>
        /// 指定时间让key过期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool RefreshKeyTime(string key, DateTime dateTime)
        {
            return true;
        }
        public async ValueTask<bool> RefreshKeyTimeAsync(string key, DateTime dateTime, CancellationToken token = default)
        {
            return true;
        }
        #endregion

        #region Refresh
        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            GetAndRefresh(key, getData: false);
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await GetAndRefreshAsync(key, getData: false, token: token).ConfigureAwait(false);
        }

        private async Task RefreshAsync(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            // 如果是绝对过期，刷新就没有效果
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                await redisClient.ExpireAsync(_instanceName + key, expr.Value.Seconds).ConfigureAwait(false);
            }
        }

        private void Refresh(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // 如果是绝对过期，刷新就没有效果
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                redisClient.Expire(_instanceName + key, expr.Value.Seconds);
            }
        }
        #endregion

        #region Remove
        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            redisClient.Del(_instanceName + key);
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }


            await redisClient.DelAsync(_instanceName + key).ConfigureAwait(false);
        }
        #endregion

        #region Get And Refresh
        private object GetAndRefresh(string key, bool getData)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }


            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            object[] results;
            if (getData)
            {
                results = redisClient.HashMemberGet(_instanceName + key, AbsoluteExpirationKey, SlidingExpirationKey, DataKey);
            }
            else
            {
                results = redisClient.HashMemberGet(_instanceName + key, AbsoluteExpirationKey, SlidingExpirationKey);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                Refresh(key, absExpr, sldExpr);
            }

            if (results.Length >= 3 && results[2] != null)
            {
                return results[2];
            }

            return null;
        }

        private async Task<object> GetAndRefreshAsync(string key, bool getData, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();


            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            object[] results;
            if (getData)
            {
                results = await redisClient.HashMemberGetAsync(_instanceName + key, AbsoluteExpirationKey, SlidingExpirationKey, DataKey);
            }
            else
            {
                results = await redisClient.HashMemberGetAsync(_instanceName + key, AbsoluteExpirationKey, SlidingExpirationKey);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                await RefreshAsync(key, absExpr, sldExpr, token);
            }

            if (results.Length >= 3 && results[2] != null)
            {
                return results[2];
            }

            return null;
        }

        private void MapMetadata(object[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            long? absoluteExpirationTicks = results[0] == null ? null : (long?)long.Parse(results[0].ToString());
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != NotPresent)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }
            var slidingExpirationTicks = results[1] == null ? null : (long?)long.Parse(results[1].ToString());
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != NotPresent)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }
        #endregion

    }
}
