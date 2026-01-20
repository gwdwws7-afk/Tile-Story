using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MyFrameWork.Framework
{
    public static class CollectionHelper
    {
        #region List

        /// <summary>
        /// 获取列表
        /// - 并清除
        /// </summary>
        public static List<T> Load<T>(this List<T> list) where T : class
        {
            lock (((ICollection)list).SyncRoot)
            {
                var tempList = list.Where(i => i != null).ToList();

                list.Clear();

                return tempList;
            }
        }

        /// <summary>
        /// 初始化固定数量的list，以null填充
        /// </summary>
        public static List<int> Initialize(this List<int> list, int maxSlot)
        {
            list.Clear();

            for (var i = 0; i < maxSlot; i++)
                list.Add(0);

            return list;
        }

        public static List<T> Initialize<T>(this List<T> list, int maxSlot) where T : class
        {
            list.Clear();

            for (var i = 0; i < maxSlot; i++)
                list.Add(default(T));

            return list;
        }

        public static List<T> Merge<T>(this List<T> list, T data, Predicate<T> prec) where T : class
        {
            var index = list.FindIndex(prec);
            if (index < 0)
                list.Add(data);
            else
                list[index] = data;

            return list;
        }

        public static List<T> Filter<T>(this List<T> list)
        {
            lock (((ICollection)list).SyncRoot)
            {
                var tempList = list.Where(i => i != null).ToList();

                return tempList;
            }
        }

        public static bool Has<T>(this List<T> list, Predicate<T> pred)
        {
            return (list.FindIndex(pred) >= 0);
        }

        public static void Renew<T>(this List<T> list, T t)
        {
            if (!list.Has(d => d.Equals(t)))
                list.Add(t);
        }

        public static T Get<T>(this T[] list, int index)
        {
            if (list == null || list.Length == 0)
                return default(T);

            if (index >= list.Length || index < 0)
                return default(T);

            return list[index];
        }

        public static T Get<T>(this List<T> list, int index)
        {
            if (list == null || list.Count == 0)
                return default(T);

            if (index >= list.Count || index < 0)
                return default(T);

            return list[index];
        }

        public static void Set<T>(this List<T> list, int index, T value, T defaultValue = default(T), bool isUnique = false)
        {
            // 如果是唯一，则首先移除当前值
            if (isUnique)
            {
                var currentIndex = list.FindIndex(t => t.Equals(value));
                if (currentIndex >= 0)
                    list[currentIndex] = defaultValue;
            }

            if (index >= 0)
            {
                if (index >= list.Count)
                {
                    var count = list.Count;
                    for (var i = 0; i <= (index - count); i++)
                        list.Add(defaultValue);
                }

                list[index] = value;
            }
        }

        #endregion

        #region Dictionary

        public static void Renew<T, V>(this Dictionary<T, V> list, T t, V v)
        {
            if (list.ContainsKey(t))
                list[t] = v;
            else
                list.Add(t, v);
        }

        public static void Add<T, V>(this Dictionary<T, List<V>> list, T t, V v, bool isUniqueItem = false)
        {
            if (!list.ContainsKey(t))
                list.Add(t, new List<V>());

            if (isUniqueItem && list[t].Contains(v))
                return;

            list[t].Add(v);
        }

        public static void TryRemove<T, V>(this Dictionary<T, V> list, T t)
        {
            if (list.ContainsKey(t))
                list.Remove(t);
        }

        public static void Combine<T, V>(this Dictionary<T, V> dic, Dictionary<T, V> newDic)
        {
            foreach (var v in newDic)
            {
                dic.Add(v.Key, v.Value);
            }
        }

        public static void Remove<T, V>(this Dictionary<T, List<V>> list, T key, Predicate<V> prec)
        {
            if (!list.ContainsKey(key))
                return;

            list[key].RemoveAll(prec);

            if (list[key].Count == 0)
                list.Remove(key);
        }

        //public static void Add<T>(this Dictionary<int, T> table, T value)
        //    where T : ISource
        //{
        //    if (value.SourceID > 0)
        //        table.Add(value.SourceID, value);
        //}

        public static void Increase<T>(this Dictionary<T, int> table, T key, int value = 1)
        {
            if (table.ContainsKey(key))
                table[key] += value;
            else
                table.Add(key, value);
        }

        #endregion

        #region Queue

        public static void Enqueue<T>(this Queue<T> queue, T t, int maxSize)
        {
            queue.Enqueue(t);

            if (queue.Count > maxSize)
                queue.Dequeue();
        }

        public static List<T> TryGetRange<T>(this List<T> list, int index, int count)
        {
            if (list.Count <= index)
            {
                index = list.Count;
            }
            if (index + count > list.Count)
            {
                count = list.Count - index;
            }
            return list.GetRange(index, count);
        }

        #endregion

        #region Get(Dictionary)

        public static bool GetBool<T, V>(this Dictionary<T, V> dic, T t)
        {
            return Convert.ToBoolean(dic.Get(t));
        }

        public static int GetInt<T, V>(this Dictionary<T, V> dic, T t)
        {
            return Convert.ToInt32(dic.Get(t));
        }

        public static double GetDouble<T, V>(this Dictionary<T, V> dic, T t)
        {
            return Convert.ToDouble(dic.Get(t));
        }

        public static V Get<T, V>(this Dictionary<T, V> dic, T t, V defaultValue = default(V))
        {
            if (!dic.ContainsKey(t))
                return defaultValue;

            return dic[t];
        }

        #endregion

        #region Set(Dictionary)

        public static void Increase<T, V>(this Dictionary<T, object> dic, T t, int v)
        {
            var value = dic.GetInt(t);
            dic.Set(t, value + v);
        }

        public static void Set<T, V>(this Dictionary<T, V> dic, T t, V v)
        {
            if (dic.ContainsKey(t))
                dic[t] = v;
            else
                dic.Add(t, v);
        }

        #endregion
    }
}
