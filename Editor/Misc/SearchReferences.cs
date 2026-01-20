using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 搜寻资源引用
/// </summary>
public class SearchReferences : EditorWindow
{
    private static int selectedTab;
    string[] toolbarStrings = new string[] { "SearchReference", "SpriteReferenceInPrefab", "FolderSpriteReference", "DeleteUnusedSprites" };

    private static string pointGuid;
    private static Object singleSearchObject;
    private static Object singleReplaceObject;
    private static Object multipleSearchObject;

    private static Object searchImageObject;
    private static Object unifiedReplacementFolder;
    private static string searchObjectPaths;
    private static Vector2 searchObjectScrollViewVector;
    private static Object[] replaceObjects = new Object[200];
    private static List<string> searchPrefabsList = new List<string>();
    private static Dictionary<string, List<string>> spriteToPrefabDic = new Dictionary<string, List<string>>();

    private static Object searchFolderObject;
    private bool checkPrefab = true;
    private bool checkScene = true;
    private bool checkMaterial = true;
    private bool checkScriptableObject = true;
    private Vector2 searchFolderScrollViewVector;
    private static Vector2[] horizontalScrollViewVector = new Vector2[300];
    private static Object[] refereneceObjects = new Object[50];
    private static Dictionary<string, List<string>> objectNameToTextureDic = new Dictionary<string, List<string>>();

    private static Object[] searchFolders;
    private static int searchFoldersNum = 1;
    private List<string> unusedSpritesGuids;
    private bool checkAllPrefab;
    private string[] usedPrefabsGuids;

    [MenuItem("Window/SearchReferences")]
    static void Init()
    {
        SearchReferences searchReferencesWindow = (SearchReferences)EditorWindow.GetWindow(typeof(SearchReferences), false, "SearchReferences", true);
        searchReferencesWindow.Show();
        searchReferencesWindow.position = new Rect(640, 192, 800, 550);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        selectedTab = GUILayout.Toolbar(selectedTab, toolbarStrings, new GUILayoutOption[] { GUILayout.Width(700) });

        GUILayout.Space(10);

        if (selectedTab == 0)
        {
            GUIGUIDToAsset();

            GUILayout.Space(10);

            GUISearchSingleReference();

            GUILayout.Space(10);

            GUIReplaceSingleReference();

            GUILayout.Space(10);

            GUISearchMultipleReference();
        }
        else if (selectedTab == 1)
        {
            GUIPrefabReferenceSprite();

            GUILayout.Space(10);

            GUISpriteInPrefabItem();
        }
        else if (selectedTab == 2)
        {
            GUICheckFolderTexturesReference();

            GUILayout.Space(10);

            GUICheckTextureReferenceItem();
        }
        else if (selectedTab == 3)
        {
            GUIFoldersNeedSearch();
        }
    }

    private void GUIGUIDToAsset()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("GUID指向目标物体：", GUILayout.Width(150));
        pointGuid = GUILayout.TextField(pointGuid, GUILayout.Width(300));

        if (GUILayout.Button("Point", GUILayout.Width(100)))
        {
            string path = AssetDatabase.GUIDToAssetPath(pointGuid);
            Debug.Log("point to asset:" + path);
            var asset= AssetDatabase.LoadAssetAtPath(path, typeof(object));
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
        }

