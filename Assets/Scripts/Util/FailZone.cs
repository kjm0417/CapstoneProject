#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class FailZone : MonoBehaviour
{
    [SerializeField] private float failRadius = 4f;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 3D 환경이라면 WireSphere, 2D 탑다운이라면 WireDisc가 보기 편합니다.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, failRadius);

        Handles.color = new Color(1f, 0f, 0f, 0.1f);
        Handles.DrawWireDisc(transform.position, Vector3.forward, failRadius);
    }
#endif
}
