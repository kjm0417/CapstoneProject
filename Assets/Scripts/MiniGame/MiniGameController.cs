using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 미니게임 컨트롤러 예시
/// PauseManager를 사용하여 미니게임 시작/종료 시 다른 오브젝트들을 일시정지/재개합니다
/// </summary>
public class MiniGameController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startMiniGameButton;
    [SerializeField] private Button endMiniGameButton;
    [SerializeField] private GameObject miniGameUI;
    
    [Header("Mini Game Settings")]
    [SerializeField] private float miniGameDuration = 10f;
    
    private float _currentGameTime;
    private bool _isMiniGameActive = false;

    private void Start()
    {
        // UI 버튼 이벤트 연결
        if (startMiniGameButton != null)
        {
            startMiniGameButton.onClick.AddListener(StartMiniGame);
        }
        
        if (endMiniGameButton != null)
        {
            endMiniGameButton.onClick.AddListener(EndMiniGame);
        }
        
        // 초기 상태 설정
        UpdateUI();
    }

    private void Update()
    {
        // 미니게임이 활성화되어 있고 시간 제한이 있는 경우
        if (_isMiniGameActive && miniGameDuration > 0)
        {
            _currentGameTime += Time.deltaTime;
            
            // 시간이 다 되면 자동으로 미니게임 종료
            if (_currentGameTime >= miniGameDuration)
            {
                EndMiniGame();
            }
        }
    }

    /// <summary>
    /// 미니게임을 시작합니다
    /// </summary>
    public void StartMiniGame()
    {
        if (!_isMiniGameActive)
        {
            _isMiniGameActive = true;
            _currentGameTime = 0f;
            
            // PauseManager를 통해 다른 모든 오브젝트 일시정지
            PauseManager.Instance.StartMiniGame();
            
            // 미니게임 UI 활성화
            if (miniGameUI != null)
            {
                miniGameUI.SetActive(true);
            }
            
            UpdateUI();
            
            Debug.Log("미니게임 시작!");
        }
    }

    /// <summary>
    /// 미니게임을 종료합니다
    /// </summary>
    public void EndMiniGame()
    {
        if (_isMiniGameActive)
        {
            _isMiniGameActive = false;
            
            // PauseManager를 통해 다른 모든 오브젝트 재개
            PauseManager.Instance.EndMiniGame();
            
            // 미니게임 UI 비활성화
            if (miniGameUI != null)
            {
                miniGameUI.SetActive(false);
            }
            
            UpdateUI();
            
            Debug.Log($"미니게임 종료! 플레이 시간: {_currentGameTime:F1}초");
        }
    }

    /// <summary>
    /// UI 상태를 업데이트합니다
    /// </summary>
    private void UpdateUI()
    {
        if (startMiniGameButton != null)
        {
            startMiniGameButton.interactable = !_isMiniGameActive;
        }
        
        if (endMiniGameButton != null)
        {
            endMiniGameButton.interactable = _isMiniGameActive;
        }
    }

    /// <summary>
    /// 현재 미니게임 상태를 반환합니다
    /// </summary>
    public bool IsActive => _isMiniGameActive;
    
    /// <summary>
    /// 현재 게임 시간을 반환합니다
    /// </summary>
    public float CurrentTime => _currentGameTime;
    
    /// <summary>
    /// 남은 시간을 반환합니다 (시간 제한이 있는 경우)
    /// </summary>
    public float RemainingTime => Mathf.Max(0f, miniGameDuration - _currentGameTime);
}