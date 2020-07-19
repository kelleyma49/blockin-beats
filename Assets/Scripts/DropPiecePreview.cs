using Logic;
using UnityEngine;

public class DropPiecePreview
{
    public DropPieceSimple DropPiece { get; set; }
    public bool IsValid => DropPiece != null;

    private const int NumColumns = DropPieceSimple.NumColumns;
    private const int NumRows = DropPieceSimple.NumRows;

    private readonly Tile[,] _tiles = new Tile[NumColumns,NumRows];

    public void DestroyVisuals()
    {
        for (int c = 0; c < NumColumns; c++)
        {
            for (int r = 0; r < NumRows; r++)
            {
                _tiles[c,r].Destroy();
            }
        }
    }

    public void TransferTo(DropPiecePreview target)
    {
        for (var c = 0; c < NumColumns; c++)
        {
            for (var r = 0; r < NumRows; r++)
            {
                target._tiles[c, r] = _tiles[c, r];
                _tiles[c, r] = null;
            }
        }
        target.DropPiece = DropPiece;
        DropPiece = null;
    }

    public delegate void GetPrefabsDelegate(Cell.States cell,out GameObject tile,out GameObject adornment,out Transform parent);

    public void GenerateTiles(GetPrefabsDelegate getPrefabs, TileAnimParams animParams)
    {
        DropPiece = new DropPieceSimple();
        DropPiece.Reset(true,DropPieceSimple.RandomCell);

        var pfTransform = GameObject.Find("Playfield").transform;

        for (var c = 0; c < NumColumns; c++)
        {
            for (var r = 0; r < NumRows; r++)
            {
                var cell = DropPiece.Cells[c][r];
                GameObject prefab = null, adornment = null;
                Transform transform = null;
                getPrefabs(cell, out prefab, out adornment, out transform);
                var tile = new Tile();
                tile.Instantiate(prefab, adornment, Vector3.zero, transform, animParams);
                _tiles[c, r] = tile;
            }
        }
    }

    public void AnimateTo(Vector2 upperLeft, Vector2 tileSizes,TileAnimParams animParams)
    {
        for (var c = 0; c < NumColumns; c++)
        {
            for (var r = 0; r < NumRows; r++)
            {
                var prevPos = _tiles[c, r].Position;
                var newPos = PlayfieldManager.ComputePos(upperLeft, tileSizes, c, r);
                _tiles[c, r].AnimateTo(prevPos,newPos,animParams);
            }
        }
    }

    public void UpdatePosition(Vector2 upperLeft, Vector2 tileSizes)
    {
        for (var c = 0; c < NumColumns; c++)
        {
            for (var r = 0; r < NumRows; r++)
            {
                var pos = PlayfieldManager.ComputePos(upperLeft, tileSizes, c, r);
                _tiles[c, r].Position = pos;
            }
        }
    }
}