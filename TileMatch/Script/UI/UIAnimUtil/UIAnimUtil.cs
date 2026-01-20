using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIAnimUtil : MonoBehaviour
{
	[SerializeField]
	private Vector3 TargetScale=Vector3.one;
	[SerializeField]
	private float AnimTime=0.4f;
	private void OnEnable()
	{
		transform.localScale = Vector3.zero;
		transform.DOScale(TargetScale, AnimTime).SetEase(Ease.OutBack);
	}

	private void OnDisable()
	{
		transform.DOKill();
		transform.localScale = Vector3.zero;
	}
}
