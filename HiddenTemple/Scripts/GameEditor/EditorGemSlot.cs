using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HiddenTemple
{
    public class EditorGemSlot : MonoBehaviour
    {
        public Image m_GemImage;
        public TextMeshProUGUI m_SizeText;
        public GameObject m_Mask;
        public Button m_Button;

        private DRGemData m_Data;
        private HiddenTempleEditorMenu m_Editor;

        public int GemId => m_Data != null ? m_Data.Id : 0;

        public void Initialize(DRGemData data, HiddenTempleEditorMenu editor)
        {
            m_Data = data;
            m_Editor = editor;

#if UNITY_EDITOR
            if (data != null)
            {
                int series = data.Id / 100;
                m_GemImage.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/HiddenTemple/Sprites/Gem/0{series}/{data.AssetName}.png");
                m_SizeText.text = data.Width + "x" + data.Height;
            }
#endif

            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(OnButtonClick);
        }

        public void Release()
        {
            m_Button.onClick.RemoveAllListeners();
        }

        private void OnButtonClick()
        {
            if (m_Data != null)
            {
                m_Editor.SelectGem(m_Data.Id);
            }
        }
    }
}
