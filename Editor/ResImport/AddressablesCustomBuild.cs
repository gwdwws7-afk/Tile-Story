using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomBuildScriptPacked.asset", menuName = "Addressables/Content Builders/Custom Build Script")]
public class AddressablesCustomBuild : BuildScriptPackedMode
{
    public override string Name => "Custom Build Script";

    protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
    {
        //打包之前先处理
        PreBuildAddressable();

        TResult result = base.BuildDataImplementation<TResult>(builderInput);

        bool success = string.IsNullOrEmpty(result.Error);

        if (success)
        {
            Debug.Log("✅ Addressables 打包完成...");
        }
        else
        {
            Debug.LogError($"❌ Addressables 打包失败: {result.Error}");
        }

        return result;
    }

    /// <summary>
    /// 打包之前的操作
    /// </summary>
    private void PreBuildAddressable()
    {
        LevelAddressableChecker.CheckAndAddMissingJson();
    }
}
