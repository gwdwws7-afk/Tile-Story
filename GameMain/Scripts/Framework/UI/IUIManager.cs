using System;
using System.Collections;
using System.Collections.Generic;
public interface IUIManager
{
    int UIGroupCount { get; }

    float AutoReleaseInterval { get; set; }

    bool IsAsyncLoading { get;}

    void SetUIFormHelper(IUIFormHelper uiFormHelper);

    bool HasUIGroup(string uiGroupName);

    UIGroup GetUIGroup(string uiGroupName);

    UIGroup[] GetAllUIGroups();

    void GetAllUIGroups(List<UIGroup> results);

    bool AddUIGroup(UIGroup uiGroup);

    bool HasUIForm(string uiName);

    bool HasUIForm(string uiName, string groupName);

    UIForm GetUIForm(string uiName);

    UIForm GetUIForm(string uiName, UIGroup group);

    UIForm[] GetAllLoadedUIForms();

    void GetAllLoadedUIForms(List<UIForm> results);

    bool IsLoadingUIForm(string uiFormName);

    void CreateUIForm(string uiName,UIFormType uiFormType,Action<UIForm> createSuccessAction = null, Action createFailAction = null);

    void ShowUIForm(string uiName, UIFormType uiFormType, Action<UIForm> showSuccessAction = null, Action showFailAction = null, object userData = null);
    
    void HideUIForm(string uiFormName, Action onHideComplete = null, object userData = null);

    void HideUIForm(UIForm uiForm, Action onHideComplete = null, object userData = null);

    void HideAllUIForm(string[] uiExceptFormsName);

    void HideAllUIForm(params UIForm[] uiExceptForms);

    void ReleaseAllUIForm(params UIForm[] uiExceptForms);

    bool IsHasFormInGroup(string uiGroupName);

    void OnReset();

    void OnInit();

    void RefreshAllUIByLogin();
}
