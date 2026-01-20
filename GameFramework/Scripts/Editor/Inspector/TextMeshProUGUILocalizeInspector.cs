using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(TextMeshProUGUILocalize))]
[CanEditMultipleObjects]
public class TextMeshProUGUILocalizeInspector : Editor
{
    TextMeshProUGUILocalize m_Localize;
    SerializedProperty mTerm;
	SerializedProperty mMaterialName;
	private SerializedProperty isTextOutlineCover;
	
	LanguageSourceData languageSourceData;

	private ReorderableList m_ParamsList;
	
	List<string> mTermsArray;
	static Language currentLanguage = Language.English;
	string filter;
	public string[] ableTerms
    {
        get
        {
			//增加过滤
			if (string.IsNullOrEmpty(filter)) return mTermsArray.ToArray();
			return mTermsArray.FindAll((s => s.Contains(filter))).ToArray();
		}
    }

	private void OnEnable()
    {
        m_Localize = (TextMeshProUGUILocalize)target;

        mTerm = serializedObject.FindProperty("m_Term");
		mMaterialName = serializedObject.FindProperty("m_MaterialName");
		isTextOutlineCover = serializedObject.FindProperty("isTextOutlineCover");

		LanguageSourceAsset languageAsset = Resources.Load<LanguageSourceAsset>("DefaultLanguageData");
		languageSourceData = languageAsset.SourceData;

		languageSourceData.UpdateDictionary(true);

		mTermsArray = languageSourceData.GetTermsList();

		m_ParamsList = getList(serializedObject);
	}

    public override void OnInspectorGUI()
    {
		serializedObject.UpdateIfRequiredOrScript();

		Undo.RecordObject(target, "Localize");

        if (GUILayout.Button("Localize", GUIStyle_Header))
        {

        }
        GUILayout.Space(10);
		filter = GUILayout.TextField(filter);

		GUILayout.Space(10);
		OnGUI_Terms();

		GUILayout.Space(10);
		OnGUI_Language();

		GUILayout.Space(10);
		OnGUI_MaterialPreset();

		GUILayout.Space(10);
		bool bestFitWidth = EditorGUILayout.Toggle("Best Fit Width", m_Localize.m_BestFitWidth);
		if (bestFitWidth != m_Localize.m_BestFitWidth)
		{
			m_Localize.m_BestFitWidth = bestFitWidth;
		}

		GUILayout.Space(10);
		bool bestFitHeight = EditorGUILayout.Toggle("Best Fit Height", m_Localize.m_BestFitHeight);
		if (bestFitHeight != m_Localize.m_BestFitHeight)
		{
			m_Localize.m_BestFitHeight = bestFitHeight;
		}

		GUILayout.Space(10);
		bool bestFitWidthAndHeight = EditorGUILayout.Toggle("Best Fit Width And Height", m_Localize.m_BestFitWidthAndHeight);
		if (bestFitWidthAndHeight != m_Localize.m_BestFitWidthAndHeight)
		{
			m_Localize.m_BestFitWidthAndHeight = bestFitWidthAndHeight;
		}

		GUILayout.Space(10);
		bool autoLineFeed = EditorGUILayout.Toggle("AutoLineFeed", m_Localize.autoLineFeed);
		if (autoLineFeed != m_Localize.autoLineFeed)
		{
			m_Localize.autoLineFeed = autoLineFeed;
		}
		
		GUILayout.Space(10);
		bool isOutlineCover = EditorGUILayout.Toggle("IsTextOutlineCover", m_Localize.isTextOutlineCover);
		if (isOutlineCover != m_Localize.isTextOutlineCover)
		{
			m_Localize.isTextOutlineCover = isOutlineCover;
		}

		GUILayout.BeginVertical();
		GUI.backgroundColor = Color.white;

		GUILayout.Space(5);
		m_ParamsList.DoLayoutList();

		GUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
    }

