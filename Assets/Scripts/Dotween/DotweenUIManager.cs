using DG.Tweening;
using UnityEngine;

public class DotweenUIManager : SingleTonGeneric<DotweenUIManager>
{
    
    public void SlideTo(RectTransform target, Vector2 endPos, float duration)
    {
        target.DOAnchorPos(endPos, duration).SetEase(Ease.OutCubic);

        //target : 움직일 UI의 RectTransform
        //endPos : UI가 최종적으로 보여져야 하는 자리
        //duration : 몇초 동안 이동 할 지  
        //Ease.OutCubic : 속도 곡선(빠르게 시작 → 부드럽게 감속)
        // -> anchoredPosition 에서 endPos 까지 duration 동안 UI를 부드럽게 이동
    }

}
