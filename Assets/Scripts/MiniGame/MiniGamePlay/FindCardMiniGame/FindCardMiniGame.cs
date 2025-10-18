using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방향키와 엔터 입력으로 플레이하는 카드 짝 맞추기 미니게임.
/// GridLayoutGroup에 배치된 <see cref="FindCardCard"/>를 자동으로 수집해
/// 키보드 포커스 이동, 선택, 매칭 검증을 관리한다.
/// </summary>
public class FindCardMiniGame : MiniGameBase
{
    [Header("Card Layout")]
    [SerializeField] private GridLayoutGroup cardContainer;
    [Tooltip("가로로 이동할 때 사용할 열의 수. 0이면 GridLayoutGroup 설정을 기반으로 추정합니다.")]
    [SerializeField] private int columns = 4;

    [Header("Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(0.2f, 0.55f, 1f, 1f);
    [SerializeField] private Color matchedColor = new Color(0.65f, 0.65f, 0.65f, 1f);
    [SerializeField] private float mismatchHideDelay = 0.75f;

    private readonly List<FindCardCard> cards = new List<FindCardCard>();
    private FindCardCard firstRevealed;
    private Coroutine mismatchRoutine;

    private int selectedIndex = -1;
    private int matchedPairs;
    private int totalPairs;
    private bool inputLocked;

    protected override void OnStartHook()
    {
        base.OnStartHook();

        if (mismatchRoutine != null)
        {
            StopCoroutine(mismatchRoutine);
            mismatchRoutine = null;
        }

        if (cardContainer == null)
        {
            cardContainer = GetComponentInChildren<GridLayoutGroup>(includeInactive: true);
        }

        cards.Clear();
        if (cardContainer != null)
        {
            FindCardCard[] found = cardContainer.GetComponentsInChildren<FindCardCard>(includeInactive: true);
            foreach (FindCardCard card in found)
            {
                if (card != null && !cards.Contains(card))
                {
                    cards.Add(card);
                }
            }
        }

        if (cards.Count == 0)
        {
            Debug.LogError("[FindCardMiniGame] 카드가 존재하지 않습니다. GridLayoutGroup 하위에 FindCardCard를 배치하세요.");
            Abort("NO_CARDS");
            return;
        }

        if (columns <= 0)
        {
            columns = GuessColumnCount(cardContainer);
        }
        columns = Mathf.Max(1, columns);

        totalPairs = CalculateTotalPairs(out bool hasOddCount);
        if (totalPairs <= 0)
        {
            Debug.LogError("[FindCardMiniGame] 올바른 짝 정보를 가진 카드가 없습니다.");
            Abort("NO_PAIR_DATA");
            return;
        }

        if (hasOddCount)
        {
            Debug.LogWarning("[FindCardMiniGame] 일부 카드가 짝이 없이 남을 수 있습니다. pairId 구성을 확인하세요.");
        }

        firstRevealed = null;
        matchedPairs = 0;
        inputLocked = false;

        foreach (FindCardCard card in cards)
        {
            if (card == null)
                continue;

            card.ApplyTheme(normalColor, highlightColor, matchedColor);
            card.ResetForGame();
        }

        selectedIndex = -1;
        int firstIndex = FindFirstSelectableIndex();
        if (firstIndex < 0)
        {
            Debug.LogError("[FindCardMiniGame] 선택 가능한 카드가 없습니다.");
            Abort("NO_SELECTABLE_CARD");
            return;
        }

        UpdateSelection(firstIndex);
    }

    protected override void OnUpdateHook(float deltaTime)
    {
        base.OnUpdateHook(deltaTime);
        if (!IsRunning || cards.Count == 0)
            return;

        if (inputLocked)
            return;

        HandleMoveInput();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            TryRevealSelected();
        }
    }

    protected override void OnCleanupHook()
    {
        base.OnCleanupHook();

        if (mismatchRoutine != null)
        {
            StopCoroutine(mismatchRoutine);
            mismatchRoutine = null;
        }

        foreach (FindCardCard card in cards)
        {
            card?.SetSelected(false);
        }

        firstRevealed = null;
        matchedPairs = 0;
        totalPairs = 0;
        selectedIndex = -1;
        inputLocked = false;
    }

    protected override bool CheckClearRealtime()
    {
        if (!IsRunning)
            return false;

        return totalPairs > 0 && matchedPairs >= totalPairs;
    }

    private void HandleMoveInput()
    {
        if (cards.Count == 0)
            return;

        int newIndex = selectedIndex;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            newIndex = MoveHorizontal(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            newIndex = MoveHorizontal(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            newIndex = MoveVertical(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            newIndex = MoveVertical(-1);
        }

        if (newIndex != selectedIndex)
        {
            UpdateSelection(newIndex);
        }
    }

    private void TryRevealSelected()
    {
        if (selectedIndex < 0 || selectedIndex >= cards.Count)
            return;

        FindCardCard current = cards[selectedIndex];
        if (current == null)
            return;

        if (current.IsMatched)
            return;

        if (firstRevealed != null && ReferenceEquals(firstRevealed, current))
            return;

        if (current.IsRevealed)
            return;

        current.Reveal();

        if (firstRevealed == null)
        {
            firstRevealed = current;
            return;
        }

        if (firstRevealed.PairId == current.PairId)
        {
            current.MarkMatched();
            firstRevealed.MarkMatched();
            matchedPairs++;
            firstRevealed = null;

            if (matchedPairs >= totalPairs && totalPairs > 0)
            {
                Complete(true);
            }
            else
            {
                MoveSelectionToNextAvailable();
            }
        }
        else
        {
            inputLocked = true;
            if (mismatchRoutine != null)
            {
                StopCoroutine(mismatchRoutine);
            }
            mismatchRoutine = StartCoroutine(HideMismatchDelayed(firstRevealed, current));
        }
    }

    private IEnumerator HideMismatchDelayed(FindCardCard first, FindCardCard second)
    {
        yield return new WaitForSeconds(mismatchHideDelay);

        if (first != null && !first.IsMatched)
        {
            first.Hide();
        }

        if (second != null && !second.IsMatched)
        {
            second.Hide();
        }

        firstRevealed = null;
        inputLocked = false;
        mismatchRoutine = null;
    }

    private void UpdateSelection(int newIndex)
    {
        if (newIndex < 0 || newIndex >= cards.Count)
            return;

        if (selectedIndex >= 0 && selectedIndex < cards.Count)
        {
            FindCardCard previous = cards[selectedIndex];
            previous?.SetSelected(false);
        }

        selectedIndex = newIndex;
        FindCardCard next = cards[selectedIndex];
        if (next != null)
        {
            next.SetSelected(true);
            if (!next.IsSelectable && !inputLocked)
            {
                MoveSelectionToNextAvailable();
            }
        }
    }

    private int MoveHorizontal(int direction)
    {
        if (cards.Count == 0)
            return selectedIndex;

        int current = Mathf.Clamp(selectedIndex, 0, cards.Count - 1);
        int candidate = current;

        while (true)
        {
            int next = candidate + direction;
            if (next < 0 || next >= cards.Count)
                break;

            candidate = next;
            if (IsSelectableIndex(candidate))
                return candidate;
        }

        return current;
    }

    private int MoveVertical(int direction)
    {
        if (cards.Count == 0)
            return selectedIndex;

        int current = Mathf.Clamp(selectedIndex, 0, cards.Count - 1);
        int candidate = current + (columns * direction);

        while (candidate >= 0 && candidate < cards.Count)
        {
            if (IsSelectableIndex(candidate))
                return candidate;

            candidate += columns * direction;
        }

        if (direction > 0)
        {
            int col = current % columns;
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (i % columns == col && IsSelectableIndex(i))
                    return i;
            }
            return current;
        }

        return current;
    }

    private int FindFirstSelectableIndex()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (IsSelectableIndex(i))
                return i;
        }
        return -1;
    }

