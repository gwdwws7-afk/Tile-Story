using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public class CardSetBookMenu : CenterForm
{
    public DelayButton closeButton;
    public DelayButton leftButton, rightButton;
    public OptimizedSwiper swiper;
    public LoopScrollView scrollView;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        
        //scroll
        List<int> indexList = new List<int>();
        foreach (var kvp in CardModel.Instance.CardSetDict)
        {
            indexList.Add(kvp.Value.CardSetID - 1);
        }
        scrollView.UpdateData(indexList, Convert.ToInt32(userData) - 1);
        //scrollView.JumpTo(Convert.ToInt32(userData) - 1);
        
        //swiper
        swiper.currentPage = Convert.ToInt32(userData) - 1;
        swiper.totalPages = CardModel.Instance.CardSetDict.Count;
        
        //button
        closeButton.OnInit(OnCloseButtonClick);
        leftButton.OnInit(swiper.PreviousPage);
        rightButton.OnInit(swiper.NextPage);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
        swiper.Init();
    }

    public override void OnRelease()
    {
        base.OnRelease();
        swiper.Release();
        foreach (Transform child in swiper.content)
        {
            child.GetComponent<CardSetPage>().Release();
        }
        
        closeButton.OnReset();
        leftButton.OnReset();
        rightButton.OnReset();
    }
    
    private void OnCloseButtonClick()
    {
        if (CardModel.Instance.NewCardDict.Remove(swiper.currentPage + 1))
        {
            CardModel.Instance.SaveToLocal();
        }
        // CardSetMainMenu mainMenu = GameManager.UI.GetUIForm<CardSetMainMenu>() as CardSetMainMenu;
        // if (mainMenu != null) mainMenu.RefreshCardSetState();
        GameManager.UI.HideUIForm(this);
    }
}
