using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private float _tileWidth;
    [SerializeField] private float _tileHeight;
    [SerializeField] private int _boardSize;
    [SerializeField] private Vector3 _firstTilePosition;
    private Tile[,] _tiles;

    public Tile[,] Tiles => _tiles;
    public int Size => _boardSize;

    public void InitializeBoard()
    {
        GenerateTiles();
        SetTilesNeighbours();
    }

    public List<Tile> GetOccupiedTiles(PlayerColor player)
    {
        List<Tile> occupiedTiles = new List<Tile>();
        for (int i = 0; i < _tiles.GetLength(0); i++)
        {
            for (int j = 0; j < _tiles.GetLength(1); j++)
            {
                var tile = _tiles[i, j];
                if (tile.Content != null && tile.Content.Player == player)
                {
                    occupiedTiles.Add(tile);
                }
            }
        }
        return occupiedTiles;
    }

    public Tile GetTile(Ray ray)
    {
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit)
        {
            var tile = hit.transform.GetComponent<Tile>();
            return tile;
        }
        return null;
    }

    private void GenerateTiles()
    {
        _tiles = new Tile[_boardSize, _boardSize];
        for (int i = 0; i < _boardSize; i++)
        {
            Vector3 tilePosition = _firstTilePosition - i * new Vector3(-_tileWidth, _tileHeight, 0);
            for (int j = 0; j < _boardSize; j++)
            {
                var tile = Instantiate(_tilePrefab);
                tile.transform.position = tilePosition;
                _tiles[i, j] = tile;

                tilePosition -= new Vector3(_tileWidth, _tileHeight, 0);
            }
        }
    }

    private void SetTilesNeighbours()
    {
        for (int i = 0; i < _boardSize; i++)
        {
            for (int j = 0; j < _boardSize; j++)
            {
                _tiles[i, j].SetNeighbors(GetTileNeighbours(i, j));
            }
        }
    }

    private List<Tile> GetTileNeighbours(int tileX, int tileY)
    {
        List<Tile> neigbours = new List<Tile>();
        for (int neighbourX = tileX - 1; neighbourX <= tileX + 1; neighbourX++)
        {
            for (int neighbourY = tileY - 1; neighbourY <= tileY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < _tiles.GetLength(0) &&
                    neighbourY >= 0 && neighbourY < _tiles.GetLength(1))
                {
                    neigbours.Add(_tiles[neighbourX, neighbourY]);
                }
            }
        }
        return neigbours;
    }


}