    private int CalculateTotalPairs(out bool hasOddCount)
    {
        if (cards.Count == 0)
        {
            hasOddCount = false;
            return 0;
        }

        Dictionary<int, int> pairCounts = new Dictionary<int, int>();
        foreach (FindCardCard card in cards)
        {
            if (card == null)
                continue;

            if (!pairCounts.TryGetValue(card.PairId, out int count))
            {
                count = 0;
            }

            pairCounts[card.PairId] = count + 1;
        }

        int pairs = 0;
        hasOddCount = false;
        foreach (KeyValuePair<int, int> entry in pairCounts)
        {
            pairs += entry.Value / 2;
            if ((entry.Value % 2) != 0)
            {
                hasOddCount = true;
            }
        }

        return pairs;
    }

    private bool IsSelectableIndex(int index)
    {
        if (index < 0 || index >= cards.Count)
            return false;

        FindCardCard card = cards[index];
        return card != null && card.IsSelectable;
    }

    private void MoveSelectionToNextAvailable()
    {
        int nextIndex = FindNextSelectableClockwise();
        if (nextIndex >= 0 && nextIndex != selectedIndex)
        {
            UpdateSelection(nextIndex);
        }
    }

    private int FindNextSelectableClockwise()
    {
        if (cards.Count == 0)
            return -1;

        int start = Mathf.Clamp(selectedIndex, 0, cards.Count - 1);
        for (int offset = 1; offset < cards.Count; offset++)
        {
            int candidate = (start + offset) % cards.Count;
            if (IsSelectableIndex(candidate))
                return candidate;
        }

        return IsSelectableIndex(start) ? start : -1;
    }

    private int GuessColumnCount(GridLayoutGroup layout)
    {
        if (layout == null)
            return Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(cards.Count)));

        if (layout.constraint == GridLayoutGroup.Constraint.FixedColumnCount && layout.constraintCount > 0)
            return layout.constraintCount;

        if (layout.constraint == GridLayoutGroup.Constraint.FixedRowCount && layout.constraintCount > 0)
            return Mathf.CeilToInt((float)cards.Count / layout.constraintCount);

        return Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(cards.Count)));
    }
}