	void OnGUI_Language()
    {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Target:", GUILayout.Width(60));

		EditorGUI.BeginChangeCheck();
		int index = EditorGUILayout.Popup((int)currentLanguage, Enum.GetNames(typeof(Language)));
		if (EditorGUI.EndChangeCheck())
        {
			currentLanguage = (Language)index;

			serializedObject.ApplyModifiedProperties();

			Refresh();
		}

		GUILayout.EndHorizontal();
	}

	void OnGUI_MaterialPreset()
    {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Material Preset:", GUILayout.Width(110));

		int index = 0;
        if (!string.IsNullOrEmpty(m_Localize.m_MaterialName))
        {
			Enum.TryParse(m_Localize.m_MaterialName, out MaterialPresetName presetName);
			index = (int)presetName;
		}

		GUI.changed = false;

		int newIndex = EditorGUILayout.Popup(index, Enum.GetNames(typeof(MaterialPresetName)));

		if (GUI.changed)
		{
			GUI.changed = false;

			MaterialPresetName newName = (MaterialPresetName)newIndex;

            if (newName == MaterialPresetName.None)
            {
				mMaterialName.stringValue = string.Empty;
			}
            else
            {
				mMaterialName.stringValue = newName.ToString();
			}

			serializedObject.ApplyModifiedProperties();
		}

		GUILayout.EndHorizontal();
	}

	void OnGUI_Terms()
    {
		OnGUI_PrimaryTerm();
	}

	void OnGUI_PrimaryTerm()
	{
		string Key = m_Localize.m_Term;
		if (string.IsNullOrEmpty(Key))
		{
			m_Localize.GetFinalTerms(out Key);
		}

		if (OnGUI_SelectKey(ref Key, string.IsNullOrEmpty(m_Localize.m_Term)))
        {
			mTerm.stringValue = Key;

			serializedObject.ApplyModifiedProperties();

			Refresh();
		}
	}

	void Refresh()
    {
		if (languageSourceData.TryGetTranslation(m_Localize.m_Term, out string Translation, currentLanguage.ToString()))
		{
			if (m_Localize.IsValid())
			{
				m_Localize.Target.text = Translation;
			}
		}
	}

	bool OnGUI_SelectKey(ref string Term, bool Inherited)
    {
		GUILayout.Space(5);
		GUILayout.BeginHorizontal();

		GUI.changed = false;

		GUILayout.Label("Term:", GUILayout.Width(60));

		bool bChanged = false;

		if (Inherited)
			GUI.contentColor = Color.Lerp(Color.gray, Color.yellow, 0.1f);

		int Index = Term == "-" || Term == "" ? ableTerms.Length - 1 : Array.IndexOf(ableTerms, Term);

		GUI.changed = false;
		int newIndex = EditorGUILayout.Popup(Index, ableTerms);

		bool comfirmIndex = false;
		comfirmIndex = EditorGUILayout.Toggle(comfirmIndex);

		GUI.contentColor = Color.white;
		if (GUI.changed || (comfirmIndex && Inherited && newIndex != -1))  
		{
			Log.Info(ableTerms.Length);
			GUI.changed = false;

			Term = ableTerms[newIndex];

			bChanged = true;
		}

		GUILayout.EndHorizontal();

		GUILayout.Space(5);
		return bChanged;
	}

	private ReorderableList getList(SerializedObject serObject)
	{
		if (m_ParamsList == null)
		{
			m_ParamsList = new ReorderableList(serObject, serObject.FindProperty("_Params"), true, true, true, true);
			m_ParamsList.drawElementCallback = drawElementCallback;
			m_ParamsList.drawHeaderCallback = drawHeaderCallback;
			m_ParamsList.onAddCallback = addElementCallback;
			m_ParamsList.onRemoveCallback = removeElementCallback;
		}
		else
		{
			m_ParamsList.serializedProperty = serObject.FindProperty("_Params");
		}
		return m_ParamsList;
	}

