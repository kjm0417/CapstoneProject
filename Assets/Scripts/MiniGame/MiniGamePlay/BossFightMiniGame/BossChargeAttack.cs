using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보스 돌진 공격 - 플레이어의 마지막 X 위치를 기억하고 일직선으로 돌진
/// </summary>
public class BossChargeAttack : MonoBehaviour
{
    [Header("돌진 설정")]
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDelay = 0.5f; // 돌진 전 대기 시간
    [SerializeField] private float lifetime = 5f;
    
    [Header("시각 효과")]
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color chargeColor = Color.red;
    
    private RectTransform rectTransform;
    private Image image;
    private Vector2 targetPosition;
    private Vector2 startPosition;
    private bool isCharging = false;
    private float elapsedTime = 0f;
    private float chargeTimer = 0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    /// <summary>
    /// 돌진 공격 초기화
    /// </summary>
    /// <param name="playerYPosition">플레이어의 현재 Y 위치</param>
    /// <param name="fromLeft">왼쪽에서 돌진할지 여부</param>
    public void Initialize(float playerYPosition, bool fromLeft = true)
    {
        startPosition = rectTransform.anchoredPosition;
        
        // 플레이어의 Y 위치로 이동
        startPosition.y = playerYPosition;
        rectTransform.anchoredPosition = startPosition;
        
        // 목표 위치 설정 (화면 반대편)
        RectTransform playArea = rectTransform.parent as RectTransform;
        float playAreaWidth = playArea.rect.width;
        
        if (fromLeft)
        {
            // 왼쪽에서 시작 → 오른쪽으로 돌진
            startPosition.x = -playAreaWidth / 2 - 100;
            targetPosition = new Vector2(playAreaWidth / 2 + 100, playerYPosition);
        }
        else
        {
            // 오른쪽에서 시작 → 왼쪽으로 돌진
            startPosition.x = playAreaWidth / 2 + 100;
            targetPosition = new Vector2(-playAreaWidth / 2 - 100, playerYPosition);
        }
        
        rectTransform.anchoredPosition = startPosition;
        
        // 경고 색상으로 시작
        if (image != null)
        {
            image.color = warningColor;
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        
        // 수명 종료
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }
        
        // 돌진 대기 중
        if (!isCharging)
        {
            chargeTimer += Time.deltaTime;
            
            // 깜빡임 효과
            if (image != null)
            {
                float alpha = Mathf.PingPong(chargeTimer * 4f, 1f);
                Color c = warningColor;
                c.a = alpha;
                image.color = c;
            }
            
            // 대기 시간 종료 → 돌진 시작
            if (chargeTimer >= chargeDelay)
            {
                isCharging = true;
                if (image != null)
                {
                    image.color = chargeColor;
                }
            }
        }
        else
        {
            // 돌진 중
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                targetPosition,
                chargeSpeed * 100f * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어와 충돌 처리는 BossFightMiniGame에서 처리
            Debug.Log("[BossChargeAttack] 플레이어 충돌!");
        }
    }
}
