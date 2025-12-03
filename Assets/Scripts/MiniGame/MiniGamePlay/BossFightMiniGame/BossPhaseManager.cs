using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스 페이즈 관리자
/// 각 페이즈의 패턴 프리팹을 관리하고 랜덤으로 실행
/// </summary>
[System.Serializable]
public class BossPhaseData
{
    [Header("페이즈 정보")]
    public string phaseName = "Phase 1";
    public float startTime = 0f;
    public float endTime = 20f;
    
    [Header("패턴 프리팹 목록")]
    [Tooltip("이 페이즈에서 사용할 패턴 프리팹들")]
    public List<GameObject> patternPrefabs = new List<GameObject>();
}

public class BossPhaseManager : MonoBehaviour
{
    [Header("페이즈 설정")]
    [SerializeField] private List<BossPhaseData> phases = new List<BossPhaseData>();
    
    [Header("참조")]
    [SerializeField] private RectTransform playArea;
    [SerializeField] private RectTransform player;
    [SerializeField] private Transform patternContainer; // 패턴이 생성될 부모
    
    private int currentPhaseIndex = -1;
    private BossPatternBase currentPattern;
    private float gameTime = 0f;
    private bool isRunning = false;

    public void StartBoss()
    {
        isRunning = true;
        gameTime = 0f;
        currentPhaseIndex = -1;
        
        if (patternContainer == null)
        {
            patternContainer = transform;
        }
    }

    public void StopBoss()
    {
        isRunning = false;
        
        if (currentPattern != null)
        {
            Destroy(currentPattern.gameObject);
            currentPattern = null;
        }
    }

    private void Update()
    {
        if (!isRunning) return;

        gameTime += Time.deltaTime;
        
        // 페이즈 체크
        int newPhaseIndex = GetCurrentPhaseIndex();
        
        if (newPhaseIndex != currentPhaseIndex)
        {
            currentPhaseIndex = newPhaseIndex;
            OnPhaseChanged(currentPhaseIndex);
        }
        
        // 현재 패턴이 없으면 새 패턴 시작
        if (currentPattern == null && currentPhaseIndex >= 0)
        {
            StartRandomPattern();
        }
    }

    private int GetCurrentPhaseIndex()
    {
        for (int i = 0; i < phases.Count; i++)
        {
            if (gameTime >= phases[i].startTime && gameTime < phases[i].endTime)
            {
                return i;
            }
        }
        return -1;
    }

    private void OnPhaseChanged(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= phases.Count) return;
        
        Debug.Log($"[BossPhaseManager] {phases[phaseIndex].phaseName} 시작!");
        
        // 이전 패턴 정리
        if (currentPattern != null)
        {
            Destroy(currentPattern.gameObject);
            currentPattern = null;
        }
    }

    private void StartRandomPattern()
    {
        if (currentPhaseIndex < 0 || currentPhaseIndex >= phases.Count) return;
        
        BossPhaseData currentPhase = phases[currentPhaseIndex];
        
        if (currentPhase.patternPrefabs == null || currentPhase.patternPrefabs.Count == 0)
        {
            Debug.LogWarning($"[BossPhaseManager] {currentPhase.phaseName}에 패턴이 없습니다!");
            return;
        }
        
        // 랜덤 패턴 선택
        int randomIndex = Random.Range(0, currentPhase.patternPrefabs.Count);
        GameObject patternPrefab = currentPhase.patternPrefabs[randomIndex];
        
        if (patternPrefab == null)
        {
            Debug.LogWarning("[BossPhaseManager] 패턴 프리팹이 null입니다!");
            return;
        }
        
        // 패턴 생성
        GameObject patternObj = Instantiate(patternPrefab, patternContainer);
        currentPattern = patternObj.GetComponent<BossPatternBase>();
        
        if (currentPattern != null)
        {
            currentPattern.Initialize(playArea, player);
            currentPattern.OnPatternComplete += OnPatternComplete;
            
            Debug.Log($"[BossPhaseManager] 패턴 시작: {patternPrefab.name}");
        }
        else
        {
            Debug.LogError($"[BossPhaseManager] {patternPrefab.name}에 BossPatternBase가 없습니다!");
            Destroy(patternObj);
        }
    }

    private void OnPatternComplete(BossPatternBase pattern)
    {
        Debug.Log($"[BossPhaseManager] 패턴 완료: {pattern.name}");
        
        pattern.OnPatternComplete -= OnPatternComplete;
        currentPattern = null;
        
        // 다음 패턴은 Update에서 자동으로 시작됨
    }

    // 디버그용
    private void OnGUI()
    {
        if (!isRunning) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label($"Game Time: {gameTime:F1}s");
        
        if (currentPhaseIndex >= 0 && currentPhaseIndex < phases.Count)
        {
            GUILayout.Label($"Phase: {phases[currentPhaseIndex].phaseName}");
        }
        
        if (currentPattern != null)
        {
            GUILayout.Label($"Pattern: {currentPattern.name}");
        }
        
        GUILayout.EndArea();
    }
}
