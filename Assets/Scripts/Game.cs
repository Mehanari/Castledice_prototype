using System;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;

public enum GameState
{
    BlueMove,
    RedMove,
    BlueWon,
    RedWon
}

public class Game : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private GameState _initialState;
    [SerializeField] private int _treesCount;

    [Header("Castles placements")]
    [SerializeField] private Vector2Int _redCastlePosition;
    [SerializeField] private Vector2Int _blueCastlePosition;

    [Space]
    [SerializeField] private GameBoard _board;
    [SerializeField] private TileContentFactory _contentFactory;

    [Space]
    [Header("UI")]
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private TextMeshProUGUI _gameOverText;
    [SerializeField] private GameObject _gameUI;
    [SerializeField] private TextMeshProUGUI _moveText;
    [SerializeField] private TextMeshProUGUI _actionPointsText;
 
    private GameState _currentState;
    private List<Tile> _moveTiles = new List<Tile>();
    private List<Tile> _clashTiles = new List<Tile>();

    private HashSet<Tile> _blueUnitsTiles = new HashSet<Tile>();
    private HashSet<Tile> _redUnitsTiles = new HashSet<Tile>();
    private int _currentPlayerActionPoints;


    #region Move processing methods

    public void Click(Vector3 screenClickPosition)
    {
        Ray ray = _camera.ScreenPointToRay(screenClickPosition);
        var tile = _board.GetTile(ray);
        if (tile == null)
        {
            return;
        }

        bool isMoveTile = _moveTiles.Contains(tile);
        bool isClashTile = _clashTiles.Contains(tile);

        if (!isMoveTile && !isClashTile)
        {
            return;
        }

        var army = GetCurrentPlayerUnitTiles();


        if (isMoveTile)
        {
            _currentPlayerActionPoints--;
            tile.PlaceContent(GetCurrentPlayerUnit());
            army.Add(tile);
        }
        else if (isClashTile)
        {
            if (ProcessClash(tile))
            {
                army.Add(tile);
            }
        }

        KillUnitsNotConnectedToCastles(PlayerColor.Red);
        KillUnitsNotConnectedToCastles(PlayerColor.Blue);

        if (_currentState != GameState.RedWon && _currentState != GameState.BlueWon)
        {
            PrepareNextMove();
        }
        else
        {
            ProcessWin();
        }
    }

    private bool ProcessClash(Tile tile)
    {
        if (tile.Content.ClashCost > _currentPlayerActionPoints)
        {
            return false;
        }

        _currentPlayerActionPoints -= tile.Content.ClashCost;
        if (tile.Content.Type == TileContentType.Castle)
        {
            if (_currentState == GameState.BlueMove)
            {
                _currentState = GameState.BlueWon;
            }
            else if (_currentState == GameState.RedMove)
            {
                _currentState = GameState.RedWon;
            }
        }
        KillUnitOnTile(tile);
        tile.PlaceContent(GetCurrentPlayerUnit());
        return true;
    }

    private void ProcessWin()
    {
        SetAllTilesViewToDefault();
        _gameOverUI.SetActive(true);
        _gameUI.SetActive(false);
        if (_currentState == GameState.BlueWon)
        {
            _gameOverText.text = "BLUE PLAYER WON";

        }
        else if (_currentState == GameState.RedWon)
        {
            _gameOverText.text = "RED PLAYER WON";
        }
    }

    private void UpdateGameUI()
    {
        if (_currentState == GameState.BlueMove)
        {
            _moveText.text = "BLUE PLAYER MOVE";
        }
        else if (_currentState == GameState.RedMove)
        {
            _moveText.text = "RED PLAYER MOVE";
        }

        _actionPointsText.text = "ACTION POINTS: " + _currentPlayerActionPoints;
    }

    private void SetAllTilesViewToDefault()
    {
        for (int i = 0; i < _board.Size; i++)
        {
            for (int j = 0; j < _board.Size; j++)
            {
                _board.Tiles[i, j].SetDefaultView();
            }
        }
    }

    public void ResetGame()
    {
        ClearBoard();
        PlaceCastles();
        PlaceTrees();
        _blueUnitsTiles.Clear();
        _redUnitsTiles.Clear();
        _currentState = _initialState;
        _currentPlayerActionPoints = UnityEngine.Random.Range(1, 7);
        _gameUI.SetActive(true);
        _gameOverUI.SetActive(false);
        PrepareNextMove();
    }

    private void Start()
    {
        _board.InitializeBoard();
        PlaceCastles();
        PlaceTrees();
        _currentState = _initialState;
        _currentPlayerActionPoints = UnityEngine.Random.Range(1, 7);
        PrepareNextMove();
    }

    private void PrepareNextMove()
    {
        _moveTiles.Clear();
        _clashTiles.Clear();

        if (_currentPlayerActionPoints <= 0)
        {
            SwitchStateToNextPlayerMove();
            _currentPlayerActionPoints = UnityEngine.Random.Range(1, 7);
        }

        if (_currentState == GameState.BlueMove)
        {
            FindPossibleMovesForPlayer(PlayerColor.Blue);
        }
        else
        {
            FindPossibleMovesForPlayer(PlayerColor.Red);
        }
        UpdateGameUI();
        ShowMoves();
    }

    private void ClearBoard()
    {
        for (int i = 0; i < _board.Size; i++)
        {
            for (int j = 0; j < _board.Size; j++)
            {
                var tile = _board.Tiles[i, j];
                tile.RemoveContent();
                tile.SetDefaultView();
            }
        }
    }

    private void PlaceCastles()
    {
        var redCastle = _contentFactory.GetPlayerContent(TileContentType.Castle, PlayerColor.Red);
        var blueCastle = _contentFactory.GetPlayerContent(TileContentType.Castle, PlayerColor.Blue);

        _board.Tiles[_redCastlePosition.x, _redCastlePosition.y].PlaceContent(redCastle);
        _board.Tiles[_blueCastlePosition.x, _blueCastlePosition.y].PlaceContent(blueCastle);
    }
    
    private void PlaceTrees()
    {
        for (int i = 0; i < _treesCount;)
        {
            int treeX = UnityEngine.Random.Range(0, _board.Size);
            int treeY = UnityEngine.Random.Range(0, _board.Size);
            var tile = _board.Tiles[treeX, treeY];
            if (tile.Content == null)
            {
                tile.PlaceContent(_contentFactory.GetObstacle());
                i++;
            }
        }
    }

    private void KillUnitOnTile(Tile tile)
    {
        if (tile.Content == null)
        {
            return;
        }
        var playerColor = tile.Content.Player;
        HashSet<Tile> armyTiles;
        if (playerColor == PlayerColor.Red)
        {
            armyTiles = _redUnitsTiles;
        }
        else
        {
            armyTiles = _blueUnitsTiles;
        }

        tile.RemoveContent();
        armyTiles.Remove(tile);
    }

    private TileContent GetCurrentPlayerUnit()
    {
        TileContent unit;
        if (_currentState == GameState.RedMove)
        {
            unit = _contentFactory.GetPlayerContent(TileContentType.Unit, PlayerColor.Red);
        }
        else
        {
            unit = _contentFactory.GetPlayerContent(TileContentType.Unit, PlayerColor.Blue);
        }
        return unit;
    }

    private HashSet<Tile> GetCurrentPlayerUnitTiles()
    {
        if (_currentState == GameState.RedMove)
        {
            return _redUnitsTiles;
        }
        else
        {
            return _blueUnitsTiles;
        }
    }

    private void KillUnitsNotConnectedToCastles(PlayerColor playerColor)
    {
        var connectedTiles = GetOccupiedTilesConnectedToCastle(playerColor);
        var playerArmyTiles = GetPlayerArmyTiles(playerColor);

        var tilesWithUnitsToKill = new HashSet<Tile>(playerArmyTiles);
        tilesWithUnitsToKill.ExceptWith(connectedTiles);
        playerArmyTiles.ExceptWith(tilesWithUnitsToKill);

        foreach (var tile in tilesWithUnitsToKill)
        {
            tile.RemoveContent();
        }
    }

    private HashSet<Tile> GetPlayerArmyTiles(PlayerColor playerColor)
    {
        if (playerColor == PlayerColor.Blue)
        {
            return _blueUnitsTiles;
        }
        else
        {
            return _redUnitsTiles;
        }
    }



    private HashSet<Tile> GetOccupiedTilesConnectedToCastle(PlayerColor playerColor)
    {
        HashSet<Tile> occupiedTiles = new HashSet<Tile>();

        Tile castleTile;

        if (playerColor == PlayerColor.Red)
        {
            castleTile = _board.Tiles[_redCastlePosition.x, _redCastlePosition.y];
        }
        else
        {
            castleTile = _board.Tiles[_blueCastlePosition.x, _blueCastlePosition.y];
        }

        HashSet<Tile> tilesToProcess = new HashSet<Tile>(); //Tiles, which neighbors will be checked

        foreach (var neigbor in castleTile.Neighbors)
        {
            if (neigbor.Content != null && neigbor.Content.Player == playerColor)
            {
                tilesToProcess.Add(neigbor);
                occupiedTiles.Add(neigbor);
            }
        }

        while (tilesToProcess.Count != 0)
        {
            HashSet<Tile> newAllyNeigbors = new HashSet<Tile>();
            foreach (var tile in tilesToProcess)
            {
                foreach (var tileNeigbor in tile.Neighbors)
                {
                    if (tileNeigbor.Content != null && 
                        !occupiedTiles.Contains(tileNeigbor) && 
                        tileNeigbor.Content.Player == playerColor)
                    {
                        newAllyNeigbors.Add(tileNeigbor);
                        occupiedTiles.Add(tileNeigbor);
                    }
                }
            }
            tilesToProcess = newAllyNeigbors;
        }

        return occupiedTiles;
    }

    #endregion

    private void SwitchStateToNextPlayerMove()
    {
        if (_currentState == GameState.RedMove)
        {
            _currentState = GameState.BlueMove;
        }
        else if (_currentState == GameState.BlueMove)
        {
            _currentState = GameState.RedMove;
        }
    }

    private void ShowMoves()
    {
        foreach (var tile in _moveTiles)
        {
            tile.SetMoveView();
        }

        foreach (var tile in _clashTiles)
        {
            tile.SetClashView();
        }
    }

    private void FindPossibleMovesForPlayer(PlayerColor playerColor)
    {
        for (int i = 0; i < _board.Size; i++)
        {
            for (int j = 0; j < _board.Size; j++)
            {
                DetermineMovesForThisTile(_board.Tiles[i, j], playerColor);
            }
        }
    }

    private void DetermineMovesForThisTile(Tile tile, PlayerColor playerColor)
    {
        Func<Tile, bool> contentBelongsToPlayer = (tile) => tile.Content != null && tile.Content.Player == playerColor;
        tile.SetDefaultView();
        if (tile.HasNeighbors(contentBelongsToPlayer))
        {
            if (tile.Content != null &&
                (tile.Content.Type == TileContentType.Unit || tile.Content.Type == TileContentType.Castle) &&
                tile.Content.Player != playerColor)
            {
                _clashTiles.Add(tile);
            }
            else if (tile.Content == null)
            {
                _moveTiles.Add(tile);
            }
        }
    }
}
