using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 키보드 기반 카드 뒤집기 미니게임에서 사용되는 개별 카드 컴포넌트.
/// - Front/Back 오브젝트를 활성/비활성 전환하며 뒤집기 연출을 단순화한다.
/// - 선택/매칭 상태에 따라 색을 변경해 현재 포커스를 시각적으로 표현한다.
/// </summary>
public class FindCardCard : MonoBehaviour
{
    [Header("Card Objects")]
    [SerializeField] private GameObject frontObject;
    [SerializeField] private GameObject backObject;

    [Header("Components")]
    [SerializeField] private Image frontImage;
    [SerializeField] private Image backImage;
    [SerializeField] private Button backButton;

    [Header("Pair Info")]
    [Tooltip("같은 값을 가진 카드끼리 짝을 이룹니다.")]
    [SerializeField] private int pairId;

    private bool isSelected;
    private bool isMatched;

    private Color normalColor = Color.white;
    private Color highlightColor = new Color(0.2f, 0.55f, 1f, 1f);
    private Color matchedColor = new Color(0.65f, 0.65f, 0.65f, 1f);

    public int PairId => pairId;
    public bool IsMatched => isMatched;
    public bool IsRevealed => frontObject != null && frontObject.activeSelf;
    public bool IsSelectable => !isMatched && gameObject.activeInHierarchy;

    private void Awake()
    {
        AutoAssignReferences();
        RefreshVisual();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        AutoAssignReferences();
        RefreshVisual();
    }
#endif

    public void ApplyTheme(Color normal, Color highlight, Color matched)
    {
        normalColor = normal;
        highlightColor = highlight;
        matchedColor = matched;
        RefreshVisual();
    }

    public void SetPairId(int id)
    {
        pairId = id;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        RefreshVisual();
    }

    public void ResetForGame()
    {
        isMatched = false;
        isSelected = false;
        if (backButton != null)
        {
            backButton.interactable = true;
        }

        ShowBack();
        RefreshVisual();
    }

    public void Reveal()
    {
        if (isMatched)
            return;

        ShowFront();
        RefreshVisual();
    }

    public void Hide()
    {
        if (isMatched)
            return;

        ShowBack();
        RefreshVisual();
    }

    public void MarkMatched()
    {
        isMatched = true;
        isSelected = false;
        ShowFront();
        if (backButton != null)
        {
            backButton.interactable = false;
        }

        RefreshVisual();
    }

    private void ShowFront()
    {
        if (frontObject != null)
            frontObject.SetActive(true);
        if (backObject != null)
            backObject.SetActive(false);
    }

    private void ShowBack()
    {
        if (frontObject != null)
            frontObject.SetActive(false);
        if (backObject != null)
            backObject.SetActive(true);
    }

    private void RefreshVisual()
    {
        Color target = isMatched ? matchedColor : (isSelected ? highlightColor : normalColor);
        if (frontImage != null)
            frontImage.color = target;
        if (backImage != null)
            backImage.color = target;
    }

    private void AutoAssignReferences()
    {
        if (frontObject == null)
        {
            Transform child = transform.Find("Front");
            if (child != null)
                frontObject = child.gameObject;
        }

        if (backObject == null)
        {
            Transform child = transform.Find("Back");
            if (child != null)
                backObject = child.gameObject;
        }

        if (frontImage == null && frontObject != null)
            frontImage = frontObject.GetComponent<Image>();
        if (backImage == null && backObject != null)
            backImage = backObject.GetComponent<Image>();
        if (backButton == null && backObject != null)
            backButton = backObject.GetComponent<Button>();
    }
}
