using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class FontEditor : EditorWindow
{
    [MenuItem("Tools/字体管理(TextMeshPro)")]
    public static void Open()
    {
        GetWindow<FontEditor>("字体管理");
    }
    MaterialPresetName nowType;
    private Material nowMaterial;
    private TMPro.TMP_FontAsset fontBase;
    string fontBaseDic;
    Color useColor;
    private void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("本身样式：");
        fontBase = (TMPro.TMP_FontAsset)EditorGUILayout.ObjectField(fontBase, typeof(TMPro.TMP_FontAsset), true);
        if (fontBase != null)
        {
            var path = AssetDatabase.GetAssetPath(fontBase);
            var result = AssetDatabase.LoadMainAssetAtPath(path);

            if (result is TMPro.TMP_FontAsset)
            {
                //Debug.LogError("LoadComplete");
                int lastindex = path.LastIndexOf(fontBase.name);
                fontBaseDic = path.Substring(0, lastindex);
                //Debug.LogError(path);
                //Debug.LogError(fontBaseDic);
            }
            //path = path.Replace("Assets", Application.dataPath);
            //Debug.LogError(path);
            //FileInfo info = new FileInfo(path);
            //if (info.Exists)
            //{
            //    Debug.LogError("Get");
            //}


        }
        GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("字体名称：");
        //fontName = EditorGUILayout.TextField(fontName);
        //GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"当前Type：{nowType}");

        if (GUILayout.Button("选择字体Type"))
        {
            GenericMenu menu = new GenericMenu();
            foreach (MaterialPresetName item in Enum.GetValues(typeof(MaterialPresetName)))
            {
                MaterialPresetName lastType = item;
                menu.AddItem(new GUIContent(item.ToString()), false, () => SetTargetOnClick(lastType));
            }
            menu.ShowAsContext();
        }
        GUILayout.EndHorizontal();


        if (nowMaterial != null)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(nowMaterial, typeof(Material), true);


            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"当前描边颜色:");
            Color lastColor = useColor;
            useColor = EditorGUILayout.ColorField(useColor);
            if (lastColor != useColor)
            {
                nowMaterial.SetColor("_OutlineColor",useColor);
                nowMaterial.SetColor("_UnderlayColor", useColor);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("刷新创建所有Enum相关Material"))
        {
            foreach (MaterialPresetName item in Enum.GetValues(typeof(MaterialPresetName)))
            {
                SetTargetOnClick(item);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    void SetTargetOnClick(MaterialPresetName type)
    {
        if (type == MaterialPresetName.None) return;
        nowType = type;
        string fontPath = fontBaseDic + fontBase.name + " - " + type.ToString() +
        ".mat";
        var result = AssetDatabase.LoadMainAssetAtPath(fontPath);
        if (result == null)
        {
            Material mater = new Material(Shader.Find("TextMeshPro/Mobile/Distance Field"));
            mater.mainTexture = fontBase.material.mainTexture;
            AssetDatabase.CreateAsset(mater, fontPath);
            nowMaterial = mater;
        }
        else
        {
            nowMaterial = result as Material;
        }
        useColor = nowMaterial.GetColor("_OutlineColor");
    }
}
