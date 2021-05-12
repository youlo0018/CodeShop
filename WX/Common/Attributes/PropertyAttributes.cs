using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Attributes
{
    public class PropertyAttributes
    {
        public IMemoryCache _memoryCache;
        public PropertyAttributes(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            //_memoryCache = new MemoryCache(new MemoryCacheOptions
            //{
            //});
        }
        /// <summary>
        /// 要验证的实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="param">不需要验证字段</param>
        /// <returns></returns>
        public string PropertyAttribute<T>(T entity, List<string> param = null) where T : class
        {
            var properties = GetPropertyInfo<T>(entity) as PropertyInfo[];//通过反射获取所有属性
            if (properties == null || properties.Count() == 0)
                return "实体类反射失败";
            foreach (PropertyInfo item in properties as PropertyInfo[])
            {
                if (param != null && param.Count != 0 && param.Contains(item.Name))
                    continue;
                var error = Attributeverify(item, entity);
                if (!string.IsNullOrEmpty(error))
                    return error;
            }
            return null;
        }
        /// <summary>
        /// 验证 Attribute具体详情 验证自加
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private string Attributeverify(PropertyInfo item, object entity)
        {
            if (item.IsDefined(typeof(RequiredAttribute), true))//判断该属性是否被RequiredAttribute特性进行标识
            {
                //字段被RequiredAttribute标识了
                var value = item.GetValue(entity);//反射获取属性值
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))//如果字段值为null 或""  "  ",则验证不通过
                {
                    return ((System.ComponentModel.DataAnnotations.ValidationAttribute)item.GetCustomAttribute(typeof(RequiredAttribute))).ErrorMessage;
                }
            }
            if (item.IsDefined(typeof(StringLengthAttribute), true))//判断该属性是否被StringLengthAttribute特性进行标识
            {
                //字段被StringLengthAttribute标识了
                var value = item.GetValue(entity);//反射获取属性值
                                                  //反射实例化StringLengthAttribute
                StringLengthAttribute attribute = item.GetCustomAttribute(typeof(StringLengthAttribute), true) as StringLengthAttribute;
                if (attribute == null)
                {
                    throw new Exception("StringLengthAttribute not instantiate");
                }
                if (value == null)
                    return null;
                if (value.ToString().Length < attribute.MinimumLength || value.ToString().Length > attribute.MaximumLength)
                {
                    return attribute.ErrorMessage;
                }

            }
            if (item.IsDefined(typeof(RegularExpressionAttribute), true))
            {
                var value = item.GetValue(entity);
                RegularExpressionAttribute attribute = item.GetCustomAttribute(typeof(RegularExpressionAttribute), true) as RegularExpressionAttribute;
                if (attribute == null)
                {
                    throw new Exception("RegularExpressionAttribute not instantiate");
                }
                if (value == null)
                    return null;
                if (!Regex.IsMatch(value.ToString(), attribute.Pattern))
                {
                    return attribute.ErrorMessage;
                }
            }
            return null;
        }
        private PropertyInfo[] GetPropertyInfo<T>(T eneity) where T : class
        {
            string Key = "Cache." + typeof(T).Name;// typeof(T).Name;
            PropertyInfo[] properties = _memoryCache.Get(Key) as PropertyInfo[];
            if (properties == null || properties.Count() == 0)
            {
                properties = InitPropertyInfo<T>(Key, eneity);
            }
            return properties;
        }
        private PropertyInfo[] InitPropertyInfo<T>(string key, T entity) where T : class
        {
            PropertyInfo[] merchantAccount = _memoryCache.Get(key) as PropertyInfo[];

            if (merchantAccount != null && merchantAccount.Count() > 0)
            {
                return merchantAccount;
            }
            Type type = entity.GetType();
            var properties = type.GetProperties() as PropertyInfo[];
            if (properties == null || properties.Count() == 0)
                return null;
            _memoryCache.Set(key, properties);
            return properties;

        }
        public (PropertyInfo[] PropertyInfos, string Typename) PropertyInfo<T>(T entity, TimeSpan? closeTime = null) where T : class
        {
            var Typename = "CacheKey." + typeof(T).Name;
            PropertyInfo[] merchantAccount = _memoryCache.Get(Typename) as PropertyInfo[];
            if (merchantAccount != null && merchantAccount.Count() > 0)
            {
                return (merchantAccount, Typename);
            }
            Type type = entity.GetType();
            var properties = type.GetProperties() as PropertyInfo[];
            if (properties == null || properties.Count() == 0)
                return (null, "");
            if (closeTime != null)
                _memoryCache.Set(Typename, properties, closeTime.Value);
            else
                _memoryCache.Set(Typename, properties);

            return (properties, Typename);


        }
    }
}
