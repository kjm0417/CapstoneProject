using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileData", menuName = "Game/TileDataBase")]
public class TileDataBase : ScriptableObject
{
    [Tooltip("Tile ID(index) 기준으로 매핑되는 타일 리스트")]
    public TileBase[] tiles;

    /// <summary>
    /// 타일 ID를 통해, ID가 어떤 타일인지 반환
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public TileBase GetTile(int id)
    {
        if (id < 0 || id >= tiles.Length)
            return null;
        return tiles[id];
    }

    /// <summary>
    /// 타일을 통해 어떤 ID를 갖고 있는지 반환
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public int GetTileId(TileBase tile)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == tile)
                return i;
        }
        return -1;
    }
}
