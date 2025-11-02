using UnityEngine;

/// <summary>
/// PauseManager 기능을 테스트하기 위한 스크립트
/// 키보드 입력으로 일시정지 기능을 테스트할 수 있습니다
/// </summary>
public class PauseManagerTester : MonoBehaviour
{
    [Header("Test Keys")]
    [SerializeField] private KeyCode pauseGameKey = KeyCode.P;
    [SerializeField] private KeyCode startMiniGameKey = KeyCode.M;
    [SerializeField] private KeyCode endMiniGameKey = KeyCode.N;
    
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // 게임 일시정지/재개
        if (Input.GetKeyDown(pauseGameKey))
        {
            if (PauseManager.Instance.IsGamePaused)
            {
                PauseManager.Instance.ResumeGame();
            }
            else
            {
                PauseManager.Instance.PauseGame();
            }
        }
        
        // 미니게임 시작
        if (Input.GetKeyDown(startMiniGameKey))
        {
            PauseManager.Instance.StartMiniGame();
        }
        
        // 미니게임 종료
        if (Input.GetKeyDown(endMiniGameKey))
        {
            PauseManager.Instance.EndMiniGame();
        }
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== Pause Manager Tester ===");
        GUILayout.Label($"게임 일시정지: {PauseManager.Instance.IsGamePaused}");
        GUILayout.Label($"미니게임 활성화: {PauseManager.Instance.IsMiniGameActive}");
        GUILayout.Label($"등록된 오브젝트 수: {PauseManager.Instance.GetPausableCount()}");
        GUILayout.Label("");
        GUILayout.Label("조작법:");
        GUILayout.Label($"[{pauseGameKey}] - 게임 일시정지/재개");
        GUILayout.Label($"[{startMiniGameKey}] - 미니게임 시작");
        GUILayout.Label($"[{endMiniGameKey}] - 미니게임 종료");
        GUILayout.EndArea();
    }
}