	private void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
	{
		var serializedElement = m_ParamsList.serializedProperty.GetArrayElementAtIndex(index);
		var content = new GUIContent();

		Rect r = rect; r.xMax = r.xMin + 40;
		GUI.Label(r, "Name");

		r = rect; r.xMax = (r.xMax + r.xMin) / 2 - 2; r.xMin = r.xMin + 40;
		EditorGUI.PropertyField(r, serializedElement.FindPropertyRelative("Name"), content);

		r = rect; r.xMin = (r.xMax + r.xMin) / 2 + 2; r.xMax = r.xMin + 40;
		GUI.Label(r, "Value");

		r = rect; r.xMin = (r.xMax + r.xMin) / 2 + 2 + 40;
		EditorGUI.PropertyField(r, serializedElement.FindPropertyRelative("Value"), content);
	}

	private void addElementCallback(ReorderableList list)
	{
		serializedObject.ApplyModifiedProperties();
		var objParams = target as TextMeshProUGUILocalize;
		objParams._Params.Add(new TextMeshProUGUILocalize.ParamValue());
		list.index = objParams._Params.Count - 1;
		serializedObject.Update();
	}

	private void removeElementCallback(ReorderableList list)
	{
		if (list.index < 0)
			return;
		serializedObject.ApplyModifiedProperties();
		var objParams = target as TextMeshProUGUILocalize;
		objParams._Params.RemoveAt(list.index);
		serializedObject.Update();
	}

	private void drawHeaderCallback(Rect rect)
	{
		GUI.Label(rect, "Parameters:");
	}

	#region Styles

	public static GUIStyle GUIStyle_Header
	{
		get
		{
			if (mGUIStyle_Header == null)
			{
				mGUIStyle_Header = new GUIStyle("HeaderLabel");
				mGUIStyle_Header.fontSize = 25;
				mGUIStyle_Header.normal.textColor = Color.Lerp(Color.white, Color.gray, 0.5f);
				mGUIStyle_Header.fontStyle = FontStyle.BoldAndItalic;
				mGUIStyle_Header.alignment = TextAnchor.UpperCenter;
			}
			return mGUIStyle_Header;
		}
	}
	static GUIStyle mGUIStyle_Header;

	public static GUIStyle GUIStyle_SubHeader
	{
		get
		{
			if (mGUIStyle_SubHeader == null)
			{
				mGUIStyle_SubHeader = new GUIStyle("HeaderLabel");
				mGUIStyle_SubHeader.fontSize = 13;
				mGUIStyle_SubHeader.fontStyle = FontStyle.Normal;
				mGUIStyle_SubHeader.margin.top = -50;
				mGUIStyle_SubHeader.alignment = TextAnchor.UpperCenter;
			}
			return mGUIStyle_SubHeader;
		}
	}
	static GUIStyle mGUIStyle_SubHeader;

	public static GUIStyle GUIStyle_Background
	{
		get
		{
			if (mGUIStyle_Background == null)
			{
				mGUIStyle_Background = new GUIStyle(EditorStyles.textArea);
				mGUIStyle_Background.fixedHeight = 0;
				mGUIStyle_Background.overflow.left = 50;
				mGUIStyle_Background.overflow.right = 50;
				mGUIStyle_Background.overflow.top = -5;
				mGUIStyle_Background.overflow.bottom = 0;
			}
			return mGUIStyle_Background;
		}
	}
	static GUIStyle mGUIStyle_Background;

	public static GUIStyle GUIStyle_OldTextArea
	{
		get
		{
			if (mGUIStyle_OldTextArea == null)
			{
				mGUIStyle_OldTextArea = new GUIStyle(EditorStyles.textArea);
				mGUIStyle_OldTextArea.fixedHeight = 0;
			}
			return mGUIStyle_OldTextArea;
		}
	}
	static GUIStyle mGUIStyle_OldTextArea;

	#endregion
}