        GUILayout.EndHorizontal();
    }

    private void GUISearchSingleReference()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("查找单个资源的引用：", GUILayout.Width(150));
        singleSearchObject = EditorGUILayout.ObjectField(singleSearchObject, typeof(Object), true, GUILayout.Width(200));

        if (GUILayout.Button("Search", GUILayout.Width(100)))
        {
            string path = AssetDatabase.GetAssetPath(singleSearchObject);
            string guid = AssetDatabase.AssetPathToGUID(path);

            SearchSingleReference(guid);
        }

        GUILayout.EndHorizontal();
    }

    //替换单个资源引用
    private void GUIReplaceSingleReference()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("替换单个资源的引用：", GUILayout.Width(150));
        singleReplaceObject = EditorGUILayout.ObjectField(singleReplaceObject, typeof(Object), true, GUILayout.Width(200));

        if (GUILayout.Button("Replace", GUILayout.Width(100)))
        {
            string path = AssetDatabase.GetAssetPath(singleSearchObject);
            string guid = AssetDatabase.AssetPathToGUID(path);

            string replacePath = AssetDatabase.GetAssetPath(singleReplaceObject);
            string replaceGuid = AssetDatabase.AssetPathToGUID(replacePath);

            ReplaceSingleReference(guid, replaceGuid);

            EditorUtility.ClearProgressBar();
        }

        GUILayout.EndHorizontal();
    }

    private void SearchSingleReference(string guid)
    {
        List<string> referenceGuids = new List<string>();

        SearchAll(guid, referenceGuids, true);

        StringBuilder sb = new StringBuilder();

        bool isUsed = false;
        for (int i = 0; i < referenceGuids.Count; i++)
        {
            string resourcePath = AssetDatabase.GUIDToAssetPath(referenceGuids[i]);
            if (resourcePath.Contains("Resources") || resourcePath.Contains("map.unity") || resourcePath.Contains("game.unity") || resourcePath.Contains("menu.unity"))
            {
                isUsed = true;
                sb.Append(string.Format("<color=#ff0000>{0}</color>\n", resourcePath));
            }
            else
                sb.Append(resourcePath + "\n");
        }

        string name = Path.GetFileName(AssetDatabase.GUIDToAssetPath(guid));

        EditorUtility.ClearProgressBar();
        if (isUsed)
            Debug.Log(string.Format("<color=#ff0000>{0}被引用：\n</color>{1}\n{2}", name, guid.ToString(), sb.ToString()));
        else
            Debug.Log(string.Format("{0}被引用：\n{1}\n{2}", name, guid.ToString(), sb.ToString()));
    }

    private void ReplaceSingleReference(string guid,string replaceGuid)
    {
        List<string> list = new List<string>();
        SearchAll(guid, list, true, replaceGuid);
        AssetDatabase.Refresh();
    }

    private void GUISearchMultipleReference()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("查找多项资源的引用(文件夹)：", GUILayout.Width(150));

        //multipleSearchObject = EditorGUILayout.ObjectField(multipleSearchObject, typeof(DefaultAsset), true, GUILayout.Width(200));

        if (GUILayout.Button("Search", GUILayout.Width(100)))
        {
            //if (multipleSearchObject == null)
            //    return;

            for (int j = 0; j < searchFoldersNum; j++)
            {
                if (searchFolders[j] == null)
                    continue;

                string folderPath = AssetDatabase.GetAssetPath(searchFolders[j]);

                string filter = string.Empty;
                filter += "t:Texture ";
                //filter += "t:Scene ";
                //filter += "t:Material ";
                //filter += "t:ScriptableObject ";
                //filter += "t:SpriteAtlas";
                filter = filter.Trim();
                string[] resourceGuids = AssetDatabase.FindAssets(filter, new string[] { folderPath });

                for (int i = 0; i < resourceGuids.Length; i++)
                {
                    SearchSingleReference(resourceGuids[i]);
                }
            }
        }

        GUILayout.EndHorizontal();

        if (searchFolders == null)
        {
            searchFolders = new Object[searchFoldersNum];
        }
        else if (searchFoldersNum > searchFolders.Length)
        {
            Object[] temp = new Object[searchFoldersNum];
            for (int i = 0; i < searchFolders.Length; i++)
            {
                temp[i] = searchFolders[i];
            }
            searchFolders = temp;
        }

        for (int i = 0; i < searchFoldersNum; i++)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("Search Folder" + i.ToString() + ":", GUILayout.Width(180));

            searchFolders[i] = EditorGUILayout.ObjectField(searchFolders[i], typeof(DefaultAsset), true, GUILayout.Width(180));

            GUILayout.EndHorizontal();
        }
    }

    private void SearchAll(string guid, List<string> referenceGuids, bool showProgress = false, string replaceGuid = null)
    {
        string filter = string.Empty;
        filter += "t:Prefab ";
        filter += "t:Scene ";
        filter += "t:Material ";
        filter += "t:ScriptableObject ";
        //filter += "t:SpriteAtlas";
        filter = filter.Trim();

        string[] allResourceGuids = AssetDatabase.FindAssets(filter, new string[] { "Assets" });
        for (int i = 0; i < allResourceGuids.Length; i++)
        {
            if (referenceGuids.Contains(allResourceGuids[i]))
                continue;
            string resourcePath = AssetDatabase.GUIDToAssetPath(allResourceGuids[i]);
            if (showProgress)
            {
                bool cancel = EditorUtility.DisplayCancelableProgressBar("Searching", resourcePath, i * 1.0f / allResourceGuids.Length);
                if (cancel)
                    break;
            }
            string content = File.ReadAllText(resourcePath);
            if (!string.IsNullOrEmpty(guid) && content.Contains(guid)) 
            {
                if (replaceGuid != null)
                {
                    content = content.Replace(guid, replaceGuid);
                    File.WriteAllText(resourcePath, content);

                    string name1 = AssetDatabase.GUIDToAssetPath(guid);
                    string name2 = AssetDatabase.GUIDToAssetPath(replaceGuid);
                    Debug.Log($"替换{name1}为{name2}......");
                }
                else
                {
                    referenceGuids.Add(allResourceGuids[i]);
                    //SearchAll(allResourceGuids[i], referenceGuids);
                }
            }
        }
    }

    #region SpriteReferenceInPrefab
    private void GUIPrefabReferenceSprite()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("查找预制体内所有引用的图片：", GUILayout.Width(180));
        searchImageObject = EditorGUILayout.ObjectField(searchImageObject, typeof(Object), true, GUILayout.Width(200));
        if (GUILayout.Button("Search", GUILayout.Width(100)))
        {
            if (searchImageObject == null)
                return;
            StringBuilder sb = new StringBuilder();
            GameObject obj = searchImageObject as GameObject;
            Image[] images = obj.GetComponentsInChildren<Image>(true);
            Hashtable hashtable = new Hashtable();
            searchObjectPaths = null;
            System.Array.Clear(replaceObjects, 0, replaceObjects.Length);
            searchPrefabsList.Clear();
            spriteToPrefabDic.Clear();

            for (int i = 0; i < images.Length; i++)
            {
                Texture texture = images[i].mainTexture;
                if (texture != null)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    if (!hashtable.ContainsKey(path))
                    {
                        hashtable[path] = path;
                        sb.Append(path + "\n");
                    }
                }
            }

            SpriteRenderer[] spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                Sprite texture = spriteRenderers[i].sprite;
                if (texture != null)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    if (!hashtable.ContainsKey(path))
                    {
                        hashtable[path] = path;
                        sb.Append(path + "\n");
                    }
                }
            }

            searchObjectPaths = sb.ToString();

            Debug.Log(searchImageObject.name + "中包含的图片的引用(快速查找)：\n" + sb.ToString());
        }

        if (GUILayout.Button("DeepSearch", GUILayout.Width(100)))
        {
            if (searchImageObject == null)
                return;
            StringBuilder sb = new StringBuilder();
            StringBuilder logSb = new StringBuilder();
            searchObjectPaths = null;
            System.Array.Clear(replaceObjects, 0, replaceObjects.Length);
            searchPrefabsList.Clear();
            spriteToPrefabDic.Clear();

            string filePath = AssetDatabase.GetAssetPath(searchImageObject);
            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new string[] { "Assets" });
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });

            Queue<string> searchPrefabPathQueue = new Queue<string>();
            searchPrefabPathQueue.Enqueue(filePath);
            searchPrefabsList.Add(filePath);

            while (searchPrefabPathQueue.Count > 0)
            {
                string tempPath = searchPrefabPathQueue.Dequeue();
                string content = File.ReadAllText(tempPath);

                for (int i = 0; i < prefabGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                    EditorUtility.DisplayCancelableProgressBar("Prefab Checking", path, (float)i / prefabGuids.Length);

                    if (!searchPrefabsList.Contains(path) && content.Contains(prefabGuids[i]))
                    {
                        searchPrefabsList.Add(path);
                        searchPrefabPathQueue.Enqueue(path);
                    }
                }
            }

            for (int i = 0; i < searchPrefabsList.Count; i++)
            {
                string searchContent = File.ReadAllText(searchPrefabsList[i]);
                for (int j = 0; j < spriteGuids.Length; j++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(spriteGuids[j]);
                    EditorUtility.DisplayCancelableProgressBar("Sprite Checking", path, (float)j / spriteGuids.Length);

                    if ((!spriteToPrefabDic.ContainsKey(path) || !spriteToPrefabDic[path].Contains(searchPrefabsList[i])) && searchContent.Contains(spriteGuids[j]))
                    {
                        if (!spriteToPrefabDic.ContainsKey(path))
                        {
                            spriteToPrefabDic[path] = new List<string>();
                            spriteToPrefabDic[path].Add(searchPrefabsList[i]);
                            sb.Append(path + "\n");
                        }
                        else
                        {
                            spriteToPrefabDic[path].Add(searchPrefabsList[i]);
                        }
                        logSb.Append(path + "\n" + "\t from:" + searchPrefabsList[i] + "\n");
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            searchObjectPaths = sb.ToString();

            Debug.Log(searchImageObject.name + "中包含的图片的引用(深度查找)：\n" + logSb.ToString());
        }

        if (!string.IsNullOrEmpty(searchObjectPaths))
        {
            GUILayout.Label("同名图片统一替换(文件夹)：", GUILayout.Width(150));

            unifiedReplacementFolder = EditorGUILayout.ObjectField(unifiedReplacementFolder, typeof(DefaultAsset), true, GUILayout.Width(150));

            if (unifiedReplacementFolder != null)
            {
                if (GUILayout.Button("替换", GUILayout.Width(100)))
                {
                    if (unifiedReplacementFolder == null)
                        return;
                    string folderPath = AssetDatabase.GetAssetPath(unifiedReplacementFolder);
                    string[] textureGuids = AssetDatabase.FindAssets("t:Texture", new string[] { folderPath });

                    string[] paths = searchObjectPaths.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < textureGuids.Length; i++)
                    {
                        string texturePath = AssetDatabase.GUIDToAssetPath(textureGuids[i]);
                        string textureName = Path.GetFileName(texturePath);

                        for (int j = 0; j < paths.Length; j++)
                        {
                            string beReplaceTextureName = Path.GetFileName(paths[j]);
                            if (textureName == beReplaceTextureName)
                            {
                                string beReplaceTextureGuid = AssetDatabase.AssetPathToGUID(paths[j]);
                                ReplaceSpriteReference(beReplaceTextureGuid, textureGuids[i]);
                            }
                        }
                    }

                    AssetDatabase.Refresh();
                }
            }
        }

        GUILayout.EndHorizontal();
    }

    //搜索出的结果
    private void GUISpriteInPrefabItem()
    {
        if (searchObjectPaths == null)
        {
            searchObjectScrollViewVector = Vector2.zero;
            return;
        }

        string[] paths = searchObjectPaths.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        if (paths.Length < 1)
            return;

        searchObjectScrollViewVector = GUILayout.BeginScrollView(searchObjectScrollViewVector);

        for (int i = 0; i < paths.Length; i++)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("point", GUILayout.Width(40)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Sprite>(paths[i]);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                }
            }

            GUILayout.Label(paths[i]);

            replaceObjects[i] = EditorGUILayout.ObjectField(replaceObjects[i], typeof(Object), true, GUILayout.Width(150));

            if (replaceObjects[i] != null && searchImageObject != null)
            {
                if (GUILayout.Button("替换", GUILayout.Width(100)))
                {
                    string replaceObjectPath = AssetDatabase.GetAssetPath(replaceObjects[i]);
                    string beReplacedObjectGUID = AssetDatabase.AssetPathToGUID(paths[i]);
                    string replaceObjectGUID = AssetDatabase.AssetPathToGUID(replaceObjectPath);

                    ReplaceSpriteReference(beReplacedObjectGUID, replaceObjectGUID);

                    AssetDatabase.Refresh();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        GUILayout.EndScrollView();
    }

    private void ReplaceSpriteReference(string beReplacedObjectGUID, string replaceObjectGUID)
    {
        string beReplacedObjectPath = AssetDatabase.GUIDToAssetPath(beReplacedObjectGUID);
        string replaceObjectPath = AssetDatabase.GUIDToAssetPath(replaceObjectGUID);

        string prefabContent = string.Empty;
        if (spriteToPrefabDic.TryGetValue(beReplacedObjectPath, out List<string> prefabList))
        {
            string name1 = string.Empty;

            foreach (var prefab in prefabList)
            {
                prefabContent += File.ReadAllText(prefab);
                string temp = name1 == string.Empty ? Path.GetFileName(prefab) : "和" + Path.GetFileName(prefab);
                name1 += temp;
            }

            string name2 = Path.GetFileName(beReplacedObjectPath);
            Debug.Log($"替换{name1}中的{name2}......");
        }
        else
        {
            Debug.LogError("字典中不存在此图片引用，建议重新search");
            return;
        }

        if (prefabContent.Contains(beReplacedObjectGUID))
        {
            for (int j = 0; j < prefabList.Count; j++)
            {
                string finishContent = File.ReadAllText(prefabList[j]);
                finishContent = finishContent.Replace(beReplacedObjectGUID, replaceObjectGUID);
                File.WriteAllText(prefabList[j], finishContent);
            }

            searchObjectPaths = searchObjectPaths.Replace(beReplacedObjectPath, replaceObjectPath);
            Debug.Log("替换成功");
        }
        else
        {
            Debug.LogError("目标物体中未包含目标引用！");
        }
    }
    #endregion

    #region FolderSpriteReference
    private void GUICheckFolderTexturesReference()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("查找文件夹内所有图片的引用：", GUILayout.Width(180));

        searchFolderObject = EditorGUILayout.ObjectField(searchFolderObject, typeof(DefaultAsset), true, GUILayout.Width(200));

        if (GUILayout.Button("Search", GUILayout.Width(100)))
        {
            if (searchFolderObject == null)
                return;
            List<string> referenceTextureList = new List<string>();
            objectNameToTextureDic.Clear();
            System.Array.Clear(refereneceObjects, 0, refereneceObjects.Length);

            string folderPath = AssetDatabase.GetAssetPath(searchFolderObject);

            //获取文件夹内全部图片文件
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture", new string[] { folderPath });

            string filter = string.Empty;
            if (checkPrefab)
                filter += "t:Prefab ";
            if (checkScene)
                filter += "t:Scene ";
            if (checkMaterial)
                filter += "t:Material ";
            if (checkScriptableObject)
                filter += "t:ScriptableObject ";
            filter = filter.Trim();

            if (!string.IsNullOrEmpty(filter))
            {
                string[] searchGuids = AssetDatabase.FindAssets(filter, new string[] { "Assets" });
                for (int i = 0; i < searchGuids.Length; i++)
                {
                    string filePath = AssetDatabase.GUIDToAssetPath(searchGuids[i]);
                    bool cancel = EditorUtility.DisplayCancelableProgressBar("Searching", filePath, i * 1.0f / searchGuids.Length);
                    if (cancel)
                        break;

                    try
                    {
                        string fileGuid = searchGuids[i];
                        string content = File.ReadAllText(filePath);
                        for (int j = 0; j < textureGuids.Length; j++)
                        {
                            if (content.Contains(textureGuids[j]))
                            {
                                if (objectNameToTextureDic.ContainsKey(fileGuid))
                                {
                                    objectNameToTextureDic[fileGuid].Add(textureGuids[j]);
                                }
                                else
                                {
                                    objectNameToTextureDic[fileGuid] = new List<string>();
                                    objectNameToTextureDic[fileGuid].Add(textureGuids[j]);
                                }

                                if (!referenceTextureList.Contains(textureGuids[j]))
                                    referenceTextureList.Add(textureGuids[j]);
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(filePath + "\n" + e.Message);
                    }
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < textureGuids.Length; i++)
                {
                    if (!referenceTextureList.Contains(textureGuids[i]))
                    {
                        string path = AssetDatabase.GUIDToAssetPath(textureGuids[i]);
                        sb.Append(path + "\n");
                    }
                }
                Debug.Log("没有任何引用的图片：\n" + sb.ToString());

                EditorUtility.ClearProgressBar();
            }
        }
        GUILayout.EndHorizontal();
    }

    private void GUICheckTextureReferenceItem()
    {
        if (objectNameToTextureDic.Count == 0)
            return;

        searchFolderScrollViewVector = GUILayout.BeginScrollView(searchFolderScrollViewVector, GUILayout.Width(position.width));

        int count = 0;
        foreach (var item in objectNameToTextureDic)
        {
            string objectPath = AssetDatabase.GUIDToAssetPath(item.Key);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("point", GUILayout.Width(40)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(objectPath);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                }
            }

            string objectName = Path.GetFileName(objectPath);
            GUILayout.Label(objectName, GUILayout.Width(220));

            if (item.Value != null)
            {
                count++;
                horizontalScrollViewVector[count] = GUILayout.BeginScrollView(horizontalScrollViewVector[count], GUILayout.Height(55));
                List<string> textureGuids = item.Value;
                foreach (var guid in textureGuids)
                {
                    string textureName = Path.GetFileName(AssetDatabase.GUIDToAssetPath(guid));
                    GUILayout.Label("[" + textureName + "]");
                }
                GUILayout.EndScrollView();
                GUILayout.Space(10);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
    #endregion

    private void GUIFoldersNeedSearch()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("查找文件夹中的无引用资源：", GUILayout.Width(180));

        searchFoldersNum = int.Parse(GUILayout.TextField(searchFoldersNum.ToString(), GUILayout.Width(100)));
        if (searchFoldersNum <= 0)
            return;

        if (GUILayout.Button("Search", GUILayout.Width(100)))
        {
            unusedSpritesGuids = null;
            GUISearchFoldersUnusedSprite();
        }

        if (GUILayout.Button("Clear", GUILayout.Width(100)))
        {
            for (int i = 0; i < searchFolders.Length; i++)
            {
                searchFolders[i] = null;
            }
            usedPrefabsGuids = null;
        }

        if (unusedSpritesGuids != null)
        {
            if (GUILayout.Button("Delete All", GUILayout.Width(100)))
            {
                for (int i = 0; i < unusedSpritesGuids.Count; i++)
                {
                    if (string.IsNullOrEmpty(unusedSpritesGuids[i]))
                        continue;

                    string path = AssetDatabase.GUIDToAssetPath(unusedSpritesGuids[i]);
                    string assetName = Path.GetFileName(path);

                    if (AssetDatabase.DeleteAsset(path))
                    {
                        Debug.Log("删除" + assetName + "成功");
                        unusedSpritesGuids[i] = string.Empty;
                    }
                }
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (searchFolders == null)
        {
            searchFolders = new Object[searchFoldersNum];
        }
        else if (searchFoldersNum > searchFolders.Length) 
        {
            Object[] temp = new Object[searchFoldersNum];
            for (int i = 0; i < searchFolders.Length; i++)
            {
                temp[i] = searchFolders[i];
            }
            searchFolders = temp;
        }

        for (int i = 0; i < searchFoldersNum; i++)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("Search Folder" + i.ToString() + ":", GUILayout.Width(180));

            searchFolders[i] = EditorGUILayout.ObjectField(searchFolders[i], typeof(DefaultAsset), true, GUILayout.Width(180));

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        GUIUnusedSpriteItem();
    }

    private void GUISearchFoldersUnusedSprite()
    {
        List<string> searchPaths = new List<string>();

        for (int i = 0; i < searchFolders.Length; i++)
        {
            if (searchFolders[i] == null)
                continue;
            string folderPath = AssetDatabase.GetAssetPath(searchFolders[i]);

            if(!searchPaths.Contains(folderPath))
                searchPaths.Add(folderPath);
        }

        //获取文件夹内全部搜索资源的GUID
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture", searchPaths.ToArray());

        string filter = string.Empty;
        if (checkPrefab)
            filter += "t:Prefab ";
        if (checkScene)
            filter += "t:Scene ";
        if (checkMaterial)
            filter += "t:Material ";
        if (checkScriptableObject)
            filter += "t:ScriptableObject ";
        filter = filter.Trim();

        if (!string.IsNullOrEmpty(filter))
        {
            List<string> usedSpritesGuids = new List<string>();
            string[] searchGuids = AssetDatabase.FindAssets(filter, new string[] { "Assets" });

            for (int i = 0; i < searchGuids.Length; i++)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(searchGuids[i]);
                bool cancel = EditorUtility.DisplayCancelableProgressBar("Searching", filePath, i * 1.0f / searchGuids.Length);
                if (cancel)
                    break;

                try
                {
                    string fileGuid = searchGuids[i];
                    string content = File.ReadAllText(filePath);
                    for (int j = 0; j < textureGuids.Length; j++)
                    {
                        if (content.Contains(textureGuids[j]))
                        {
                            if (!usedSpritesGuids.Contains(textureGuids[j]))
                                usedSpritesGuids.Add(textureGuids[j]);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError(filePath + "\n" + e.Message);
                }
            }

            List<string> guids = new List<string>();
            for (int i = 0; i < textureGuids.Length; i++)
            {
                if (!usedSpritesGuids.Contains(textureGuids[i]))
                {
                    guids.Add(textureGuids[i]);
                }
            }
            unusedSpritesGuids = guids;
            EditorUtility.ClearProgressBar();
        }
    }

    private void GUIUnusedSpriteItem()
    {
        if (unusedSpritesGuids == null)
            return;

        searchObjectScrollViewVector = GUILayout.BeginScrollView(searchObjectScrollViewVector, GUILayout.MinWidth(position.width), GUILayout.Height(200));

        for (int i = 0; i < unusedSpritesGuids.Count; i++)
        {
            if (string.IsNullOrEmpty(unusedSpritesGuids[i]))
                continue;

            GUILayout.BeginHorizontal();

            string path = AssetDatabase.GUIDToAssetPath(unusedSpritesGuids[i]);

            if (GUILayout.Button("point", GUILayout.Width(40)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                }
            }

            string assetName = Path.GetFileName(path);
            GUILayout.Label(assetName, GUILayout.Width(150));

            if (GUILayout.Button("delete", GUILayout.Width(80)))
            {
                if (AssetDatabase.DeleteAsset(path))
                {
                    Debug.Log("删除" + assetName + "成功");
                    unusedSpritesGuids[i] = string.Empty;
                }
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        AssetDatabase.Refresh();
    }

    //获取有引用的预制体的guid(不准确，弃用)
    private string[] GetUsedPrefabsGuids()
    {
        List<string> usedPrefabGuids = new List<string>();

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });

        string filter = string.Empty;
        //filter += "t:Prefab ";
        filter += "t:Scene ";
        filter += "t:ScriptableObject ";
        filter = filter.Trim();

        string[] searchGuids = AssetDatabase.FindAssets(filter, new string[] { "Assets" });
        for (int i = 0; i < searchGuids.Length; i++)
        {
            bool cancel = EditorUtility.DisplayCancelableProgressBar("GetUsedPrefabsGuids", searchGuids[i], i * 1.0f / searchGuids.Length);
            if (cancel)
                break;

            string searchPath = AssetDatabase.GUIDToAssetPath(searchGuids[i]);
            string searchContent = File.ReadAllText(searchPath);

            for (int j = 0; j < prefabGuids.Length; j++)
            {
                if (searchGuids[i] == prefabGuids[j] || usedPrefabGuids.Contains(prefabGuids[j])) 
                    continue;
                if (searchContent.Contains(prefabGuids[j]))
                {
                    if (!usedPrefabGuids.Contains(prefabGuids[j]))
                    {
                        usedPrefabGuids.Add(prefabGuids[j]);
                    }
                }
            }
        }
        EditorUtility.ClearProgressBar();

        return usedPrefabGuids.ToArray();
    }
}
