using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HarvestKitchenChosenBar : MonoBehaviour
{
	private bool isStartTileMoveAnim = false;

	public Image RedBottomBG;

	[SerializeField] private HarvestKitchenGameMenu menu;
	
	private Transform[] TileMatchPositions;
	[SerializeField]
	private Transform[] TileMatchPositions_7;
	
    public OrderDictionary<int, List<HarvestKitchenTileItem>> chooseItemDict = new OrderDictionary<int, List<HarvestKitchenTileItem>>();
	public Dictionary<int, List<HarvestKitchenTileItem>> matchDict = new Dictionary<int, List<HarvestKitchenTileItem>>();

	public bool RecordChooseTile(HarvestKitchenTileItem tileItem, CallBack<HarvestKitchenTileItem> recoveryItem)
    {
	    if (tileItem)
	    {
		    if (GetChooseTotalNum() >= 7) return false;
		    
		    int tileId = tileItem.itemID;
		    if (!chooseItemDict.ContainsKey(tileId))
			    chooseItemDict.Add(tileId, new List<HarvestKitchenTileItem>());
		    chooseItemDict[tileId].Add(tileItem);
		    tileItem.transform.SetParent(transform);
		    ShowRedBottomBG();

            foreach (var itemKey in chooseItemDict.Keys)
            {
				if (chooseItemDict[itemKey].Count >= 3 && !matchDict.ContainsKey(itemKey))  
                {
					List<HarvestKitchenTileItem> list = new List<HarvestKitchenTileItem>();
					for (int i = 0; i < 3; i++)
					{
						list.Add(chooseItemDict[itemKey][i]);
						chooseItemDict[itemKey][i].isSelected = true;
					}
					matchDict.Add(itemKey, list);
				}
			}
	    }
	    
        int itemIndex = 0;
        bool isStartAnim = (tileItem == null);
        foreach (var itemKey in chooseItemDict.Keys)
        {
	        int valueIndex = 0;
	        int key = itemKey;
            foreach (var item in chooseItemDict[key])
            {
                bool isRemove = ((valueIndex + 1) % 3 == 0);
                if (!isStartAnim) isStartAnim = (item == tileItem);
                int animType = item == tileItem ? 1 : 2;
                if (isRemove)
                {
                    if (isStartAnim)
                    {
                        TilePosMove(animType, itemIndex, item, () =>
						{
							int tileID = item.itemID;
							int canDestroyNum = 0;
							if(matchDict.ContainsKey(tileID))
								canDestroyNum = matchDict[tileID].Count >= 3 ? 3 : 0;

                            void AnimEndAction()
                            {
	                            if (!chooseItemDict.ContainsKey(key)) return;
								if (!matchDict.ContainsKey(key)) return;

								var list = matchDict[key];
								int min = Mathf.Min(list.Count, 3);
	                            Vector3 target = Vector3.zero;
	                            for (int i = 0; i < min; i++)
	                            {
		                            target += list[i].transform.position;
	                            }

								bool isMatch = menu.FoodElimination(list[0], target / min + new Vector3(0, 0.07f, 0));

								if (isMatch && canDestroyNum == 3 && chooseItemDict.ContainsKey(key) && chooseItemDict[key] != null && chooseItemDict[key].Count >= 3) 
                                {
                                    //棋子合并成目标食物
                                    for (int i = 0; i < list.Count; i++)
                                    {
										if (chooseItemDict[key].Contains(list[i]))
											chooseItemDict[key].Remove(list[i]);
										list[i].DOKill();
									}
									list.Sort((a, b) =>
									{
										return a.transform.position.x < b.transform.position.x ? 1 : -1;
									});

									Vector3 upPos = new Vector3(0, 0.05f, 0);
									Vector3 newScale = new Vector3(1.1f, 1.1f, 1);

									Vector3 mergePos = list[1].transform.position + upPos;
									Vector3 backPos = mergePos - new Vector3(0, 0.03f, 0);
									Vector3 targetPos = mergePos + new Vector3(0, 0.03f, 0);
									list[0].transform.SetAsLastSibling();
									list[2].transform.SetAsLastSibling();
									list[1].transform.SetAsLastSibling();
									Vector3 fixedPos1 = list[0].transform.position + upPos;
									Vector3 backPos1 = fixedPos1 + (fixedPos1 - backPos).normalized * 0.05f;
									list[0].transform.DOScale(newScale, 0.15f);
									list[0].transform.DOLocalRotate(new Vector3(0, 0, -5), 0.15f);
									list[0].transform.DOMove(backPos1, 0.15f).SetEase(Ease.InOutQuad).onComplete = () =>
									{
										list[0].transform.DOMove(targetPos, 0.1f).SetEase(Ease.InCubic);
									};
									Vector3 fixedPos2 = list[2].transform.position + upPos;
									Vector3 backPos2 = fixedPos2 + (fixedPos2 - backPos).normalized * 0.05f;
									list[2].transform.DOScale(newScale, 0.15f);
									list[2].transform.DOLocalRotate(new Vector3(0, 0, 5), 0.15f);
									list[2].transform.DOMove(backPos2, 0.15f).SetEase(Ease.InOutQuad).onComplete = () =>
									{
										list[2].transform.DOMove(targetPos, 0.1f).SetEase(Ease.InCubic);
									};
									Vector3 backPos3 = targetPos + upPos;
									list[1].transform.DOScale(newScale, 0.15f);
									list[1].transform.DOMove(backPos3, 0.15f).SetEase(Ease.InOutQuad).onComplete = () =>
									{
										list[1].transform.DOMove(targetPos, 0.1f).SetEase(Ease.InCubic);
									};

									GameManager.Task.AddDelayTriggerTask(0.15f, () =>
									{
										GameManager.ObjectPool.SpawnWithRecycle<EffectObject>("KitchenMergeEffect", "TileItemDestroyEffectPool", 1.3f, transform.position, transform.rotation, transform, obj =>
										{
											GameObject effect = obj.Target as GameObject;
											effect.transform.position = mergePos;
											var m_PropMergeEffect = effect.GetComponent<Merge.PropMergeEffect>();
											m_PropMergeEffect.Show();
										});

										GameManager.Sound.PlayUISound(SoundType.SFX_Kitchen_Match_Combine_Required.ToString());
									});

									GameManager.Task.AddDelayTriggerTask(0.25f, () =>
									{
										foreach (var item in list)
                                        {
                                            item.DOKill();
											item.transform.localRotation = Quaternion.Euler(0, 0, 0);
											item.transform.localScale = Vector3.one;
											recoveryItem?.Invoke(item);
											item.gameObject.SetActive(false);
										}

										if (matchDict.ContainsKey(key)) matchDict.Remove(key);
									});
								}
                                else
                                {
									while (canDestroyNum > 0 && chooseItemDict.ContainsKey(key) && chooseItemDict[key] != null && chooseItemDict[key].Count > 0)
									{
										chooseItemDict[key][0].DestroyTile(canDestroyNum == 1, null, recoveryItem);
										chooseItemDict[key].RemoveAt(0);
										canDestroyNum--;
									}

									if (matchDict.ContainsKey(key)) matchDict.Remove(key);
								}

                                //if (chooseItemDict[key].Count <= 0) chooseItemDict.Remove(key);
							}

							AnimEndAction();

                            if (!isStartTileMoveAnim)
                            {
								isStartTileMoveAnim = true;
								GameManager.Task.AddDelayTriggerTask(0.1f, () =>
								{
									isStartTileMoveAnim = false;
									RecordChooseTile(null, recoveryItem);
								});
							}
                        }, true);
                    }
                }
                else
                {
	                TilePosMove(animType, itemIndex, item, null);
                }

                itemIndex++;
                valueIndex++;
            }
        }

        return CheckLose();
    }
    
    class TileSequence
    {
	    public Sequence sequ;
	    public float startTime;
	    public int taskId;

	    public TileSequence(Sequence seq,float time,int id)
	    {
		    sequ = seq;
		    startTime = time;
		    taskId = id;
	    }
    }
    
    Dictionary<HarvestKitchenTileItem, TileSequence> recordSequenceDict = new Dictionary<HarvestKitchenTileItem, TileSequence>();
    private void TilePosMove(int animType, int posIndex, HarvestKitchenTileItem tile, CallBack finishAction = null, bool isDestroy = false, bool isHaveAnim = true)
	{
		if (animType != 1 && tile.isSelected)
        {
			GameManager.Task.AddDelayTriggerTask(0, () =>
			{
				finishAction?.Invoke();
			});
			return;
		}

		float animTime = 0.4f;
		Vector3 targetPos = GetChoosePos(posIndex);

		Transform tileTrans = tile.transform;
		if (!isHaveAnim)
		{
			tileTrans.localEulerAngles = Vector3.zero;
			tileTrans.position = targetPos;
			tileTrans.localScale = Vector3.one * GetTileScale();
			recordSequenceDict.Remove(tile);
			GameManager.Task.AddDelayTriggerTask(0, () =>
			{
				finishAction?.Invoke();
			});
			return;
		}

		float useTime = 0;
		if (recordSequenceDict.TryGetValue(tile, out TileSequence sequenceData))
		{
			if (sequenceData.sequ != null)
			{
				sequenceData.sequ.Kill();
			}
			useTime = Time.time - sequenceData.startTime;
			if (sequenceData.taskId != 0) 
				GameManager.Task.RemoveDelayTriggerTask(sequenceData.taskId);
		}
		tile.transform.DOKill();

		animTime = Mathf.Max(animTime - useTime, 0.1f);
		int id = 0;

		id = GameManager.Task.AddDelayTriggerTask(animTime + 0.04f, () =>
		{
			finishAction?.Invoke();
			finishAction = null;
		});
		
		Sequence sequence1 = DOTween.Sequence();
		switch (animType)
		{
			case 1://当前飞下来的动画  放大然后左右摇晃 然后缩小 回正
				sequence1.Append(tileTrans.DOMove(targetPos, animTime)).SetEase(Ease.OutSine);
				sequence1.Join(tileTrans.DOScale(Vector3.one * GetTileScale(), animTime * 0.5f));
				sequence1.Join(tileTrans.DOLocalRotate(tile.transform.position.x >= 0 ? Vector3.back * 20 : Vector3.forward * 20, animTime * 2 / 3f));
				sequence1.Join(tileTrans.DOLocalRotate(Vector3.zero, animTime / 3f).SetDelay(animTime * 2 / 3f));
				if (!isDestroy)
					sequence1.Append(tileTrans.DOShakeRotation(0.1f, Vector3.forward * 5, 8, 180));
				break;
			default://
				sequence1.Append(tileTrans.DOMove(targetPos, animTime)).SetEase(Ease.OutSine);
				sequence1.Join(tileTrans.DOScale(Vector3.one * GetTileScale(), animTime * 0.7f));
				sequence1.Join(tileTrans.DOLocalRotate(Vector3.zero, animTime));
				break;
		}
		sequence1.OnKill(() => sequence1 = null);
		sequence1
				.OnComplete(() =>
				{
					if (tileTrans)
					{
						tileTrans.DOKill();
						tileTrans.localEulerAngles = Vector3.zero;
						tileTrans.localScale = Vector3.one * GetTileScale();
						tileTrans.position = targetPos;

						GameManager.Task.AddDelayTriggerTask(0, () =>
						{
							finishAction?.Invoke();
							finishAction = null;
						});
					}
					recordSequenceDict.Remove(tile);
				});
		recordSequenceDict[tile] = new TileSequence(sequence1, Time.time, id);
	}

    public float GetTileScale()
    {
	    return 0.89f;
    }
    
    public int GetChooseTotalNum(bool isEliminate = true)
    {
	    int num = 0;
	    foreach (var key in chooseItemDict.Keys)
	    {
		    num += isEliminate ? chooseItemDict[key].Count % 3 : chooseItemDict[key].Count;
	    }
	    return num;
    }
    
    public Vector3 GetChoosePos(int index)
    {
	    if (TileMatchPositions == null) 
		    TileMatchPositions = TileMatchPositions_7;

	    return (TileMatchPositions[1].position - TileMatchPositions[0].position) * index + TileMatchPositions[0].position;
    }
    
    Sequence redBGSequece = null;
    private void ShowRedBottomBG()
    {
	    if ((GetChooseTotalNum() + 1) >= 7)
	    {
		    if (redBGSequece != null && redBGSequece.IsPlaying()) return;
		    if (redBGSequece != null) redBGSequece.Kill();

		    RedBottomBG.gameObject.SetActive(true);
		    RedBottomBG.color = new Color(1, 1, 1, 0.4f);
		    redBGSequece = DOTween.Sequence()
			    .Append(RedBottomBG.DOFade(1, 1f))
			    .SetLoops(-1, LoopType.Yoyo).OnKill(() => redBGSequece = null);
	    }
	    else
	    {
		    if (redBGSequece != null) redBGSequece.Kill();

		    RedBottomBG.color = new Color(1, 1, 1, 0.4f);
		    RedBottomBG.gameObject.SetActive(false);
	    }
    }

    public bool CheckLose()
    {
	    if (GetChooseTotalNum() < 7) return false;
	    Log.Info("Kitchen：消除栏满了，游戏失败");
	    return true;
    }

    public void OnRelease()
    {
	    isStartTileMoveAnim = false;
	    RedBottomBG.gameObject.SetActive(false);
	    foreach (var key in chooseItemDict.Keys)
	    {
		    foreach (var item in chooseItemDict[key])
		    {
			    if (item)
			    {
				    item.gameObject.SetActive(false);
				    Destroy(item.gameObject);
			    }
		    }
	    }
	    chooseItemDict.Clear();
	    recordSequenceDict.Clear();
    }

    /// <summary>
    /// 接关，清空选择栏中的棋子
    /// </summary>
    public void LevelContinue(CallBack<HarvestKitchenTileItem> recoveryItem)
    {
	    foreach (var key in chooseItemDict.Keys)
	    {
		    while (chooseItemDict[key].Count > 0)
		    {
			    chooseItemDict[key][0].DestroyTile(false, null, recoveryItem);
			    chooseItemDict[key].RemoveAt(0);
		    }
	    }
	    chooseItemDict.Clear();
	    ShowRedBottomBG();
    }
}
