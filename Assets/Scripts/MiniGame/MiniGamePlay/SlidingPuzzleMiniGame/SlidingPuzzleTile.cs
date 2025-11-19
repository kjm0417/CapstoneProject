using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 슬라이딩 퍼즐의 개별 타일 컴포넌트.
/// 타일 번호, 이미지, 빈 타일 여부를 관리합니다.
/// </summary>
public class SlidingPuzzleTile : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private Image tileImage;
    [SerializeField] private Text tileNumberText; // 디버그용 번호 표시 (선택사항)

    private int tileNumber; // 1~16, 16번이 빈 타일
    private int correctPosition; // 정답 위치 (0~15)
    private bool isEmpty; // 빈 타일 여부

    public int TileNumber => tileNumber;
    public int CorrectPosition => correctPosition;
    public bool IsEmpty => isEmpty;

    /// <summary>
    /// 타일 초기화
    /// </summary>
    public void Initialize(int number, int correctPos, Sprite tileSprite, bool empty = false)
    {
        tileNumber = number;
        correctPosition = correctPos;
        isEmpty = empty;

        if (tileImage != null)
        {
            tileImage.sprite = tileSprite;
            tileImage.enabled = !isEmpty; // 빈 타일은 이미지 숨김
        }

        if (tileNumberText != null)
        {
            tileNumberText.text = isEmpty ? "" : number.ToString();
        }
    }

    /// <summary>
    /// 타일이 정답 위치에 있는지 확인
    /// </summary>
    public bool IsInCorrectPosition(int currentIndex)
    {
        return currentIndex == correctPosition;
    }

    /// <summary>
    /// 빈 타일 표시 설정
    /// </summary>
    public void SetEmptyVisual(bool show)
    {
        if (tileImage != null)
        {
            tileImage.enabled = !show;
        }
    }
}
