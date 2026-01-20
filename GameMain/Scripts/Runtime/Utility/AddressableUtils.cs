using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class AddressableUtils
{
	//addressable 同步加载
	public static T LoadAsset<T>(string key)
	{
		try
		{
			var async = Addressables.LoadAssetAsync<T>(key);
			var obj = async.WaitForCompletion();
			return obj;
		}
		catch (System.Exception e)
		{
			return default(T);
		}
	}

	public static void ReleaseAsset<T>(T obj)
	{
		Addressables.Release<T>(obj);
    }

	public static bool IsHaveAsset(string name)
	{
		return IsHaveAssetSync(name, out long size);
	}

	private static readonly Dictionary<string, bool> IsHaveAssetDict = new Dictionary<string, bool>();

	public static bool IsHaveAssetSync<T>(string name)
	{
		if (IsHaveAssetDict.TryGetValue(name, out bool result))
			return result;
		
		Type type = typeof(T);

		bool isResourceExist = false;
		if (!string.IsNullOrEmpty(name))
		{
			foreach (var locator in Addressables.ResourceLocators)
			{
				IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> locations;
				if (locator.Locate(name, type, out locations))
				{
					isResourceExist = true;
					break;
				}
			}
		}
		
		if (isResourceExist)
		{
			return IsHaveAssetSync(name, out long size);
		}
		else
		{
			return false;
		}
	}
	
	public static bool IsHaveAssetSync(string name, out long size)
    {
		size = 0;
		if (IsHaveAssetDict.TryGetValue(name, out bool result))
			return result;
		
		if (name.Equals("CardSetMainMenu-1"))
			return false;
		
		bool isDownloading = GameManager.Download.IsDownloading(name);
		if (!isDownloading)
		{
			var async = Addressables.GetDownloadSizeAsync(name);
			async.WaitForCompletion();
			size = async.Result;
			Addressables.Release(async);
			
			if (size <= 0)
				IsHaveAssetDict.Add(name, true);
			
			return size <= 0;
		}
		else
		{
			return false;
		}
	}

	public static void IsHaveAssetAsync(string name, Action<bool> resultAction)
	{
		GameManager.Download.GetDownloadSize(name, (size) =>
		{
			resultAction?.InvokeSafely(size <= 0);
		}, (fail) =>
		{
			Log.Info($"IsHaveAssetAsync:{fail}");
			resultAction?.InvokeSafely(true);
		});
	}

	public static Dictionary<string, long> GetNeedDownLoadAssetSize(List<string> downloadNames)
	{
		Dictionary<string, long> dict = new Dictionary<string, long>();
		while (downloadNames.Count > 0)
		{
			if (!IsHaveAssetSync(downloadNames[0], out long size))
			{
				dict.Add(downloadNames[0], size);
			}
			downloadNames.RemoveAt(0);
		}
		return dict;
	}
}
