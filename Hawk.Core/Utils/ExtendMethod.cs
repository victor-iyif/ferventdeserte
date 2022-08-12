﻿using System;
using Hawk.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hawk.Core.Connectors;
using Hawk.Core.Utils.Logs;
using Hawk.Core.Utils.Plugins;
using NPOI.SS.Formula.Functions;

namespace Hawk.Core.Utils
{
    public class DisposeHelper : IDisposable
    {
        private readonly Action action;

        public DisposeHelper(Action action2)
        {
            action = action2;
        }

        public void Dispose()
        {
            action();
        }
    }

    public class ScriptHelper
    {
        public static FreeDocument ToDocument(dynamic obj)

        {
            var free = new FreeDocument();
            if (obj is IEnumerable)
            {
                foreach (var item in obj)
                {
                    free.Add(item.Key, item.Value);
                }
                return free;
            }
            return free;
        }

        public static List<FreeDocument> ToDocuments(dynamic obj)

        {
            var documents = new List<FreeDocument>();
            if (obj is IEnumerable)
            {
                foreach (var value in obj)
                {
                    var free = new FreeDocument();
                    if (value is IEnumerable)
                    {
                        foreach (var item in value)
                        {
                            if (item is string)
                            {
                                free.Add(item, value[item]);
                            }
                            else
                                free.Add(item.Key, item.Value);
                        }
                    }

                    documents.Add(free);
                }
            }

            return documents;
        }
        public static String RemoveSpecialCharacter(String hexData)
        {
            return Regex.Replace(hexData, "[ \\[ \\] \\^ \\-*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "").ToUpper();
        }
    }


    public static class ExtendMethods
    {
        /// <summary>
        ///     添加一个新实例
        /// </summary>
        /// <param name="value">要添加的类型</param>
        public static T Add<T>(this ICollection<T> collection, Type value)
        {
            var instance = (T) Activator.CreateInstance(value);
            collection.Add(instance);
            return instance;
        }

       

        /// <summary>
        ///     添加一个新实例
        /// </summary>
        /// <param name="value">要添加的类型</param>
        /// <param name="args">自动添加的数据参数 </param>
        public static void Add<T>(this ICollection<T> collection, Type value, params object[] args)
        {
            var instance = (T) Activator.CreateInstance(value, args);
            collection.Add(instance);
        }

      
        /// <summary>
        ///     添加一个新实例
        /// </summary>
        /// <param name="value">要添加的类型</param>
        public static void Add<T>(this ICollection<T> collection, string name) where T : class
        {
            var item = PluginProvider.GetObjectInstance<T>(name);
            if (item == null)
                return;
            collection.Add(item);
        }

        /// <summary>
        ///     添加新实例
        /// </summary>
        /// <param name="index"></param>
        public static void Add<T>(this ICollection<T> collection, int index)
        {
            T instance;
            try
            {
                instance = (T) PluginProvider.GetObjectInstance(typeof (T), index);
            }
            catch (Exception ex)
            {
                XLogSys.Print.Warn(ex);
                throw;
            }

            if (instance != null)
            {
                // instance.Init();

                collection.Add(instance);
            }
        }

        /// <summary>
        ///     在集合中获取一个实例，若无该实例，将搜索插件，并自动添加之
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isAddToList">是否要加入列表，被托管 </param>
        /// <returns></returns>
        public static T Get<T>(this ICollection<T> collection, string name, bool? isAddToList = true)
            where T : class, IProcess
        {
            var process = collection.FirstOrDefault(d => name == d.GetType().Name);
            if (process != null) return process;
            var newProcess =
                PluginProvider.GetPluginCollection(typeof (T)).FirstOrDefault(d => d.Name == name);
            if (newProcess == null)
            {
                // throw new Exception(string.Format("要获取的插件{0}无法在插件集合中找到"));
                return null;
            }

            if (isAddToList == true)
            {
                collection.Add(newProcess.MyType);

                var newone = collection.FirstOrDefault(d => name == d.GetType().Name);

                return newone;
            }
            var plugin = PluginProvider.GetObjectInstance(newProcess.MyType) as T;
            return plugin;
        }

        public static IEnumerable<IFreeDocument> InserDataCollection(this IDataBaseConnector connector, DataCollection collection,
            string tableName = null, int batchMount = 1000)
        {
           return collection.ComputeData.BatchDo(data =>
           {
               if (tableName == null)
                   tableName = collection.Name;
               List<string> columns =new List<string>();
               var sample = data.MergeToDocument();
               columns = data.GetKeys().ToList();
               if (connector.RefreshTableNames().FirstOrDefault(d => d.Name == tableName) == null)
               {
                   var result=connector.CreateTable(sample, tableName);
                   if(result==false)
                       throw new Exception(String.Format(GlobalHelper.Get("key_349"), tableName));
                   // connector.RefreshTableNames();
               }
               return columns;

           },(list, columns) =>
           {
               connector.BatchInsert(list,(List<string>)columns , collection.Name);
           });
         
          
          
        }

        public static IEnumerable<FreeDocument> CacheDo(this IEnumerable<FreeDocument> documents,IList<FreeDocument> cache=null,int maxCount=100 )
        {
            if (cache == null||cache.Count==0)
            {

                foreach (var document in documents)
                {
                    if(cache?.Count<maxCount)
                        cache.Add(document.Clone());
                    yield return document;
                }
                yield break;
            }
            else
            {
                foreach (var item in cache)
                {
                    yield return item;
                }
                yield break;
            
            }

        }

       

        public static IEnumerable<T> BatchDo<T>(this IEnumerable<T> documents,
            Func<List<T>,object> initFunc, Action<List<T>,object > batchFunc, Action endFunc = null, int batchMount = 100) 
        {
            
            var i = 0;
            var initMount = batchMount/2;
            if (initMount <= 0)
                initMount = 1;
            bool isInit = false;
            var list = new List<T>();
            object result = null;
            foreach (var document in documents)
            {
                if(document==null)
                    continue;

// list.Add(document.Clone());
                list.Add(document);
                if (!isInit )
                {
                    if (list.Count == initMount)
                    {
                        try
                        {
                            result = initFunc(list);
                        }
                        catch (Exception ex)
                        {
                            XLogSys.Print.Error("batch execute failed "+ ex.Message);
                            yield break;                            
                        }
                      
                        isInit = true;
                    }
                    else
                    {
                        yield return document;
                        continue;
                    }
                }
                if (list.Count == batchMount)
                {
                    try
                    {
                        batchFunc(list, result);
                    }
                    catch (Exception ex)
                    {
                        XLogSys.Print.Warn(GlobalHelper.Get("key_111")+ ex.Message);
                    }

                    list = new List<T>();
                }
                yield return document;
                i++;
            }
            if (isInit == false)
            {
                try
                {
                    result = initFunc(list);
                }
                catch (Exception ex)
                {
                    XLogSys.Print.Error("batch execute failed " + ex.Message);
                    yield break;
                }

            }
            if (list.Count != 0)
            {
                batchFunc(list, result);
            }
            endFunc?.Invoke();
        }
    }
}