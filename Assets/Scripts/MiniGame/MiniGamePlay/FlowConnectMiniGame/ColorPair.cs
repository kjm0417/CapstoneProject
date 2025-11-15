using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 연결해야 할 색상 쌍의 정보와 현재 경로 상태를 관리
/// </summary>
public class ColorPair
{
    public int colorId;                     // 고유 색상 식별자 (0, 1, 2, ...)
    public Vector2Int pointA;               // 연결해야 할 첫 번째 점의 위치
    public Vector2Int pointB;               // 연결해야 할 두 번째 점의 위치
    public List<Vector2Int> path;           // 현재 그려진 경로
    public bool isConnected;                // 완전히 연결되었는지 여부

    public ColorPair(int id, Vector2Int a, Vector2Int b)
    {
        colorId = id;
        pointA = a;
        pointB = b;
        path = new List<Vector2Int>(50); // 용량 미리 할당
        isConnected = false;
    }
}
