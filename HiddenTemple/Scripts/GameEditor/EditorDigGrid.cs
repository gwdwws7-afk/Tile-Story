using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HiddenTemple
{
    public class EditorDigGrid : DigGrid, IPointerClickHandler
    {
        public override void Initialize(int row, int col, int canDigTime, HiddenTempleBaseMenu menu)
        {
            base.Initialize(row, col, canDigTime, menu);

            if (m_CanDigTime == 2)
                m_Cover.SetActive(true);
            m_BrokenCover.SetActive(m_CanDigTime == 2);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            HiddenTempleEditorMenu editorMenu = m_Menu as HiddenTempleEditorMenu;
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (editorMenu.SelectedDoubleBlock)
                {
                    editorMenu.AddDoubleBlock(m_Row, m_Col);
                }
                else
                {
                    if (editorMenu.CurSelectedGemId <= 0)
                        return;
                    editorMenu.AddGem(editorMenu.CurSelectedGemId, m_Row, m_Col);
                }
            }
            else
            {
                if(editorMenu.SelectedDoubleBlock)
                    editorMenu.RemoveDoubleBlock(m_Row, m_Col);
                else
                    editorMenu.RemoveGem(m_Row, m_Col);
            }
        }

        protected override void OnButtonClick()
        {
        }
    }
}
