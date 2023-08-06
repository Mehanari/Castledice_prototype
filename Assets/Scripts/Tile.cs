using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileContent Content => _content;

    public List<Tile> Neighbors => _neighbors;

    [SerializeField] private Transform _contentPosition;


    [SerializeField] private GameObject _moveView;
    [SerializeField] private GameObject _clashView;

    private TileContent _content;

    private List<Tile> _neighbors;

    #region Content methods

    public void PlaceContent(TileContent content)
    {
        RemoveContent();
        _content = content;
        content.transform.SetParent(transform);
        content.transform.localPosition = _contentPosition.localPosition;
    }

    public void RemoveContent()
    {
        if (_content != null)
        {
            Destroy(_content.gameObject);
            _content = null;
        }
    }

    #endregion


    #region Neighbors methods

    public void SetNeighbors(List<Tile> neighbours)
    {
        _neighbors = neighbours;
    }

    public bool HasNeighbors(Func<Tile, bool> predicate)
    {
        return _neighbors.Any(predicate);
    }

    #endregion


    #region View methods

    public void SetMoveView()
    {
        _moveView.SetActive(true);
    }

    public void SetClashView()
    {
        _clashView.SetActive(true);
    }

    public void SetDefaultView()
    {
        _moveView.SetActive(false);
        _clashView.SetActive(false);
    }

    #endregion
}
