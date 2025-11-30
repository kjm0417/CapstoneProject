using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IPausable 오브젝트들을 관리하는 싱글톤 매니저
/// 미니게임 중이거나 특정 상황에서 게임을 일시정지할 수 있습니다
/// </summary>
public class PauseManager : SingleTonGeneric<PauseManager>
{

    [Header("일시정지 상태")]
    [SerializeField] private bool _isGamePaused = false;
    [SerializeField] private bool _isMiniGameActive = false;

    // IPausable 오브젝트들을 관리하는 리스트
    private List<IPausable> _pausableObjects = new List<IPausable>();

    public bool IsGamePaused => _isGamePaused;
    public bool IsMiniGameActive => _isMiniGameActive;

    /// <summary>
    /// IPausable 오브젝트를 등록합니다
    /// </summary>
    public void RegisterPausable(IPausable pausable)
    {
        if (pausable != null && !_pausableObjects.Contains(pausable))
        {
            _pausableObjects.Add(pausable);
        }
    }

    /// <summary>
    /// IPausable 오브젝트를 등록 해제합니다
    /// </summary>
    public void UnregisterPausable(IPausable pausable)
    {
        if (pausable != null && _pausableObjects.Contains(pausable))
        {
            _pausableObjects.Remove(pausable);
        }
    }

    /// <summary>
    /// 미니게임을 시작하고 등록된 모든 오브젝트를 일시정지합니다
    /// </summary>
    public void StartMiniGame()
    {
        if (!_isMiniGameActive)
        {
            _isMiniGameActive = true;
            PauseAllObjects();
        }
    }

    /// <summary>
    /// 미니게임을 종료하고 등록된 모든 오브젝트의 일시정지를 해제합니다
    /// </summary>
    public void EndMiniGame()
    {
        if (_isMiniGameActive)
        {
            _isMiniGameActive = false;
            ResumeAllObjects();
        }
    }

    /// <summary>
    /// 게임을 일시정지합니다 (미니게임과 별개)
    /// </summary>
    public void PauseGame()
    {
        if (!_isGamePaused)
        {
            _isGamePaused = true;
            PauseAllObjects();
        }
    }

    /// <summary>
    /// 게임 일시정지를 해제합니다
    /// </summary>
    public void ResumeGame()
    {
        if (_isGamePaused)
        {
            _isGamePaused = false;
            // 미니게임이 활성화되어 있지 않을 때만 재개
            if (!_isMiniGameActive)
            {
                ResumeAllObjects();
            }
        }
    }

    /// <summary>
    /// 등록된 모든 오브젝트를 일시정지합니다
    /// </summary>
    private void PauseAllObjects()
    {
        foreach (var pausable in _pausableObjects)
        {
            if (pausable != null)
            {
                pausable.Pause();
            }
        }
    }

    /// <summary>
    /// 등록된 모든 오브젝트의 일시정지를 해제합니다
    /// </summary>
    private void ResumeAllObjects()
    {
        foreach (var pausable in _pausableObjects)
        {
            if (pausable != null)
            {
                pausable.Resume();
            }
        }
    }

    /// <summary>
    /// 현재 일시정지 상태인지 확인합니다 (게임 일시정지 또는 미니게임 활성화)
    /// </summary>
    public bool ShouldBePaused()
    {
        return _isGamePaused || _isMiniGameActive;
    }

    /// <summary>
    /// 등록된 Pausable 오브젝트 수를 반환합니다
    /// </summary>
    public int GetPausableCount()
    {
        return _pausableObjects.Count;
    }
}