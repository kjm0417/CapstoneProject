using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방향키와 엔터 입력으로 카드 짝을 맞추는 미니게임.
/// GridLayoutGroup 내 FindCardCard들을 자동 수집하여,
/// 키보드 이동, 선택, 짝 확인 및 게임 클리어 상태를 관리한다.
/// 시작 시 카드 순서를 셔플(섞기)하여 배치한다.
/// </summary>
public class FindCardMiniGame : MiniGameBase
{
    // 카드 배치 관련 설정
    [Header("Card Layout")]
    [SerializeField] private GridLayoutGroup cardContainer;
    [Tooltip("가로로 이동할 때 사용할 열 수. 0이면 GridLayoutGroup 설정을 기반으로 추정")]
    [SerializeField] private int columns = 4;

    // 시각적 설정
    [Header("Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(0.2f, 0.55f, 1f, 1f);
    [SerializeField] private Color matchedColor = new Color(0.65f, 0.65f, 0.65f, 1f);
    [SerializeField] private float mismatchHideDelay = 0.75f; // 틀린 카드 숨기기 딜레이

    // 내부 상태
    private readonly List<FindCardCard> cards = new List<FindCardCard>();
    private FindCardCard firstRevealed;      // 첫 번째 뒤집힌 카드
    private Coroutine mismatchRoutine;       // 틀린 카드 숨기기 코루틴
    private int selectedIndex = -1;          // 현재 선택된 카드 인덱스
    private int matchedPairs;                // 맞춘 짝 개수
    private int totalPairs;                  // 전체 짝 개수
    private bool inputLocked;                // 입력 잠금 여부(틀린 짝일 때 잠금)

    /// <summary>
    /// 게임 시작 시 초기화.
    /// - 카드 수집 → 총 짝 수 계산 → 카드 상태 리셋 → **셔플 후 Transform 순서 재배치** → 첫 선택 설정
    /// </summary>
    protected override void OnStartHook()
    {
        base.OnStartHook();

        // 이전 코루틴 정리
        if (mismatchRoutine != null)
        {
            StopCoroutine(mismatchRoutine);
            mismatchRoutine = null;
        }

        // 카드 컨테이너 자동 탐색
        if (cardContainer == null)
        {
            cardContainer = GetComponentInChildren<GridLayoutGroup>(includeInactive: true);
        }

        // 카드 목록 초기화 및 수집
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

        // 카드가 없는 경우 게임 중단
        if (cards.Count == 0)
        {
            Debug.LogError("[FindCardMiniGame] 카드가 존재하지 않습니다. GridLayoutGroup 하위에 FindCardCard를 배치하세요.");
            Abort("NO_CARDS");
            return;
        }

        // 열 수 추정
        if (columns <= 0)
        {
            columns = GuessColumnCount(cardContainer);
        }
        columns = Mathf.Max(1, columns);

        // 총 짝 수 계산
        totalPairs = CalculateTotalPairs(out bool hasOddCount);
        if (totalPairs <= 0)
        {
            Debug.LogError("[FindCardMiniGame] 올바른 짝 정보를 가진 카드가 없습니다.");
            Abort("NO_PAIR_DATA");
            return;
        }

        // 홀수 개 카드 경고
        if (hasOddCount)
        {
            Debug.LogWarning("[FindCardMiniGame] 일부 카드가 짝이 없이 남을 수 있습니다. pairId 구성을 확인하세요.");
        }

        // 초기 상태 리셋
        firstRevealed = null;
        matchedPairs = 0;
        inputLocked = false;

        // 카드 비주얼 테마/상태 리셋
        foreach (FindCardCard card in cards)
        {
            if (card == null)
                continue;

            card.ApplyTheme(normalColor, highlightColor, matchedColor);
            card.ResetForGame();
        }

        // ? 시작 시 카드 순서 셔플 + Transform 순서 반영(화면 배치가 실제로 섞이도록)
        ShuffleCards();
        ApplySiblingOrderToMatchList();

        // 첫 번째 선택 가능한 카드로 포커스 이동
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

    /// <summary>
    /// 매 프레임마다 호출(입력 처리).
    /// - 방향키 이동, 엔터로 뒤집기 시도
    /// </summary>
    protected override void OnUpdateHook(float deltaTime)
    {
        base.OnUpdateHook(deltaTime);

        if (!IsRunning || cards.Count == 0)
            return;

        if (inputLocked)
            return;

        HandleMoveInput(); // 방향키 이동

        // 엔터 입력 시 카드 뒤집기 시도
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            TryRevealSelected();
        }
    }

    /// <summary>
    /// 게임 종료/리셋 시 정리.
    /// </summary>
    protected override void OnCleanupHook()
    {
        base.OnCleanupHook();

        // 코루틴 정리
        if (mismatchRoutine != null)
        {
            StopCoroutine(mismatchRoutine);
            mismatchRoutine = null;
        }

        // 카드 선택 해제
        foreach (FindCardCard card in cards)
        {
            card?.SetSelected(false);
        }

        // 상태 초기화
        firstRevealed = null;
        matchedPairs = 0;
        totalPairs = 0;
        selectedIndex = -1;
        inputLocked = false;
    }

    /// <summary>
    /// 클리어 여부 실시간 판정.
    /// </summary>
    protected override bool CheckClearRealtime()
    {
        if (!IsRunning)
            return false;

        return totalPairs > 0 && matchedPairs >= totalPairs;
    }

    /// <summary>
    /// 방향키 입력 처리(좌/우/상/하 이동).
    /// </summary>
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

    /// <summary>
    /// 현재 선택된 카드를 뒤집고, 두 장이면 짝 검증까지 수행.
    /// </summary>
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

        // 카드 뒤집기
        current.Reveal();

        // 첫 선택이면 저장하고 종료
        if (firstRevealed == null)
        {
            firstRevealed = current;
            return;
        }

        // 두 번째 카드일 경우 짝 비교
        if (firstRevealed.PairId == current.PairId)
        {
            // 짝 성공
            current.MarkMatched();
            firstRevealed.MarkMatched();
            matchedPairs++;
            firstRevealed = null;

            // 모든 짝을 맞추면 클리어
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
            // 틀림: 잠시 보여주고 숨기기
            inputLocked = true;
            if (mismatchRoutine != null)
            {
                StopCoroutine(mismatchRoutine);
            }
            mismatchRoutine = StartCoroutine(HideMismatchDelayed(firstRevealed, current));
            PlayerManager.Instance.AddConcentration(-1f);
            PlayerManager.Instance.AddConcentration(-2f);
        }
    }

    /// <summary>
    /// 틀린 두 장을 일정 시간 후 숨긴다.
    /// </summary>
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

    /// <summary>
    /// 카드 선택 시각 효과 갱신(이전 해제 → 신규 선택 적용).
    /// 선택 불가 카드인 경우, 다음 가능한 카드로 이동.
    /// </summary>
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

    /// <summary>
    /// 가로(좌/우) 이동 처리.
    /// </summary>
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

    /// <summary>
    /// 세로(상/하) 이동 처리.
    /// </summary>
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

    /// <summary>
    /// 첫 선택 가능한 카드 인덱스 찾기.
    /// </summary>
    private int FindFirstSelectableIndex()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (IsSelectableIndex(i))
                return i;
        }
        return -1;
    }

    /// <summary>
    /// pairId 갯수를 세어 전체 짝 개수 계산. 홀수 카드 존재 여부도 반환.
    /// </summary>
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

    /// <summary>
    /// 해당 인덱스의 카드가 선택 가능한지 확인.
    /// </summary>
    private bool IsSelectableIndex(int index)
    {
        if (index < 0 || index >= cards.Count)
            return false;

        FindCardCard card = cards[index];
        return card != null && card.IsSelectable;
    }

    /// <summary>
    /// 다음 선택 가능한 카드로 포커스 이동.
    /// </summary>
    private void MoveSelectionToNextAvailable()
    {
        int nextIndex = FindNextSelectableClockwise();
        if (nextIndex >= 0 && nextIndex != selectedIndex)
        {
            UpdateSelection(nextIndex);
        }
    }

    /// <summary>
    /// 시계 방향으로 다음 선택 가능한 카드 찾기.
    /// </summary>
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

    /// <summary>
    /// GridLayoutGroup 기반으로 열 수 추정.
    /// </summary>
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

    /// <summary>
    /// ?? 카드 리스트를 Fisher?Yates 방식으로 셔플한다(메모리상 순서만 변경).
    /// - 짝 정보(pairId)는 그대로 유지되며, 화면 배치는 아직 바뀌지 않는다.
    /// </summary>
    private void ShuffleCards()
    {
        // i부터 끝까지 중 임의 인덱스와 스왑
        for (int i = 0; i < cards.Count; i++)
        {
            int r = Random.Range(i, cards.Count);
            FindCardCard temp = cards[i];
            cards[i] = cards[r];
            cards[r] = temp;
        }
    }

    /// <summary>
    /// ?? 셔플된 리스트 순서를 실제 Transform 순서(형제 인덱스)로 반영한다.
    /// - GridLayoutGroup은 자식(Transform) 순서대로 그리므로 이 단계가 있어야 화면에서 섞여 보인다.
    /// </summary>
    private void ApplySiblingOrderToMatchList()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                cards[i].transform.SetSiblingIndex(i);
            }
        }

        // 레이아웃 강제 갱신(선택사항: 프레임 지연 없이 즉시 반영하고 싶을 때)
        if (cardContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer.GetComponent<RectTransform>());
        }
    }
}
