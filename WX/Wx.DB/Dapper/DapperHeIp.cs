using Dapper;
using Ganss.XSS;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WX.Common;
using WX.Common.ApiResults;

namespace WX.DB.Dapper
{
    public static class DapperHeIp
    {
        /// <summary>
        /// 是否保存sql在执行中的错误
        /// </summary>
        public static bool isSaveErrorLog = true;
        

        private static object CheckWhereParam(this object whereParam)
        {
            var param = whereParam;
            if (param != null)
            {
                if (whereParam is string)
                    param = null;

            }
            return param;
        }
        #region 查询一个对象




        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conStr"></param>
        /// <param name="whereParam">查询条件，支持new{id=1,status=1}对象和字段名称=value</param>
        /// <param name="selectField">要返回的字段，null为返回所有字段，返回某个字段则为：new{id=0,title="",status=0}或字段名称+逗号的形式</param>
        /// <returns></returns>
        public static T GetModel<T>(this DbConnection conStr, object selectField, object whereParam)
        {
            var querySql = GetAntiXssSql(CreateQueryTSql(GetTableName(typeof(T)), selectField, whereParam));
            try
            {
              
                    var reuslt = conStr.Query<T>(querySql, whereParam.CheckWhereParam());
                    if (reuslt != null)
                        return reuslt.FirstOrDefault();
                    return default(T);
                
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, querySql);
                return default(T); ;
            }
        }



        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conStr"></param>
        /// <param name="filterDeleted"> </param>
        /// <returns></returns>
        public static T GetModel<T>(this DbConnection conStr, bool filterDeleted = true)
        {
            var querySql = $"select * from  {GetTableName(typeof(T))}(nolock)   {(filterDeleted ? " where deleted=0" : "")}  ";
            try
            {
             
                    var list = conStr.Query<T>(querySql, null);
                    if (list != null)
                        return list.FirstOrDefault();

                    return default(T);
                

            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name + conStr, querySql);
                return default(T); ;
            }
        }


        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conStr"></param>
        /// <param name="id"> </param>
        /// <returns></returns>
        public static T GetModel<T>(this DbConnection conStr, int id, bool filterDeleted = true)
        {
            var querySql = $"select * from  {GetTableName(typeof(T))}(nolock) where id={id} {(filterDeleted ? " and deleted=0" : "")}  ";
            try
            {
               
                    var list = conStr.Query<T>(querySql, null);
                    if (list != null)
                        return list.FirstOrDefault();

                    return default(T);
                

            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name + conStr, querySql);
                return default(T); ;
            }
        }


        /// <summary>
        /// 执行sql并返回单个对象
        /// </summary>
        /// <typeparam name="T">model类型</typeparam>
        /// <param name="con">直接调用DapperConnection类</param>
        /// <param name="sql">sql语句</param>
        /// <param name="param">sql里面用的参数，假设用到@a，则传入new {a='xxx'},传入对象即可</param>
        /// <returns>查询到的对象，可能为空</returns>
        public static T GetModelBySql<T>(this string conStr, string sql, object param = null)
        {
            //if (sql.ToLower().Contains("where") && param == null)
            //    throw new Exception("请使用参数查询");
            try
            {
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    var reuslt = con.Query<T>(GetAntiXssSql(sql), param.CheckWhereParam());
                    if (reuslt != null)
                        return reuslt.FirstOrDefault();
                    return default(T);


                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return default(T);
            }
        }


        /// <summary>
        /// 执行sql并返回单个对象
        /// </summary>
        /// <typeparam name="T">model类型</typeparam>
        /// <param name="con">直接调用DapperConnection类</param>
        /// <param name="sql">sql语句</param>
        /// <param name="param">sql里面用的参数，假设用到@a，则传入new {a='xxx'},传入对象即可</param>
        /// <returns>查询到的对象，可能为空</returns>
        public static async Task<T> GetModelBySql<T>(this MySqlConnection con, string sql, object param = null)
        {
            //if (sql.ToLower().Contains("where") && param == null)
            //    throw new Exception("请使用参数查询");
            try
            {
                var reuslt = await con.QueryAsync<T>(GetAntiXssSql(sql), param.CheckWhereParam());
                if (reuslt != null)
                    return reuslt.FirstOrDefault();
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DBEx:" + JsonConvert.SerializeObject(ex));
                //AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return default(T);
            }
        }


        /// <summary>
        /// 执行sql并返回自定义对象（注意：获取对象后，一定要判断是否为null）
        /// </summary>
        /// <typeparam name="T">model类型</typeparam>
        /// <param name="con">直接调用DapperConnection类</param>
        /// <param name="sql">sql语句</param>
        /// <param name="param">sql里面用的参数，假设用到@a，则传入new {a='xxx'},传入对象即可，这里不是memberinitexpression</param>
        /// <param name="model">自定义对象，格式：new{name="",age=0}</param>
        /// <returns>查询到的对象，可能为空</returns>
        public static T GetModelBySql<T>(this string conStr, string sql, object param, T model)
        {
            //if (sql.ToLower().Contains("where") && param == null)
            //    throw new Exception("请使用参数查询");
            try
            {
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    var reuslt = con.Query<T>(GetAntiXssSql(sql), param.CheckWhereParam());
                    if (reuslt != null)
                        return reuslt.FirstOrDefault();
                    return default(T);

                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return default(T); ;
            }
        }
        #endregion

        #region insert方法
        /// <summary>
        /// insert 一个实体，返回一个实体（实体包含插入成功的自增id）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="con">直接调用DapperConnection类</param>
        /// <param name="model">要插入的对象</param>
        /// <param name="primaryKey">不需要插入的字段，主要是针对自增主键</param>
        /// <param name="isFilter">sql语句安全过滤</param>
        /// <returns></returns>
        public static T AddModel<T>(this string conStr, T model, string primaryKey = "id", bool isFilter = true)
        where T : class
        {
            var insertParameterSql = string.Empty;
            try
            {
                insertParameterSql = GetInsertParamSql(typeof(T), primaryKey, (primaryKey == "" ? false : true));
                if (isFilter)
                    model = ReturnSecurityObject(model) as T;
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    var identify = con.Query<int>(insertParameterSql, model).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(primaryKey))
                        model = SetIdentify(model, primaryKey, identify);
                    return model;
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, insertParameterSql, model);
                return default(T); ;
            }
        }



        /// <summary>
        /// insert 一个实体，返回受影响行数
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="con">直接调用DapperConnection类</param>
        /// <param name="model">Entity对象</param>
        /// <param name="primaryKey">不需要插入的字段，主要是针对自增主键</param>
        /// <param name="isFilter">sql语句安全过滤</param>
        /// <returns></returns>
        public static int AddModelRowCount<T>(this string conStr, T model, string primaryKey = "id", bool isFilter = true, string asName = "")
        where T : class
        {
            var insertParameterSql = string.Empty;
            try
            {
                insertParameterSql = GetInsertParamSql(typeof(T), primaryKey, false, asName);
                if (isFilter)
                    model = ReturnSecurityObject(model) as T;
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    var identify = con.Execute(insertParameterSql, model);
                    return identify;
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, insertParameterSql, model);
                return 0;
            }
        }


        /// <summary>
        /// insert 一个实体，返回插入后的Id
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="con">直接调用DapperConnection类</param>
        /// <param name="model">Entity对象</param>
        /// <param name="primaryKey">不需要插入的字段，主要是针对自增主键</param>
        /// <param name="isFilter">sql语句安全过滤</param>
        /// <returns></returns>
        public static int AddModelIdentity<T>(this string conStr, T model, string primaryKey = "id", bool isFilter = true)
        where T : class
        {
            var insertParameterSql = string.Empty;
            try
            {
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    insertParameterSql = GetInsertParamSql(typeof(T), primaryKey);
                    if (isFilter)
                        model = ReturnSecurityObject(model) as T;

                    var identify = 0;
                    var reuslt = con.Query<int>(insertParameterSql, model);
                    if (reuslt != null)
                        identify = reuslt.FirstOrDefault();
                    return identify;
                }


            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name + conStr, insertParameterSql, model);
                return 0;
            }
        }


        /// <summary>
        /// 批量插入多个model对象，注：如数据量千条以上，建议使用数据库的自定义表类型来批量插入
        /// </summary>
        /// <param name="conStr">数据库连接字符串</param>
        /// <param name="primaryKey">生成sql的对象的对象</param>
        /// <param name="listModel">待插入数据库的对象</param>
        /// <param name="isFilter">是否安全过滤sql语句</param>
        /// <param name="asName">表名</param>
        /// <returns></returns>
        public static int AddBatchModelRowCount<T>(this string conStr, List<T> listModel, string primaryKey = "id", bool isFilter = true, string asName = "")
        where T : class, new()
        {
            var insertParameterSql = string.Empty;
            try
            {
                insertParameterSql = GetInsertParamSql(typeof(T), primaryKey, false, asName);
                if (isFilter)
                    listModel = ReturnSecurityObject(listModel) as List<T>;
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    var identify = con.Execute(insertParameterSql, listModel);
                    return identify;
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, insertParameterSql, listModel);
                return 0;
            }
        }
      

      


        #endregion


        /// <summary>
        /// 插入错误日志
        /// </summary>
        /// <param name="ex">错误异常</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="tsql">操作的sql语句</param>
        /// <param name="model">操作的对象</param>
        public static void AddDbErrorLog(Exception ex, string methodName, string tsql = "", object model = null)
        {
            if (isSaveErrorLog)
            {
                //DB.DBLog.Error(new Wathet.Common.Entity.DBError()
                //{
                //    ConnectionStr = methodName,
                //    Sql = tsql,
                //    IsTrans = false,
                //    Params = model,
                //    Error = ex,
                //    StackTraceModelName = WX.Common.DoException.GetStackTraceModelName()
                //});
            }
        }



        #region 获取列表






        /// <summary>
        /// 查询分页数据列表
        /// </summary>
        /// <typeparam name="T">查询对象</typeparam>
        /// <param name="con">数据库链接</param>
        /// <param name="fieldParam">查询字段,查询全部为null,查询某个字段，如：new {name="",id=0,status=0}</param>
        /// <param name="whereField">查询条件, sql语句,不包含where</param>
        /// <param name="whereParam">查询条件参数值，如：new {name="danny",status=1}</param>
        /// <param name="orderBy">排序字段 ,正序为true，降序为false，如：new {sort_id=false,id=true}</param>
        /// <param name="pagesize">每页条数</param>
        /// <param name="pageindex">当前页</param>
        /// <param name="totalCount">返回总条数</param>
        /// <returns></returns>
        public static List<T> GetList<T>(this string conStr, object fieldParam, string whereField, object whereParam, object orderBy, int pagesize, int pageindex, out int totalCount)
        {
            Config.CheckIsOnlyReadDB(conStr);
            var safeSql = "";
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(CreateQueryTSql(GetTableName(typeof(T)), fieldParam));

                #region 查询条件
                if (!string.IsNullOrEmpty(whereField.Trim()))
                    sql.Append($" where {whereField.ToString()}");
                #endregion

                var order = CreateOrderByTsql(orderBy, true);

                safeSql = GetAntiXssSql(sql.ToString());
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    totalCount = con.Query<int>(CreateCountingSql(safeSql), whereParam.CheckWhereParam()).First();
                    var pagingSql = CreatePagingSql(totalCount, pagesize, pageindex, safeSql, order);
                    return con.Query<T>(pagingSql, whereParam.CheckWhereParam()).ToList();
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, safeSql, whereParam);
                totalCount = 0;
                return new List<T>();
            }
        }



        /// <summary>
        /// 查询数据列表
        /// </summary>
        /// <typeparam name="T">查询对象</typeparam>
        /// <param name="con">数据库链接</param>
        /// <param name="fieldParam">查询字段,查询全部为null,查询某个字段，如：new {name="",id=0,status=0}</param>
        /// <param name="whereParam">查询条件，如：new {name="danny",status=1}</param>
        /// <param name="orderBy">排序字段 ,正序为true，降序为false，如：new {sort_id=false,id=true}</param>
        /// <returns></returns>
        public static List<T> GetList<T>(this string conStr, object fieldParam, object whereParam = null, object orderBy = null)
        {
            Config.CheckIsOnlyReadDB(conStr);
            var safeSql = string.Empty;
            try
            {
                safeSql = GetAntiXssSql(CreateQueryTSql(GetTableName(typeof(T)), fieldParam, whereParam, orderBy));
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    return con.Query<T>(safeSql, whereParam.CheckWhereParam()).ToList();
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, safeSql, whereParam);
                return new List<T>();
            }

        }




        /// <summary>
        ///获取数据列表
        /// </summary>
        /// <typeparam name="T">获取的列表类型</typeparam>
        /// <param name="conStr"></param>
        /// <param name="con">直接调用DapperConnection类</param>
        /// <param name="sql">sql语句含orderby</param>
        /// <param name="param">参数</param>
        /// <param name="isFilter"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static List<T> GetListBySql<T>(this string conStr, string sql, object param = null, bool isFilter = true, CommandType commandType = CommandType.Text)
        {
            Config.CheckIsOnlyReadDB(conStr);
            try
            {
                //if (sql.ToLower().Contains("where") && param == null)
                //    throw new Exception("请使用参数查询");
                var safeSql = sql;
                if (isFilter)
                    safeSql = GetAntiXssSql(sql);

                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    return con.Query<T>(safeSql, param.CheckWhereParam(), commandType: commandType).ToList();
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return new List<T>();
            }
        }

        //public static async System.Collections.Generic.IAsyncEnumerable<T> GetListBySqlAsync<T>(this string conStr, string sql, object param = null, bool isFilter = true, CommandType commandType = CommandType.Text)
        //{
        //    Config.CheckIsOnlyReadDB(conStr);
        //    try
        //    {
        //        var safeSql = sql;
        //        if (isFilter)
        //            safeSql = GetAntiXssSql(sql);

        //        using MySqlConnection con = new MySqlConnection(conStr);

        //        var reader = await con.QueryMultipleAsync(sql, param.CheckWhereParam(), commandType: commandType);
        //        var idFromDb = (await reader.ReadAsync<int?>().ConfigureAwait(false)).SingleOrDefault();
        //        if (idFromDb == null)
        //        {
        //            return null;
        //        }

        //        var items = await reader.ReadAsync<T>(buffered: false).ConfigureAwait(false);

        //        using (reader)
        //        {
        //            await foreach (var item in items.ToAsyncEnumerable())
        //            {
        //                yield return item;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
        //        throw ex;
        //    }
        //}


        /// <summary>
        ///获取数据列表
        /// </summary>
        /// <typeparam name="T">获取的列表类型</typeparam>
        /// <param name="con">直接调用DapperConnection类</param>
        /// <param name="sql">sql语句含orderby</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static List<T> GetListByStoredProcedure<T>(this MySqlConnection con, string sql, object param = null)
        {

            try
            {
                return con.Query<T>(sql, param.CheckWhereParam(), commandType: CommandType.StoredProcedure).ToList();

            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return new List<T>();
            }
        }

        /// <summary>
        /// Dapper获取分页列表
        /// </summary>
        /// <typeparam name="T">获取的列表类型</typeparam>
        /// <param name="conStr"></param>
        /// <param name="sql">sql语句（不包含orderby以外的部分）</param>
        /// <param name="orderby">orderby的字段，如果多个可用,分隔，逆序可用desc</param>
        /// <param name="pagesize">页大小</param>
        /// <param name="pageindex">当前页</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="param"></param>
        /// <param name="sqlCount"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static List<T> GetListBySql<T>(this string conStr, string sql, string orderby, int pagesize, int pageindex, out int totalCount, object param = null, string sqlCount = null,
            CommandType commandType = CommandType.Text)
        {
            Config.CheckIsOnlyReadDB(conStr);
            try
            {
                //if (sql.ToLower().Contains("where") && param == null)
                //    throw new Exception("请使用参数查询");
                var safeSql = GetAntiXssSql(sql);
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    if (sqlCount == null)
                    {
                        var tmpStr = CreateCountingSql(safeSql);
                        totalCount = con.Query<int>(tmpStr, param.CheckWhereParam()).First();
                    }
                    else
                        totalCount = con.Query<int>(sqlCount).First();
                    var pagingSql = CreatePagingSql(totalCount, pagesize, pageindex, safeSql, orderby);
                    return con.Query<T>(pagingSql, param.CheckWhereParam(), commandType: commandType).ToList();
                }
            }
            catch (Exception ex)
            {
                totalCount = 0;
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return new List<T>();
            }
        }



        /// <summary>
        /// 获取匿名对象列表 
        /// </summary>
        /// <typeparam name="T">匿名对象类型</typeparam>
        /// <param name="conStr"></param>
        /// <param name="con"></param>
        /// <param name="sql"></param>
        /// <param name="param">查询条件的参数值对象</param>
        /// <param name="model">匿名对象，对象的字段顺序及类型要跟sql查询的字段一致</param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static List<T> GetDynamicList<T>(this string conStr, string sql, object param, T model, CommandType commandType = CommandType.Text)
        {
            Config.CheckIsOnlyReadDB(conStr);
            try
            {
                //if (sql.ToLower().Contains("where") && param == null)
                //    throw new Exception("请使用参数查询");
                var safeSql = GetAntiXssSql(sql);
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    return con.Query<T>(safeSql, param.CheckWhereParam(), commandType: commandType).ToList();
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return new List<T>();
            }
        }

        public static IEnumerable<T> QueryList<T>(this string conStr, string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                var safeSql = GetAntiXssSql(sql);
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    return con.Query<T>(safeSql, param.CheckWhereParam(), commandType: commandType);
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                throw ex;
            }
        }

        public static async Task<IEnumerable<T>> QueryListAsync<T>(this string conStr, string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                var safeSql = GetAntiXssSql(sql);
                using MySqlConnection con = new MySqlConnection(conStr);

                return await con.QueryAsync<T>(safeSql, param.CheckWhereParam(), commandType: commandType);

            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                throw ex;
            }
        }

        //public static async IAsyncEnumerable<T> QueryAsyncEnumerable<T>(this string conStr, string sql, object param = null, CommandType commandType = CommandType.Text)
        //{
        //    try
        //    {
        //        var safeSql = GetAntiXssSql(sql);
        //        using MySqlConnection con = new MySqlConnection(conStr);

        //        return (await con.QueryAsync<T>(safeSql, param.CheckWhereParam(), commandType: commandType)).ToAsyncEnumerable();

        //    }
        //    catch (Exception ex)
        //    {
        //        AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
        //        throw ex;
        //    }
        //}

        /// <summary>
        ///获取匿名对象分页列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conStr"></param>
        /// <param name="con"></param>
        /// <param name="sql"></param>
        /// <param name="orderby">排序字段，不包含order by</param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <param name="totalCount"></param>
        /// <param name="model">匿名对象，第一个属性字段一定是row_number，并赋值Int64.MaxValue，对象的字段顺序及类型要跟sql查询的字段一致</param>
        /// <param name="param">查询条件的参数值对象</param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static List<T> GetDynamicList<T>(
            this string conStr,
            string sql,
            string orderby,
            int pagesize,
            int pageindex,
            out int totalCount,
            T model,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            Config.CheckIsOnlyReadDB(conStr);
            try
            {
                //if (sql.ToLower().Contains("where") && param == null)
                //    throw new Exception("请使用参数查询");
                var safeSql = GetAntiXssSql(sql);
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    totalCount = con.Query<int>(CreateCountingSql(safeSql), param.CheckWhereParam()).First();
                    var pagingSql = CreatePagingSql(totalCount, pagesize, pageindex, safeSql, orderby);
                    return con.Query<T>(pagingSql, param.CheckWhereParam(), commandType: commandType).ToList();
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                totalCount = 0;
                return new List<T>();

            }
        }






        /// <summary>
        /// 获取总数
        /// </summary>
        /// <param name="param">sql里面用的参数，假设用到@a，则传入new {a='xxx'},传入对象即可</param>
        /// <returns></returns>
        public static int GetCount<T>(this string conStr, object whereParam) where T : class, new()
        {
            StringBuilder sql = new StringBuilder();
            try
            {
                var properties = typeof(T).GetProperties();
                sql.Append($"select count(*) from {GetTableName(typeof(T))}");
                if (whereParam != null)
                {
                    if (whereParam is string)
                    {
                        sql.Append($" where {whereParam}");
                    }
                    else
                    {
                        sql.Append(" where ");
                        var whereField = whereParam.GetType().GetProperties();
                        for (int i = 0; i < whereField.Length; i++)
                        {
                            if (i > 0)
                                sql.Append(" and  ");
                            sql.Append($" {whereField[i].Name}=@{whereField[i].Name}");
                        }
                    }
                }
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    int count = con.Query<int>(sql.ToString(), whereParam.CheckWhereParam()).FirstOrDefault();
                    return count;
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql.ToString(), whereParam);
                return 0;
            }
        }


        /// <summary>
        /// 获取总数
        /// </summary>
        /// <param name="param">sql里面用的参数，假设用到@a，则传入new {a='xxx'},传入对象即可</param>
        /// <returns></returns>
        public static int GetCount(this string conStr, string sql, object whereParam = null)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    int count = con.Query<int>(sql.ToString(), whereParam).FirstOrDefault();
                    return count;
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, whereParam.CheckWhereParam());
                return 0;
            }
        }


        /// <summary>
        /// 获取单个值数据,需要判断返回的值是否为null
        /// </summary>
        /// <param name="conStr"></param>
        /// <param name="sql">sql语句</param>
        /// <param name="param">sql里面用的参数，假设用到@a，则传入new {a='xxx'},传入对象即可</param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static object GetScalarValue(this string conStr, string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            if (string.IsNullOrEmpty(conStr))
            {
                throw new ArgumentException("message", nameof(conStr));
            }

            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("message", nameof(sql));
            }



            try
            {
                //if (sql.ToLower().Contains("where") && param == null)
                //    throw new Exception("请使用参数查询");
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    return con.ExecuteScalar(GetAntiXssSql(sql), param.CheckWhereParam(), commandType: commandType);
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return null;
            }
        }

        #endregion

        #region update方法
        /// <summary>
        /// update一个实体,实体是先查询出来的
        /// </summary>
        /// <param name="conStr"></param>
        /// <param name="con">直接调用DapperConnection类</param>
        /// <param name="model">需要更新的Entity对象</param>
        /// <param name="primaryKey">不需要更新的字段，主要是针对自增主键</param>
        /// <param name="isFilter"></param>
        /// <param name="asName"></param>
        /// <returns></returns>
        public static bool UpdateModel<T>(this string conStr, T model, string primaryKey = "id", bool isFilter = true, string asName = "") where T : class
        {
            var updateParameterSql = GetUpdateParamSql(typeof(T), primaryKey, asName);
            try
            {
                if (isFilter)
                    model = ReturnSecurityObject(model) as T;
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    return con.Execute(updateParameterSql, model) > 0;
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, updateParameterSql, model);
                return false;
            }
        }

        /// <summary>
        /// 批量更新多个model对象，注：如数据量千条以上，建议使用数据库的自定义表类型来批量更新
        /// </summary>
        /// <typeparam name="T">更新对象的类型</typeparam>
        /// <param name="conStr">链接字符串</param>
        /// <param name="listModel">更新的list</param>
        /// <param name="primaryKey">主键id</param>
        /// <param name="isFilter"></param>
        /// <returns></returns>
        public static int UpdateBatchModelRowCount<T>(this string conStr, List<T> listModel, string primaryKey = "id", bool isFilter = true, string asName = "")
        where T : class, new()
        {
            var sql = string.Empty;
            try
            {
                sql = GetUpdateParamSql(typeof(T), primaryKey, asName);
                if (isFilter)
                    listModel = ReturnSecurityObject(listModel) as List<T>;
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    var identify = con.Execute(sql, listModel);
                    return identify;
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, listModel);
                return 0;
            }
        }



        /// <summary>
        /// 更新对象
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="conStr"></param>
        /// <param name="updateParam">set字段</param>
        /// <param name="whereParam">where字段</param>
        /// <param name="isFilter"></param>
        /// <returns></returns>
        public static bool UpdateModel<T>(this string conStr, object updateParam, object whereParam, bool isFilter = true) where T : class, new()
        {
            T model = new T();
            StringBuilder sql = new StringBuilder();
            var t = typeof(T);
            var properties = t.GetProperties();
            sql.Append($"update {GetTableName(typeof(T))} set ");

            #region 更新字段
            if (updateParam != null)
            {
                if (updateParam is string)
                {
                    sql.Append(updateParam.ToString());
                }
                else
                {
                    var fieldObj = updateParam.GetType().GetProperties();
                    int i = 0;
                    foreach (var pi in fieldObj)
                    {
                        if (i > 0)
                            sql.Append(",");
                        sql.Append($" {pi.Name}=@{pi.Name} ");
                        #region 处理参数
                        var info = properties.Where(x => x.Name.ToLower() == pi.Name.ToLower()).FirstOrDefault();
                        if (info != null)
                        {
                            if (info.PropertyType == "".GetType() && isFilter)//如果属性为string类型
                            {
                                var inputString = (pi.GetValue(updateParam) ?? "").ToString();
                                info.SetValue(model, GetAntiXssSql(inputString));//将过滤后的值设置给传入的对象
                            }
                            else
                            {
                                info.SetValue(model, pi.GetValue(updateParam));
                            }
                        }
                        #endregion
                        i++;
                    }
                }
            }
            #endregion

            #region where条件
            if (whereParam != null)
            {
                sql.Append($" where ");
                if (whereParam is string)
                {
                    sql.Append($"   {whereParam}");
                }
                else
                {
                    var whereField = whereParam.GetType().GetProperties();
                    int i = 0;
                    foreach (var pi in whereField)
                    {
                        if (i > 0)
                            sql.Append(" and ");
                        sql.Append($" {pi.Name}=@{pi.Name} ");

                        var info = properties.Where(x => x.Name.ToLower() == pi.Name.ToLower()).FirstOrDefault();
                        if (info != null)
                        {
                            if (info.PropertyType == "".GetType() && isFilter)//如果属性为string类型
                            {
                                var inputString = (pi.GetValue(whereParam) ?? "").ToString();
                                info.SetValue(model, GetAntiXssSql(inputString));//将过滤后的值设置给传入的对象
                            }
                            else
                            {
                                info.SetValue(model, pi.GetValue(whereParam));
                            }
                        }
                        i++;
                    }
                }
            }

            #endregion
            try
            {
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    int num = con.Execute(sql.ToString(), model);
                    return num > 0;
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql.ToString(), updateParam);
                return false;
            }
        }

        #endregion

        #region 增删改 方法




        /// <summary>
        /// 删除对象
        /// </summary>
        /// <typeparam name="T">要删除的对象</typeparam>
        /// <param name="con"></param>
        /// <param name="whereParam"> 参数， 假设用到@a，则传入new {a='xxx'},传入对象即可</param>
        /// <returns></returns>
        public static bool DeleteModel<T>(this string conStr, object whereParam)
        {
            //if (whereParam == null)
            //    throw new Exception("参数不能为null");
            var sql = $"delete {GetTableName(typeof(T))}  {CreateWhereTsql(whereParam)} ";
            try
            {
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    int num = con.Execute(sql, whereParam);
                    return num > 0;
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, whereParam);
                return false;
            }
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <typeparam name="T">要删除的对象</typeparam>
        /// <param name="conStr"></param>
        /// <param name="id"> 要删除的id</param>
        /// <returns></returns>
        public static bool DeleteModel<T>(this string conStr, int id)
        {

            var sql = $"delete {GetTableName(typeof(T))}  where id={id} ";
            try
            {
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    int num = con.Execute(sql, null);
                    return num > 0;
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, id);
                return false;
            }
        }




        /// <summary>
        /// 执行一条sql语句(增删改)
        /// </summary>
        /// <param name="conStr"></param>
        /// <param name="con">直接调用DapperConnection类</param>
        /// <param name="sql">TSQL语句</param>
        /// <param name="param">sql里面用的参数，假设用到@a，则传入new {a='xxx'},传入对象即可</param>
        /// <param name="saveLog"></param>
        /// <param name="commandType"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static bool ExecuteSql(this string conStr, string sql, object param = null, bool saveLog = true, CommandType commandType = CommandType.Text)
        {
            try
            {
                //if (sql.ToLower().Contains("where") && param == null)
                //    throw new Exception("请使用参数查询");
                string filterSql = GetAntiXssSql(sql);
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    int num = con.Execute(filterSql, param.CheckWhereParam(), commandType: commandType);
                    return num > 0;
                }
            }
            catch (Exception ex)
            {
                if (saveLog)
                    AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return false;
            }
        }


        public static async ValueTask<bool> ExecuteSqlAsync(this string conStr, string sql, object param = null, bool saveLog = true, CommandType commandType = CommandType.Text)
        {
            try
            {
                //if (sql.ToLower().Contains("where") && param == null)
                //    throw new Exception("请使用参数查询");
                string filterSql = GetAntiXssSql(sql);
                using (MySqlConnection con = new MySqlConnection(conStr))
                {
                    int num = await con.ExecuteAsync(filterSql, param.CheckWhereParam(), commandType: commandType);
                    return num > 0;
                }
            }
            catch (Exception ex)
            {
                if (saveLog)
                    AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return false;
            }
        }
        #endregion

        #region 事务处理

        /// <summary>
        /// 增改事物
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="model">操作entity</param>
        /// <param name="tran">事物</param>
        /// <param name="isAdd">是否是添加操作</param>
        /// <param name="primaryKey">主键字段名称</param>
        /// <param name="isFilter">是否安全过滤entity</param>
        /// <returns></returns>
        public static int TransAddOrUpdate<T>(this MySqlConnection con, T model, MySqlTransaction tran, bool isAdd, string primaryKey = "id", bool isFilter = true) where T : class
        {
            int result = 0;
            var sql = "";
            try
            {
                if (isFilter)
                    model = ReturnSecurityObject(model) as T;
                bool wasClosed = con.State == ConnectionState.Closed;
                if (wasClosed) con.Open();
                if (isAdd)
                {
                    sql = GetInsertParamSql(typeof(T), primaryKey);
                    result = con.Query<int>(sql, model, tran).FirstOrDefault();
                }

                else
                {
                    sql = GetUpdateParamSql(typeof(T), primaryKey);
                    result = con.Execute(sql, model, tran);
                }

            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, model);
                return 0;
            }
            return result;
        }


        /// <summary>
        /// 增改事物(批量处理)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="list">操作entity list</param>
        /// <param name="tran">事物</param>
        /// <param name="isAdd">是否是添加操作</param>
        /// <param name="primaryKey">主键字段名称</param>
        /// <param name="isFilter">是否安全过滤entity</param>
        /// <returns></returns>
        public static int TransAddOrUpdateList<T>(this MySqlConnection con, List<T> list, MySqlTransaction tran, bool isAdd, string primaryKey = "id", bool isFilter = true) where T : class
        {
            int result = 0;
            var sql = "";
            try
            {
                if (isFilter)
                    list = ReturnSecurityObject(list) as List<T>;
                bool wasClosed = con.State == ConnectionState.Closed;
                if (wasClosed) con.Open();
                if (isAdd)
                    sql = GetInsertParamSql(typeof(T), primaryKey, false);
                else
                    sql = GetUpdateParamSql(typeof(T), primaryKey);
                result = con.Execute(sql, list, tran);
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, sql);
                return 0;
            }
            return result;
        }



        /// <summary>
        /// 删除事物
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="whereParam">删除条件</param>
        /// <param name="tran">事物</param>
        /// <returns></returns>
        public static int TransDelete<T>(this MySqlConnection con, object whereParam, MySqlTransaction tran) where T : class
        {
            int result = 0;
            var sql = "";
            try
            {
                //if (whereParam == null)
                //    throw new Exception("参数不能为null");
                sql = $"delete { GetTableName(typeof(T))}  {CreateWhereTsql(whereParam)} ";
                bool wasClosed = con.State == ConnectionState.Closed;
                if (wasClosed) con.Open();
                result = con.Execute(sql, whereParam, tran);
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, whereParam);
                return 0;
            }
            return result;
        }



        /// <summary>
        /// 查询数据事物
        /// </summary>
        /// <typeparam name="T">查询对象</typeparam>
        /// <param name="con">数据库链接对象</param>
        /// <param name="tran">事物</param>
        /// <param name="fieldParam">查询字段,查询全部为null,查询某个字段，如：new {name="",id=0,status=0}</param>
        /// <param name="whereParam">查询条件，如：new {name="danny",status=1}</param>
        /// <param name="orderBy">排序字段 ,正序为true，降序为false，如：new {sort_id=false,id=true}</param>
        /// <returns></returns>
        public static List<T> TransQueryList<T>(this MySqlConnection con, MySqlTransaction tran, object whereParam = null, object fieldParam = null, object orderBy = null)
        {
            var safeSql = string.Empty;
            try
            {
                bool wasClosed = con.State == ConnectionState.Closed;
                if (wasClosed) con.Open();
                var tableName = GetTableName(typeof(T));
                safeSql = GetAntiXssSql(CreateQueryTSql(tableName, fieldParam, whereParam, orderBy));
                return con.Query<T>(safeSql, whereParam, tran).ToList();
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, safeSql, whereParam);
                return new List<T>();
            }

        }

        /// <summary>
        /// 查询数据事物
        /// </summary>
        /// <typeparam name="T">查询对象</typeparam>
        /// <param name="con">数据库链接对象</param>
        /// <param name="tran">事物</param>
        /// <param name="fieldParam">查询字段,查询全部为null,查询某个字段，如：new {name="",id=0,status=0}</param>
        /// <param name="whereParam">查询条件，如：new {name="danny",status=1}</param>
        /// <param name="orderBy">排序字段 ,正序为true，降序为false，如：new {sort_id=false,id=true}</param>
        /// <returns></returns>
        public static T TransQueryModel<T>(this MySqlConnection con, MySqlTransaction tran, string sql)
        {

            try
            {
                bool wasClosed = con.State == ConnectionState.Closed;
                if (wasClosed) con.Open();
                return con.Query<T>(sql, null, tran).FirstOrDefault();
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, null);
                return default(T);
            }

        }


        /// <summary>
        /// 增删改事物
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="whereParam">条件</param>
        /// <param name="tran">事物</param>
        /// <returns></returns>
        public static int TransExecute(this MySqlConnection con, string sql, object whereParam, MySqlTransaction tran)
        {
            try
            {
                //if (whereParam == null)
                //    throw new Exception("参数不能为null");
                bool wasClosed = con.State == ConnectionState.Closed;
                if (wasClosed) con.Open();
                return con.Execute(sql, whereParam, tran);
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, whereParam);
                return 0;
            }
        }

        #endregion


        #region 私有方法


        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTableName(Type type)
        {
            var tableName = "t_" + type.Name;
            return tableName;
        }

        #region 构建sql语句

        /// <summary>
        /// 构建sql语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="selectField"></param>
        /// <param name="whereParam"></param>
        /// <param name="orderBy"></param>
        /// <param name="isNoLock"></param>
        /// <param name="pageOrderBy">是否是分页的排序（分页排序不需要加 order by）</param>
        /// <returns></returns>
        private static string CreateQueryTSql(string tableName, object selectField, object whereParam = null, object orderBy = null, bool isNoLock = true, bool pageOrderBy = false)
        {
            var sql = string.Empty;

            #region 查询字段
            if (selectField != null)
                sql = $"select {CreateQueryFiedlTsql(selectField)} from  { tableName}";
            else
                sql = $"select * from  {tableName}{(isNoLock ? "(nolock)" : "")}   ";
            #endregion
            #region where 条件
            sql += CreateWhereTsql(whereParam);

            #endregion
            #region 排序字段
            sql += CreateOrderByTsql(orderBy, pageOrderBy);
            #endregion
            return sql;

        }

        private static string CreateWhereTsql(object whereParam)
        {
            var sql = "";
            if (whereParam != null)
            {
                if (whereParam is string)
                {
                    sql += $" where {whereParam}";
                }
                else
                {
                    sql += " where ";
                    var t = whereParam.GetType();
                    var properties = t.GetProperties();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        if (i > 0)
                            sql += $" and  ";
                        sql += $" {properties[i].Name}=@{properties[i].Name}";
                    }
                }

            }
            return sql;
        }

        private static string CreateQueryFiedlTsql(object field)
        {
            var sql = "";
            if (field != null)
            {
                if (field is string)
                {
                    sql = field.ToString();
                }
                else
                {
                    var fieldObj = field.GetType().GetProperties();
                    for (int i = 0; i < fieldObj.Length; i++)
                    {
                        if (i > 0)
                            sql += ",";
                        sql += $" {fieldObj[i].Name} ";
                    }
                }
            }
            return sql;
        }

        private static string CreateOrderByTsql(object orderBy, bool pageOrderBy = false)
        {
            var sql = "";
            if (orderBy != null)
            {
                if (orderBy is string)
                {
                    sql += $"{(pageOrderBy ? "" : " order by  ")}{orderBy}";
                }
                else
                {
                    sql += $" {(pageOrderBy ? "" : " order by  ")}";
                    var t = orderBy.GetType();
                    var properties = t.GetProperties();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        if (i > 0)
                            sql += $" , ";
                        sql += $" {properties[i].Name} {(properties[i].GetValue(orderBy).ToBool() ? "asc" : "desc")}";
                    }
                }

            }
            return sql;
        }

        #endregion

        /// <summary>
        /// 位置主键的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="primaryKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static T SetIdentify<T>(T model, string primaryKey, int? value)
        where T : class
        {
            primaryKey = primaryKey.ToLower();
            var t = typeof(T);
            var properties = t.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].Name.ToLower() == primaryKey)
                {
                    properties[i].SetValue(model, value);
                    break;
                }
            }
            var fields = t.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Name.ToLower() == primaryKey)
                {
                    fields[i].SetValue(model, value);
                    break;
                }
            }
            return model;
        }

        /// <summary>
        /// 返回安全的entity对象
        /// </summary>
        /// <typeparam name="T">entity对象的类型</typeparam>
        /// <param name="model">entity对象</param>
        /// <returns>安全的entity对象</returns>
        public static object ReturnSecurityObject(object model)
        {
            Type t = model.GetType();//获取类型
            if (t.IsGenericType)
            {
                #region 泛型反射
                var count = Convert.ToInt32(t.GetProperty("Count").GetValue(model, null));
                for (int i = 0; i < count; i++)
                {
                    object listItem = t.GetProperty("Item").GetValue(model, new object[] { i });

                    Type item = listItem.GetType();
                    foreach (PropertyInfo propertyInfo in item.GetProperties())//遍历该类型下所有属性
                    {
                        if (propertyInfo.PropertyType == "".GetType())//如果属性为string类型
                        {
                            var inputString = (propertyInfo.GetValue(listItem) ?? "").ToString();
                            var sx = GetAntiXssSql(inputString);//进行字符串过滤

                            propertyInfo.SetValue(listItem, sx);//将过滤后的值设置给传入的对象
                        }
                        else if (propertyInfo.PropertyType == DateTime.Now.GetType() && propertyInfo.Name.ToLower() == "updatetime")
                        {
                            propertyInfo.SetValue(listItem, DateTime.Now);//以防万一给updatetime赋值
                        }
                    }
                }
                #endregion

            }
            else
            {
                #region class 反射
                foreach (PropertyInfo propertyInfo in t.GetProperties())//遍历该类型下所有属性
                {
                    if (propertyInfo.PropertyType == "".GetType())//如果属性为string类型
                    {
                        var inputString = (propertyInfo.GetValue(model) ?? "").ToString();
                        var sx = GetAntiXssSql(inputString);//进行字符串过滤

                        propertyInfo.SetValue(model, sx);//将过滤后的值设置给传入的对象
                    }
                    else if (propertyInfo.PropertyType == DateTime.Now.GetType() && propertyInfo.Name.ToLower() == "updatetime")
                    {
                        propertyInfo.SetValue(model, DateTime.Now);//以防万一给updatetime赋值
                    }

                }
                #endregion
            }


            return model;//返回安全对象
        }

        /// <summary>
        /// 获取更新sql
        /// </summary>
        /// <param name="type">需要更新的类型</param>
        /// <param name="primaryKey">需要更新的主键名</param>
        /// <returns></returns>
        public static string GetUpdateParamSql(Type type, string primaryKey, string asName = "")
        {

            var tableName = GetTableName(type);
            if (!string.IsNullOrEmpty(asName))
                tableName = asName;
            var properties = type.GetProperties();
            var fields = type.GetFields();
            var paramSql = $"update {tableName} set ";
            primaryKey = (primaryKey ?? "").ToLower();
            if (properties != null && properties.Length > 0)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var name = properties[i].Name;
                    if (primaryKey != (name.ToLower()))
                        paramSql += "[" + name + "]=@" + name + ",";
                }
            }
            return paramSql.TrimEnd(',') + string.Format(" where {0}=@{0}", primaryKey);
        }


        /// <summary>
        /// 获取insert子句 insert into table （xxx,yyy,zzz） values (@xxx,@yyy,@zzz)
        /// </summary>
        /// <param name="type">需要转sql的对象类型</param>
        /// <param name="primaryKey">自增主键的字段名称</param>
        /// <param name="isSelectPrimaryId">是否需要查询插入成功后的自增id</param>
        /// <returns></returns>
        public static string GetInsertParamSql(Type type, string primaryKey, bool isSelectPrimaryId = true, string asName = "")
        {
            var properties = type.GetProperties();
            var fields = type.GetFields();

            var tabName = GetTableName(type);
            if (!string.IsNullOrEmpty(asName))
                tabName = asName;
            var paramSql = $"insert into {tabName} ";
            var paramString = string.Empty;
            var fieldString = string.Empty;
            var allFieldsName = new List<string>();
            primaryKey = (primaryKey ?? "id").ToLower();
            if (properties != null && properties.Length > 0)
                for (int i = 0; i < properties.Length; i++)
                    if (primaryKey != (properties[i].Name.ToLower()))
                        allFieldsName.Add(properties[i].Name);

            allFieldsName.ForEach(x => { fieldString += $"[{x}],"; paramString += $"@{x},"; });
            paramSql += string.Format("({0}) values ({1});", fieldString.TrimEnd(','), paramString.TrimEnd(','));
            if (isSelectPrimaryId)
                paramSql += "select @@identity;";
            return GetAntiXssSql(paramSql);
        }

        /// <summary>
        /// 获取防JS攻击的sql
        /// </summary>
        /// <param name="inputString">输入的sql</param>
        /// <returns></returns>
        private static string GetAntiXssSql(string inputString)
        {
            return inputString;
            var sanitizer = new HtmlSanitizer();
            return sanitizer.Sanitize(inputString); //过滤js的标签
        }

        /// <summary>
        /// 获取分页SQL语句，默认row_number为关健字，所有表不允许使用该字段名
        /// </summary>
        /// <param name="_recordCount">记录总数</param>
        /// <param name="_pageSize">每页记录数</param>
        /// <param name="_pageIndex">当前页数</param>
        /// <param name="_safeSql">SQL查询语句</param>
        /// <param name="_orderField">排序字段，多个则用“,”隔开</param>
        /// <returns>分页SQL语句</returns>
        private static string CreatePagingSql(int _recordCount, int _pageSize, int _pageIndex, string _safeSql, string _orderField)
        {
            //计算总页数
            _pageSize = _pageSize == 0 ? _recordCount : _pageSize;
            int pageCount = (_recordCount + _pageSize - 1) / _pageSize;

            //检查当前页数
            if (_pageIndex < 1)
            {
                _pageIndex = 1;
            }
            else if (_pageIndex > pageCount)
            {
                _pageIndex = pageCount;
            }
            //拼接SQL字符串，加上ROW_NUMBER函数进行分页
            StringBuilder newSafeSql = new StringBuilder();
            newSafeSql.AppendFormat("SELECT ROW_NUMBER() OVER(ORDER BY {0}) as row_number,", _orderField);
            newSafeSql.Append(_safeSql.Substring(_safeSql.ToUpper().IndexOf("SELECT") + 6));

            //拼接成最终的SQL语句
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("SELECT * FROM (");
            sbSql.Append(newSafeSql.ToString());
            sbSql.Append(") AS T");
            sbSql.AppendFormat(" WHERE row_number between {0} and {1}", ((_pageIndex - 1) * _pageSize) + 1, _pageIndex * _pageSize);

            return sbSql.ToString() + " order by  row_number asc";
        }


        /// <summary>
        ///构建lastId的分页查询语句
        /// </summary>
        /// <param name="lastId"></param>
        /// <param name="_pageSize"></param>
        /// <param name="_safeSql"></param>
        /// <param name="_orderField"></param>
        /// <returns></returns>

        private static string CreatePagingSqlByLastId(int lastId, int _pageSize, string _safeSql, string _orderField)
        {



            //拼接SQL字符串，加上ROW_NUMBER函数进行分页
            StringBuilder newSafeSql = new StringBuilder();
            newSafeSql.AppendFormat("SELECT ROW_NUMBER() OVER(ORDER BY {0}) as rowId,", _orderField);
            newSafeSql.Append(_safeSql.Substring(_safeSql.ToUpper().IndexOf("SELECT") + 6));

            //拼接成最终的SQL语句
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append($"SELECT top {_pageSize} * FROM (");
            sbSql.Append(newSafeSql.ToString());
            sbSql.Append(") AS T");
            sbSql.AppendFormat(" WHERE rowId>{0}", lastId);

            return sbSql.ToString() + $" order by  {_orderField}";
        }


        /// <summary>
        /// 获取记录总数SQL语句
        /// </summary>
        /// <param name="_safeSql">SQL查询语句</param>
        /// <returns>记录总数SQL语句</returns>
        private static string CreateCountingSql(string _safeSql)
        {
            return string.Format(" SELECT COUNT(1) AS RecordCount FROM ({0}) AS T ", _safeSql);
        }
        private static string CreatePagingSql(int _pageSize, int _pageIndex, string _safeSql, string _orderField)
        {
            //拼接SQL字符串，加上ROW_NUMBER函数进行分页
            StringBuilder newSafeSql = new StringBuilder();
            newSafeSql.AppendFormat("SELECT ROW_NUMBER() OVER(ORDER BY {0}) as row_number,", _orderField);
            newSafeSql.Append(_safeSql.Substring(_safeSql.ToUpper().IndexOf("SELECT") + 6));

            //拼接成最终的SQL语句
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("SELECT * FROM (");
            sbSql.Append(newSafeSql.ToString());
            sbSql.Append(") AS T");
            sbSql.AppendFormat(" WHERE row_number between {0} and {1}", ((_pageIndex - 1) * _pageSize) + 1, _pageIndex * _pageSize);

            return sbSql.ToString() + " order by  row_number asc";
        }
        #endregion
        /// <summary>
        /// 异步获取分页结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conStr"></param>
        /// <param name="sql"></param>
        /// <param name="orderby"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <param name="param"></param>
        /// <param name="countSql"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static async Task<Common.ApiResults.PagedResult<T>> GetPagedResultAsync<T>(this string conStr, string sql, string orderby, int pagesize, int pageindex, object param = null, string countSql = null, CommandType commandType = CommandType.Text)
        {
            Config.CheckIsOnlyReadDB(conStr);
            try
            {
                var safeSql = GetAntiXssSql(sql);
                using MySqlConnection con = new MySqlConnection(conStr);
                countSql ??= CreateCountingSql(safeSql);
                var pagingSql = CreatePagingSql(pagesize, pageindex, safeSql, orderby);
                var reader = await con.QueryMultipleAsync($"{countSql};{pagingSql}", param.CheckWhereParam());
                var totalCount = await reader.ReadSingleOrDefaultAsync<int>();
                var items = await reader.ReadAsync<T>();
                return items.ToPagedResult(totalCount, pageindex, pagesize);
            }
            catch (Exception ex)
            {
                AddDbErrorLog(ex, MethodBase.GetCurrentMethod().Name, sql, param);
                return null;
            }
        }

    }
}
