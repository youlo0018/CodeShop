using CSRedis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WX.Comcon.Caching.Redis
{
    public static class RedisExtensions
    {
        //使用LUA调用
        private const string HmGetScript = (@"return redis.call('HMGET', KEYS[1], unpack(ARGV))");
        internal static object[] HashMemberGet(this CSRedisClient cache, string key, params string[] members)
        {
            object[] result = cache.Eval(
                    HmGetScript,
                    key,
                    GetRedisMembers(members)) as object[];

            return result;
        }

        internal static async Task<object[]> HashMemberGetAsync(
            this CSRedisClient cache,
            string key,
            params string[] members)
        {
            var task = cache.EvalAsync(
                    HmGetScript,
                    key,
                    GetRedisMembers(members));
            await task.ConfigureAwait(false);

            return await task as object[];
        }

        private static string[] GetRedisMembers(params string[] members)
        {
            return members;
        }
    }
}
