using DG.Tweening;
using UnityEngine;

public abstract  class UIAnimatedPanel : MonoBehaviour
{
    protected RectTransform rt;
    protected CanvasGroup cg;

    protected virtual void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>(); // 없으면 null 그대로
    }

    protected virtual void OnEnable()
    {
       // PlayShowAnimation();
    }

    protected virtual void OnDisable()
    {
        rt?.DOKill();
        cg?.DOKill();
    }

    // 자식이 반드시 구현해야 하는 애니메이션 규칙
    protected abstract void PlayShowAnimation();
    protected abstract void PlayHideAnimation();
    protected abstract void PlayInitAnimation();
}
