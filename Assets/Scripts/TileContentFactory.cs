using UnityEngine;

public class TileContentFactory : MonoBehaviour
{
    [SerializeField] private TileContent _redCastlePrefab;
    [SerializeField] private TileContent _blueCastlePrefab;
    [SerializeField] private TileContent _redWariorPrefab;
    [SerializeField] private TileContent _blueWariorPrefab;
    [SerializeField] private TileContent _treePrefab;

    public TileContent GetPlayerContent(TileContentType tileType, PlayerColor player)
    {
        if (tileType == TileContentType.Castle)
        {
            if (player == PlayerColor.Red)
            {
                return Instantiate(_redCastlePrefab);
            }
            else if (player == PlayerColor.Blue)
            {
                return Instantiate(_blueCastlePrefab);
            }
            else
            {
                Debug.LogError("No prefab for this type of content!");
            }
        }
        else if (tileType == TileContentType.Unit)
        {
            if (player == PlayerColor.Red)
            {
                return Instantiate(_redWariorPrefab);
            }
            else if (player == PlayerColor.Blue)
            {
                return Instantiate(_blueWariorPrefab);
            }
            else
            {
                Debug.LogError("No prefab for this type of content!");
            }
        }
        else
        {
            Debug.LogError("No prefab for this type of content!");
        }
        return null;
    }

    public TileContent GetObstacle()
    {
        return Instantiate(_treePrefab);
    }


}
