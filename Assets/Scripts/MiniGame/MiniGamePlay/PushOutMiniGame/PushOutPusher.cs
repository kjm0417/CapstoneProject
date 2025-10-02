using UnityEngine;

/// <summary>
/// Push Out 미니게임에서 플레이어를 밀어내는 푸셔
/// - 지정된 방향으로 이동하며 플레이어와 충돌 시 임펄스를 가한다
/// </summary>
[DisallowMultipleComponent]
public class PushOutPusher : MonoBehaviour
{
    [Header("References")]
    [Tooltip("푸셔 Rigidbody2D (Kinematic 또는 Dynamic)")]
    [SerializeField] private Rigidbody2D body;
    [Tooltip("플레이어와의 충돌을 감지할 Collider2D (Trigger 권장)")]
    [SerializeField] private Collider2D hitCollider;

    [Header("Movement")]
    [Tooltip("이동 속도(초기값)")]
    [SerializeField] private float defaultSpeed = 3f;

    [Header("Physics")]
    [Tooltip("목표 속도를 유지하기 위한 가속력")]
    [SerializeField] private float driveForce = 6f;
    [Tooltip("푸셔가 유지할 최대 속도")]
    [SerializeField] private float maxVelocity = 6f;
    [Tooltip("플레이어에게 임펄스를 줄 때 반대 방향으로 받을 비율")]
    [SerializeField] private float counterImpulseMultiplier = 0.35f;

    private Vector2 _direction = Vector2.left;
    private float _speed;
    private float _impulse;
    private float _remainingLife;

    /// <summary>푸셔 초기화</summary>
    public void Configure(Vector2 direction, float speed, float impulse, float lifetime)
    {
        if (!body) body = GetComponent<Rigidbody2D>();
        if (!hitCollider) hitCollider = GetComponent<Collider2D>();

        _direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.left;
        _speed = speed > 0f ? speed : defaultSpeed;
        _impulse = impulse;
        _remainingLife = lifetime;

        if (body)
        {
            if (body.bodyType != RigidbodyType2D.Dynamic)
            {
                body.bodyType = RigidbodyType2D.Dynamic;
            }

            body.simulated = true;
            body.gravityScale = 0f;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.linearVelocity = _direction * _speed;
        }
    }

    private void FixedUpdate()
    {
        if (!body) return;

        _remainingLife -= Time.fixedDeltaTime;
        if (_remainingLife <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 desiredVelocity = _direction * _speed;
        Vector2 currentVelocity = body.linearVelocity;
        Vector2 velocityDelta = desiredVelocity - currentVelocity;

        if (driveForce > 0f)
        {
            body.AddForce(velocityDelta * driveForce, ForceMode2D.Force);
        }

        if (maxVelocity > 0f)
        {
            body.linearVelocity = Vector2.ClampMagnitude(body.linearVelocity, maxVelocity);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PushOutPlayer>();
        if (!player && other.attachedRigidbody)
        {
            player = other.attachedRigidbody.GetComponent<PushOutPlayer>();
        }

        ApplyPushToPlayer(player);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.collider.GetComponent<PushOutPlayer>();
        if (!player && collision.rigidbody)
        {
            player = collision.rigidbody.GetComponent<PushOutPlayer>();
        }

        ApplyPushToPlayer(player);
    }

    private void ApplyPushToPlayer(PushOutPlayer player)
    {
        if (player == null) return;

        player.ApplyImpulse(_direction * _impulse);

        if (!body || counterImpulseMultiplier <= 0f) return;
        if (body.bodyType != RigidbodyType2D.Dynamic) return;

        body.AddForce(-_direction * _impulse * counterImpulseMultiplier, ForceMode2D.Impulse);
    }
}
