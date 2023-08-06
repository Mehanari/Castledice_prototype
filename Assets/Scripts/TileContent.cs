using UnityEngine;

public class TileContent : MonoBehaviour
{
    public TileContentType Type => _type;
    public PlayerColor Player => _player;
    public int ClashCost => _clashCost;

    [SerializeField] private TileContentType _type;
    [SerializeField] private int _clashCost;
    [SerializeField] private PlayerColor _player;
}
