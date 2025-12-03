using System;
using UnityEngine;

/// <summary>
/// 보스 공격 패턴 베이스 클래스
/// 각 패턴은 독립적인 프리팹으로 존재하며 일정 시간 동안 실행 후 종료
/// </summary>
public abstract class BossPatternBase : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] protected GameObject attackPrefab;
    [SerializeField] protected float patternDuration = 10f; // 패턴 지속 시간
    [SerializeField] protected float spawnInterval = 2f;
    [SerializeField] protected float timePenalty = 5f;
    
    protected RectTransform playArea;
    protected RectTransform player;
    protected float spawnTimer = 0f;
    protected float durationTimer = 0f;
    protected bool isActive = false;

    public event Action<BossPatternBase> OnPatternComplete;

    /// <summary>
    /// 패턴 초기화 및 시작
    /// </summary>
    public virtual void Initialize(RectTransform playArea, RectTransform player)
    {
        this.playArea = playArea;
        this.player = player;
        spawnTimer = 0f;
        durationTimer = 0f;
        isActive = true;
        
        OnPatternStart();
    }

    /// <summary>
    /// 패턴 시작 시 호출 (오버라이드 가능)
    /// </summary>
    protected virtual void OnPatternStart()
    {
    }

    protected virtual void Update()
    {
        if (!isActive) return;

        durationTimer += Time.deltaTime;
        spawnTimer += Time.deltaTime;
        
        // 패턴 지속 시간 종료
        if (durationTimer >= patternDuration)
        {
            CompletePattern();
            return;
        }
        
        // 공격 스폰
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            ExecutePattern();
        }
    }

    /// <summary>
    /// 패턴 실행 (각 패턴에서 구현)
    /// </summary>
    protected abstract void ExecutePattern();

    /// <summary>
    /// 패턴 완료
    /// </summary>
    protected virtual void CompletePattern()
    {
        isActive = false;
        OnPatternComplete?.Invoke(this);
        Destroy(gameObject, 0.5f); // 약간의 딜레이 후 제거
    }

    /// <summary>
    /// 공격 오브젝트 생성 헬퍼
    /// </summary>
    protected GameObject SpawnAttack(Vector2 position)
    {
        if (attackPrefab == null || playArea == null) return null;

        GameObject attackObj = Instantiate(attackPrefab, playArea);
        RectTransform attackRect = attackObj.GetComponent<RectTransform>();
        
        if (attackRect != null)
        {
            attackRect.anchoredPosition = position;
        }

        BossAttackObject attackComponent = attackObj.GetComponent<BossAttackObject>();
        if (attackComponent != null)
        {
            attackComponent.timePenalty = timePenalty;
        }

        return attackObj;
    }

    protected Vector2 GetPlayerPosition()
    {
        return player != null ? player.anchoredPosition : Vector2.zero;
    }

    protected Vector2 GetPlayAreaSize()
    {
        return playArea != null ? playArea.rect.size : Vector2.zero;
    }
}
