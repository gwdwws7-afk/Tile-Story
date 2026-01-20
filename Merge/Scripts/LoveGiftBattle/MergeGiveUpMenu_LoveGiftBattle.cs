using System;

namespace Merge
{
    public class MergeGiveUpMenu_LoveGiftBattle : MergeGiveUpMenu
    {
        protected override void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"));
            GameManager.UI.HideUIForm(this);

            if (GameManager.DataNode.GetData<bool>("ShowedMergeEndMenu", false))
            {
                MergeManager.Instance.EndActivity();
            }
            else
            {
                GameManager.UI.ShowUIForm("MergeEndMenu_Fail");
            }
        }
    }
}
