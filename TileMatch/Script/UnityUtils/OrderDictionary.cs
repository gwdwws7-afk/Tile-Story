using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OrderDictionary<TKey, TValue>
{
	public List<TKey> Keys = new List<TKey>();
	public Dictionary<TKey, TValue> Values = new Dictionary<TKey, TValue>();

	public TValue this[TKey key]
	{
		get
		{
			return Values[key];
		}
		set
		{
			Values[key] = value;
		}
	}

	public OrderDictionary() { }

	public OrderDictionary(Dictionary<TKey, TValue> dict)
	{
		if (dict != null)
		{
			Keys = dict.Keys.ToList();
			Values = dict;
		}
	}
	
	public bool ContainsKey(TKey key)
	{
		return Keys.Contains(key);
	}
	public void Add(TKey key,TValue value)
	{
		if (Keys.Contains(key))
			throw new System.Exception($"OrderDictionary Contain Key:{key}");
		else
		{
			Keys.Add(key);
			Values.Add(key,value);
		}	
	}
	public void Remove(TKey key)
	{
		Keys.Remove(key);
		Values.Remove(key);
	}

	public void Clear()
	{
		Keys.Clear();
		Values.Clear();
	}

	public Dictionary<TKey,TValue> ToDictionary()
	{
		Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
		foreach (var key in Keys)
		{
			dict.Add(key,Values[key]);
		}
		return dict;
	}

	public TValue GetLastValue()
	{
		if (Keys != null && Keys.Count > 0)
		{
			return Values[Keys[Keys.Count - 1]];
		}
		return default(TValue);
	}

	public TKey GetLastKey()
	{
		if (Keys != null && Keys.Count > 0)
		{
			return Keys.Last();
		}
		return default(TKey);
	}
}
