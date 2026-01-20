using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class GameWellDonePanel_RewardsArea : MonoBehaviour
{
	[SerializeField]
	private GameObject Coin, MergeEnergy, KitchenChefHat, HiddenTemplePickaxe,HarvestKitchenChefHat;
	[SerializeField]
	private TextMeshProUGUI CoinNum_Text, MergeEnergyNum_Text, KitchenChefHatNum_Text,HarvestKitchenChefHatNum_Text, HiddenTemplePickaxeNum_Text;
	[SerializeField]
	private TextMeshProUGUI CoinNum_TripleText, MergeEnergyNum_TripleText, KitchenChefHatNum_TripleText,HarvestKitchenChefHatNum_TripleText;
	[SerializeField]
	private ParticleSystem CoinNumChangeEffect, MergeEnergyNumChangeEffect, KitchenChefHatNumChangeEffect,HarvestKitchenChefHatNumChangeEffect;

	private AsyncOperationHandle spriteHandle;

	public void Initialize(int recordCoinNum)
    {
		List<GameObject> showObjects = new List<GameObject>();

		if (Merge.MergeManager.Instance.CheckLevelWinCanGetTarget())
		{
			int level = GameManager.PlayerData.RealLevel() - 1;
			int hardIndex = DTLevelUtil.GetLevelHard(level);
			MergeEnergyNum_Text.text = Merge.MergeManager.Instance.GetLevelWinCanGetTargetNum(1, hardIndex).ToString();
			MergeEnergy.SetActive(true);
			showObjects.Add(MergeEnergy);

			UnityUtility.UnloadAssetAsync(spriteHandle);
			spriteHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetAltasSpriteName(Merge.MergeManager.Instance.GetMergeEnergyBoxName(), "TotalItemAtlas"), sp =>
			{
				Image propImage = MergeEnergy.GetComponentInChildren<Image>();
				propImage.sprite = sp;
				//propImage.SetNativeSize();
				propImage.DOFade(1, 0.1f);
			});
		}
		else
		{
			MergeEnergy.SetActive(false);
		}

		if (KitchenManager.Instance != null && KitchenManager.Instance.CheckLevelWinCanGetTarget())
		{
			KitchenChefHatNum_Text.text = KitchenManager.Instance
				.GetPropNum(
					DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel(GameManager.PlayerData.NowLevel - 1)))
				.ToString();
			KitchenChefHat.SetActive(true);
			showObjects.Add(KitchenChefHat);
		}
		else
		{
			KitchenChefHat.SetActive(false);
		}

        if (HiddenTemple.HiddenTempleManager.Instance.CheckLevelWinCanGetPickaxe())
        {
			HiddenTemplePickaxeNum_Text.text = HiddenTemple.HiddenTempleManager.PlayerData.GetPickaxeLevelCollectNum().ToString();
			HiddenTemplePickaxe.SetActive(true);
			showObjects.Add(HiddenTemplePickaxe);
		}
        else
        {
			HiddenTemplePickaxe.SetActive(false);
		}

        if (HarvestKitchenManager.Instance != null && HarvestKitchenManager.Instance.CheckLevelWinCanGetTarget())
        {
	        HarvestKitchenChefHatNum_Text.text = HarvestKitchenManager.Instance
		        .GetPropNum(
			        DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel(GameManager.PlayerData.NowLevel - 1)))
		        .ToString();
	        HarvestKitchenChefHat.SetActive(true);
	        showObjects.Add(HarvestKitchenChefHat);
        }
        else
        {
	        HarvestKitchenChefHat.SetActive(false);
        }
        
		if (recordCoinNum > 0)
        {
			CoinNum_Text.text = recordCoinNum.ToString();
			Coin.transform.parent.gameObject.SetActive(true);
            if (showObjects.Count > 0)
            {
				Coin.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
				Coin.transform.localPosition = new Vector3(-37, 106.2f, 0);
				CoinNum_Text.transform.localPosition = new Vector3(26, 73, 0);
			}
            else
            {
				Coin.transform.localScale = Vector3.one;
				Coin.transform.localPosition = new Vector3(-117.8f, 106.2f, 0);
				CoinNum_Text.transform.localPosition = new Vector3(69, 110.8f, 0);
			}

			showObjects.Add(Coin.transform.parent.gameObject);
		}
        else
        {
			Coin.transform.parent.gameObject.SetActive(false);
		}

        Vector3[] localPos = UnityUtility.GetAveragePosition(Vector3.zero, new Vector3(300, 0, 0), showObjects.Count);
        for (int i = 0; i < showObjects.Count; i++)
        {
			showObjects[i].transform.localPosition = localPos[i];
		}
	}

	public void Release()
    {
		UnityUtility.UnloadAssetAsync(spriteHandle);
	}

	public void ShowCoinTextAnim(int startNum, int endNum)
	{
		try
		{
			GameManager.Sound.PlayAudio(SoundType.SFX_AddCoin.ToString());
			DOTween.To(() => startNum, a => CoinNum_Text.text = $"{a}", endNum, 0.7f).SetEase(Ease.Linear).OnComplete(() =>
			{
				CoinNum_Text.transform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.InOutSine).OnComplete(() =>
				{
					CoinNum_Text.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
				});
			});
		}
		catch (Exception e)
		{
			Log.Error("ShowCoinTextAnim error:{0}", e.Message);
		}
	}

	public void ShowFirstTryIncreaseRewardAnim(Sequence sequence, int coinEndNum)
    {
		ShowFirstTryStage1Anim(sequence, true, CoinNum_TripleText);
		if (Merge.MergeManager.Instance.CheckLevelWinCanGetTarget() && Merge.MergeManager.Instance.CheckLevelWinGainedTargetNumAffectedByFirstTry()) 
			ShowFirstTryStage1Anim(sequence, false, MergeEnergyNum_TripleText);
		if(KitchenManager.Instance != null && KitchenManager.Instance.CheckLevelWinCanGetTarget())
			ShowFirstTryStage1Anim(sequence, false, KitchenChefHatNum_TripleText);
		if(HarvestKitchenManager.Instance != null && HarvestKitchenManager.Instance.CheckLevelWinCanGetTarget())
			ShowFirstTryStage1Anim(sequence, false, HarvestKitchenChefHatNum_TripleText);
		
		ShowFirstTryStage2Anim(sequence, true, CoinNum_TripleText);
		if (Merge.MergeManager.Instance.CheckLevelWinCanGetTarget() && Merge.MergeManager.Instance.CheckLevelWinGainedTargetNumAffectedByFirstTry())
			ShowFirstTryStage2Anim(sequence, false, MergeEnergyNum_TripleText);
		if(KitchenManager.Instance != null && KitchenManager.Instance.CheckLevelWinCanGetTarget())
			ShowFirstTryStage2Anim(sequence, false, KitchenChefHatNum_TripleText);
		if(HarvestKitchenManager.Instance != null && HarvestKitchenManager.Instance.CheckLevelWinCanGetTarget())
			ShowFirstTryStage2Anim(sequence, false, HarvestKitchenChefHatNum_TripleText);
		
		ShowFirstTryStage3Anim(sequence, true, CoinNum_Text, CoinNum_TripleText, CoinNumChangeEffect, coinEndNum);
		if (Merge.MergeManager.Instance.CheckLevelWinCanGetTarget() && Merge.MergeManager.Instance.CheckLevelWinGainedTargetNumAffectedByFirstTry())
			ShowFirstTryStage3Anim(sequence, false, MergeEnergyNum_Text, MergeEnergyNum_TripleText,
				MergeEnergyNumChangeEffect, Merge.MergeManager.PlayerData.GetMergeEnergyBoxLevelCollectNum());
		if (KitchenManager.Instance != null && KitchenManager.Instance.CheckLevelWinCanGetTarget())
			ShowFirstTryStage3Anim(sequence, false, KitchenChefHatNum_Text, KitchenChefHatNum_TripleText,
				KitchenChefHatNumChangeEffect, GameManager.DataNode.GetData("KitchenLevelWinChefHatNum", 1));
		if (HarvestKitchenManager.Instance != null && HarvestKitchenManager.Instance.CheckLevelWinCanGetTarget())
			ShowFirstTryStage3Anim(sequence, false, HarvestKitchenChefHatNum_Text, HarvestKitchenChefHatNum_TripleText,
				HarvestKitchenChefHatNumChangeEffect, GameManager.DataNode.GetData("HarvestKitchenLevelWinChefHatNum", 1));
    }

	private void ShowFirstTryStage1Anim(Sequence sequence, bool isHead, TextMeshProUGUI tripleText)
    {
		tripleText.alpha = 0;
		tripleText.transform.localPosition = Vector3.zero;
		tripleText.gameObject.SetActive(true);
		Vector3 jumpFirstPos = tripleText.transform.position + new Vector3(0.2f, 0.1f, 0);

        if (isHead)
        {
			sequence.Append(tripleText.transform.DOMove(jumpFirstPos, 0.3f)).Join(tripleText.DOFade(1, 0.15f)).Join(tripleText.transform.DOScale(1.3f, 0.2f).OnComplete(() =>
			{
				tripleText.transform.DOScale(0.9f, 0.15f).SetEase(Ease.InQuad).OnComplete(() =>
				{
					tripleText.transform.DOScale(1f, 0.15f);
				});
			}));
		}
        else
        {
			sequence.Join(tripleText.transform.DOMove(jumpFirstPos, 0.3f)).Join(tripleText.DOFade(1, 0.15f)).Join(tripleText.transform.DOScale(1.3f, 0.2f).OnComplete(() =>
			{
				tripleText.transform.DOScale(0.9f, 0.15f).SetEase(Ease.InQuad).OnComplete(() =>
				{
					tripleText.transform.DOScale(1f, 0.15f);
				});
			}));
		}
	}

	private void ShowFirstTryStage2Anim(Sequence sequence, bool isHead, TextMeshProUGUI tripleText)
    {
		if (isHead)
		{
			sequence.Append(tripleText.transform.DOScale(new Vector3(1.05f, 0.95f, 1f), 0.2f).SetEase(Ease.OutQuart));
		}
		else
		{
			sequence.Join(tripleText.transform.DOScale(new Vector3(1.05f, 0.95f, 1f), 0.2f).SetEase(Ease.OutQuart));
		}
	}

	private void ShowFirstTryStage3Anim(Sequence sequence, bool isHead, TextMeshProUGUI mainText, TextMeshProUGUI tripleText, ParticleSystem effect, int endNum)
	{
		Vector3 jumpTargetPos = mainText.transform.position;

		if (isHead)
		{
			sequence.Append(tripleText.transform.DOJump(jumpTargetPos, 0.4f, 1, 0.4f).SetEase(Ease.InQuart).OnComplete(() =>
			{
				effect.Play();
				tripleText.gameObject.SetActive(false);
				mainText.transform.DOPunchScale(new Vector3(0.5f, 0.5f), 0.3f, 1);

				mainText.text = endNum.ToString();

				GameManager.Sound.PlayAudio("SFX_Level_FirstTry_Triple");
			})).Join(tripleText.transform.DOScale(new Vector3(0.95f, 1.05f, 1f), 0.2f).SetEase(Ease.InQuart));
		}
		else
		{
			sequence.Join(tripleText.transform.DOJump(jumpTargetPos, 0.4f, 1, 0.4f).SetEase(Ease.InQuart).OnComplete(() =>
			{
				effect.Play();
				tripleText.gameObject.SetActive(false);
				mainText.transform.DOPunchScale(new Vector3(0.5f, 0.5f), 0.3f, 1);

				mainText.text = endNum.ToString();
			})).Join(tripleText.transform.DOScale(new Vector3(0.95f, 1.05f, 1f), 0.2f).SetEase(Ease.InQuart));
		}
	}
}
