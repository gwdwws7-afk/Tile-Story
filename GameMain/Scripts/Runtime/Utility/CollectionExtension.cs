using System;
using System.Collections.Generic;

/// <summary>
/// 容器扩展类
/// </summary>
public static class CollectionExtension
{
    /// <summary>
    /// 随机打乱数组
    /// </summary>
    /// <typeparam name="T">容器存储类型</typeparam>
    /// <param name="list">容器</param>
    /// <returns>打乱后的容器</returns>
    public static List<T> Shuffle<T>(this List<T> list)
    {
        Random randomNum = new Random();
        int index = 0;
        T temp;
        for (int i = 0; i < list.Count; i++)
        {
            index = randomNum.Next(0, list.Count - 1);
            if (index != i)
            {
                temp = list[i];
                list[i] = list[index];
                list[index] = temp;
            }
        }
        return list;
    }
}
