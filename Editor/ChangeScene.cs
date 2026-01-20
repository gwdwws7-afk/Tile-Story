using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene
{
	public enum SceneType
	{
		menu,
		map,
		leveltype,
		game,
		editor,
	}

	static SceneType Type;

	[MenuItem("Scene/OpenMenu")]
	public static void OpenMenuScene()
	{
		EditorSceneManager.OpenScene(GetScenePath(SceneType.menu));
	}

	private static string GetScenePath(SceneType sceneType)
	{
		string sceneName = sceneType.ToString();
		string[] guids = AssetDatabase.FindAssets("t:Scene",new[] { "Assets"});

		foreach (var child in guids)
		{
			var filePath = AssetDatabase.GUIDToAssetPath(child);
			if (filePath.Contains(sceneName))
			{
				return filePath;
			}
		}
		throw new System.Exception($"{sceneName}:scenePath is Null!");
	}

	//[MenuItem("Scene/FindAllScripts")]
	public static void FindAllScripts()
	{
		EditorUtility.DisplayProgressBar("Progress", "Find Class...", 0);
		string[] dirs = { "Assets/Res", "Assets/RaccoonRescue" };
		var asstIds = AssetDatabase.FindAssets("t:Prefab", dirs);
		int count = 0;
		List<string> classList = new List<string>();
		for (int i = 0; i < asstIds.Length; i++)
		{
			string path = AssetDatabase.GUIDToAssetPath(asstIds[i]);
			var pfb = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			foreach (Transform item in pfb.transform)
			{
				var coms = item.GetComponentsInChildren<Component>();
				foreach (var com in coms)
				{
					if (com == null||com.GetType()==null) continue;
					string tName = com.GetType().FullName;
					if (!classList.Contains(tName) && (tName.StartsWith("UnityEngine") || tName.StartsWith("TMPro")))
					{
						classList.Add(tName);
					}
				}
			}
			count++;
			EditorUtility.DisplayProgressBar("Find Class", pfb.name, count / (float)asstIds.Length);
		}
		for (int i = 0; i < classList.Count; i++)
		{
			classList[i] = string.Format("<type fullname=\"{0}\" preserve=\"all\"/>", classList[i]);
		}
		System.IO.File.WriteAllLines(Application.dataPath + "/ClassTypes.txt", classList);
		EditorUtility.ClearProgressBar();
	}

	[MenuItem("Scene/FindAllShaders")]
	public static void FindAllShaders()
	{
		EditorUtility.DisplayProgressBar("Progress", "Find Shader...", 0);
		string[] dirs = { "Assets"};
		var asstIds = AssetDatabase.FindAssets("t:Material", dirs);
		int count = 0;
		List<string> classList = new List<string>();
		for (int i = 0; i < asstIds.Length; i++)
		{
			string path = AssetDatabase.GUIDToAssetPath(asstIds[i]);
			var pfb = AssetDatabase.LoadAssetAtPath<Material>(path);
			if (pfb.shader.name == "Particles/Standard Surface")
			{
				Debug.Log($"Shader Surface:{path}");
			}
			else if (pfb.shader.name == "Particles/Standard Unlit")
			{
				Debug.Log($"Shader Unlit:{path}");
			}
			else if (pfb.shader.name == "Legacy Shaders/Bumped Specular")
			{
				Debug.Log($"Legacy Shaders Bumped Specular:{path}");
			}
			else if (pfb.shader.name == "Legacy Shaders/Reflective/Bumped Specular")
			{
				Debug.Log($"Legacy Shaders Reflective Bumped Specular:{path}");
			}


			count++;
			EditorUtility.DisplayProgressBar("Find Class", pfb.name, count / (float)asstIds.Length);
		}
		EditorUtility.ClearProgressBar();
	}
}
