using UnityEngine;

/// <summary>
/// 일시정지 기능을 구현할 수 있는 인터페이스
/// </summary>
public interface IPausable
{
    /// <summary>
    /// 오브젝트를 일시정지 상태로 만듭니다
    /// </summary>
    void Pause();
    
    /// <summary>
    /// 오브젝트의 일시정지를 해제합니다
    /// </summary>
    void Resume();
    
    /// <summary>
    /// 현재 일시정지 상태인지 확인합니다
    /// </summary>
    bool IsPaused { get; }
}