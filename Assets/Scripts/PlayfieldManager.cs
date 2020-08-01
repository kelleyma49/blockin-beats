using UnityEngine;
using System;
using System.Collections;
using Logic;
using DG.Tweening;
using System.Collections.Generic;

public class PlayfieldManager : MonoBehaviour
{
    // square
    public GameObject prefabCompleting;
    public GameObject prefabOutline;

    // cell
    public GameObject prefabRemoving;
    public GameObject prefabJewelBoth;
    public GameObject prefabJewelHorz;
    public GameObject prefabJewelVert;
    public GameObject prefabJewelWillBeRemoved;

    public TileAnimParams removingAnim;
    public TileAnimParams previewMove;
    public float removingDelayMultiplier = 1.0f/30.0f;
    public float previewX = 0;
    public float previewMargin = 0.0f;

    public const int NumColumns = 16;
    public const int NumRows = 10;

    public Rect BackgroundTextureRect { get; private set; }
    public Vector2 TileSizes { get; private set; }
    public Vector2 SquareSizes { get; private set; }
    public Vector2 BotLeft { get; private set; }
    public Vector2 UpLeft { get; private set; }
    public Vector2 BotRight { get; private set; }
    public Vector2 UpRight { get; private set; }
    public Logic.Playfield Playfield { get; private set; }
    public AbstractLevel Level { get; private set; }

    private int _currentLevel = 0;
    private readonly string[] _levelNames = new string[]
    {
        "NeonGuitar",
        "WaveGreekFrieze5Animated",
        "TrippyWaves",
        "PentagonalTessellations",
        "ElDorado",
        "DotGridThing",
        "ParticlesDance",
        "CuteAndPop",
        "Blobs",
        "FlowerOfLifeRGB",
        "StringTheory",
        "Waves",
        "CirclePatternIq",
        "HexagonsIq",
        "PerspexWebLattice",
        "CreationSilexars",
        "SimplicityGalaxy",
        //"FractalLand",
        "TestBubbles",
        "BouncingPrimitive",
        "DistortSquare",
        "SineGridWarping",
        "VertexShader1",
        "VertexShader2",
        "Circles1",
        "Circles2",
        "Circles3",
        "Circles4",
        "Circles5",
        "SpEq1"
    };
    private readonly Tile[,] _tiles = new Tile[NumColumns,NumRows];
    private GameObject[,] _squaresCenter;
    private GameObject[,] _squaresOutline;
    private AudioViz _audioViz;
    private DropPieceView _dropPieceView;
    private const int NumPreviews = 3;
    private readonly Queue<DropPiecePreview> _dropPiecePreviews = new Queue<DropPiecePreview>();

    // Use this for initialization
    void Start()
    {
        _audioViz = new AudioViz();

        Playfield = new Logic.Playfield(NumColumns, NumRows);
        foreach (var c in Playfield.GetEnumeratorCells())
        {
            c.StateChanged += this.CellStateChanged;
            c.RemoveStateChanged += this.CellRemoveStateChanged;
            c.Transferred += this.CellTransferred;
        }

        _squaresCenter = new GameObject[Playfield.NumSquareColumns,Playfield.NumSquareRows];
        _squaresOutline = new GameObject[Playfield.NumSquareColumns,Playfield.NumSquareRows];
        foreach (var s in Playfield.GetEnumeratorSquares())
        {
            s.StateChanged += this.SquareStateChanged;
        }

        for (var c = 0; c < NumColumns; c++)
        {
            for (var r = 0; r < NumRows; r++)
            {
                _tiles[c,r] = new Tile();
            }
        }

        var sr = GetComponent<SpriteRenderer>();
        var exs = sr.bounds.extents;
        var center = sr.bounds.center;
        BackgroundTextureRect = sr.sprite.textureRect;

        // compute extents:
        BotLeft = new Vector2(center.x - exs.x, center.y - exs.y);
        UpLeft = new Vector2(center.x - exs.x, center.y + exs.y);
        BotRight = new Vector2(center.x + exs.x, center.y - exs.y);
        UpRight = new Vector2(center.x + exs.x, center.y + exs.y);

        TileSizes = new Vector2((exs.x * 2.0f) / (float)NumColumns, (exs.y * 2.0f) / (float)NumRows);
        SquareSizes = new Vector2(TileSizes.x*2.0f, TileSizes.y*2.0f);

        // drop piece setup:
        _dropPieceView = GameObject.Find("DropPiece").GetComponent<DropPieceView>();

        SetLevel();

        // must come after set level:
        for (int p = 0; p < NumPreviews; p++)
        {
            var dpp = new DropPiecePreview();
            dpp.GenerateTiles(GetTileVisuals, new TileAnimParams(Ease.InCubic, 0.5f));
            _dropPiecePreviews.Enqueue(dpp);
        }
    }

    int _lastColumn = -1;

    public DropPieceSimple PopPreview()
    {
        // create new preview piece:
        var newDpp = new DropPiecePreview();
        {
            newDpp.GenerateTiles(GetTileVisuals, new TileAnimParams(Ease.InCubic, 0.5f));
            var previewCount = _dropPiecePreviews.Count;
            var x = 2.0f*UpLeft.x;
            var y = (previewCount * TileSizes.y * 2.0f) + (previewCount * previewMargin);
            newDpp.UpdatePosition(new Vector2(x,y),TileSizes);
        }

        // add new piece:
        var dpp = _dropPiecePreviews.Dequeue();
        dpp.DestroyVisuals();

        _dropPiecePreviews.Enqueue(newDpp);

        // animate tiles:
        var num = 0;
        foreach (var piece in _dropPiecePreviews)
        {
            var y = (num * TileSizes.y * 2.0f) + num * previewMargin;
            var pos = new Vector2(UpLeft.x + previewX, UpLeft.y - y);
            piece.AnimateTo(pos, TileSizes, previewMove);
            num++;
        }

        return dpp.DropPiece;
    }

    public void GetTileVisuals(Logic.Cell.States cell, out GameObject tile, out GameObject adornment,out Transform parent)
    {
        tile = Logic.Cell.IsStateBlack(cell) ? Level.prefabBlack : Level.prefabWhite;
        adornment = GetAdornment(cell);
        parent = GameObject.Find("Playfield").transform;
    }

    // Update is called once per frame
    void Update()
    {
        // update preview positions:
        /*int num = 0;
        foreach (var piece in _dropPiecePreviews)
        {
            var y = num * TileSizes.y * 2.0f + num * previewMargin;
            var pos = new Vector2(UpLeft.x + previewX,UpLeft.y - y);
            piece.UpdatePosition(pos,TileSizes);
            num++;
        }
        */

        // Obtain the samples from the frequency bands of the attached AudioSource  
        if (audioSource!=null) {
            _audioViz.SetSpectrumData(audioSource, 1.0f * Time.deltaTime);
            Level.AudioTex = _audioViz.AudioTexture;
        }

        //if (_dropPieceView.inputHandler.Action == InputHandler.Actions.Pause)
        //{
        //Debug.Log("set level '" + _levelNames[_currentLevel] + "'");
        //SetLevel();
        // }

        // update plane to fill camera:
        {
            var plane = GameObject.Find("BackgroundPlane");
            Camera cam = Camera.main;

            float pos = (cam.nearClipPlane + 0.01f);

            plane.transform.position = cam.transform.position + (cam.transform.forward * pos);
            plane.transform.LookAt(cam.transform);
            plane.transform.Rotate(90.0f, 0.0f, 0.0f);

            float h = (Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f) / 10.0f;

            plane.transform.localScale = new Vector3(h * cam.aspect, 1.0f, h);
        }
        
        if (Playfield.Timeline.TotalColumnAbs != _lastColumn) {
            _lastColumn = Playfield.Timeline.TotalColumnAbs;
            var output = Playfield.PrintCells(_lastColumn.ToString());
            //Debug.Log(output);
        }

        Playfield.Timeline.IncrementPosition(Time.deltaTime * Level.timelineRate);
    }

    AudioSource audioSource;
    GameObject lgo;
    void SetLevel()
    {
        if (lgo != null)
        {
            GameObject.Destroy(lgo);
            lgo = null;
        }

        var level = _levelNames[_currentLevel];
        var subDir = $"Levels/{level}";
        var goPath = $"{subDir}/Level{level}";
        Debug.Log($"Instantiating {goPath}");
        {
            var go = Resources.Load<GameObject>(goPath);
            if (go == null)
            {
                throw new Exception($"failed to load resource '{goPath}'");
            }
            lgo = Instantiate(go);
        }
        Level = (AbstractLevel) lgo?.GetComponent<MonoBehaviour>();
        if (Level==null)
        {
            throw new Exception($"failed to find component for level '{level}'");
        }

        if (audioSource!=null)
            audioSource.Stop();
        audioSource = lgo.GetComponent<AudioSource>();
        if (audioSource!=null)
            audioSource.Play(); 
        
        var bgcam = GameObject.Find("Background Camera").GetComponent<Camera>();
        bgcam.backgroundColor = Level.clearColor;

        var bggo = GameObject.Find("BackgroundPlane");
        bggo.GetComponent<MeshRenderer>().material = Level.backgroundMaterial;
        _currentLevel = ++_currentLevel % _levelNames.Length;

        // update current visuals:
        foreach (var c in Playfield.GetEnumeratorCells())
        {
            InstantiateVisual(c);
        }

        foreach (var s in Playfield.GetEnumeratorSquares())
        {
        //    InstantiateVisual(s);
        }

        var dpv = GameObject.Find("DropPiece").GetComponent<DropPieceView>();
        dpv.UpdateVisuals = true;
    }

    public Vector2 ComputePos(PlayfieldPoint point)
    {
        return ComputePos(point.Column,point.Row);
    }

    public Vector2 ComputePos(int column, int row)
    {
        return ComputePos(UpLeft, TileSizes, column, row);
    }

    public static Vector2 ComputePos(Vector2 upLeft,Vector2 tileSizes,int column, int row)
    {
        return new Vector2(upLeft.x + column * tileSizes.x, upLeft.y - row * tileSizes.y);
    }

    public Vector2 ComputeSquarePos(int column, int row)
    {
        return ComputePos(column,row);
    }

    public void PopulateCell(PlayfieldPoint p,Cell.States state)
    {
        Playfield.GetCell(p).State = state;
    }
    public Tile InstantiateVisual(Cell c)
    {
        var point = c.Point;
        RemoveCell(point);

        GameObject prefabCell = null;
        GameObject prefabOverlay = null;

        switch (c.RemoveState)
        {
            case Cell.RemoveStates.JewelWillBeRemoved:
            case Cell.RemoveStates.JewelRemoving:
                prefabCell = prefabJewelWillBeRemoved;
                //prefabCell = prefabRemoving;
                Debug.Log("jewel removing");
                break;

            case Cell.RemoveStates.Removing:
            prefabCell = prefabRemoving;
            break;

            case Cell.RemoveStates.NotRemoved:
            //case Cell.RemoveStates.WillBeRemoved:
            {
                switch (c.State)
                {
                case Cell.States.Black:
                    prefabCell = Level.prefabBlack;
                    break;

                case Cell.States.BlackJeweledBoth:
                    prefabCell = Level.prefabBlack;
                    prefabOverlay = prefabJewelBoth;
                    break;

                case Cell.States.BlackJeweledHorz:
                    prefabCell = Level.prefabBlack;
                    prefabOverlay = prefabJewelHorz;
                    break;

                case Cell.States.BlackJeweledVert:
                    prefabCell = Level.prefabBlack;
                    prefabOverlay = prefabJewelVert;
                    break;

                case Cell.States.WhiteJeweledBoth:
                    prefabCell = Level.prefabWhite;
                    prefabOverlay = prefabJewelBoth;
                    break;

                case Cell.States.WhiteJeweledVert:
                    prefabCell = Level.prefabWhite;
                    prefabOverlay = prefabJewelVert;
                    break;

                case Cell.States.WhiteJeweledHorz:
                    prefabCell = Level.prefabWhite;
                    prefabOverlay = prefabJewelHorz;
                    break;

                case Cell.States.White:
                    prefabCell = Level.prefabWhite;
                    break;
                }    
            }
            break;
        }

        var result = (Tile)null;
        var parent = GameObject.Find("Playfield").transform;
        if (prefabCell != null)
        {
            var anim = new TileAnimParams(_dropPieceView.jewelEaseType, _dropPieceView.jewelHighlightAnimTime);
            var pos = ComputePos(point.Column, point.Row);
            _tiles[point.Column,point.Row].Instantiate(prefabCell,prefabOverlay,pos,parent,anim);
            result = _tiles[point.Column, point.Row];
        }
        
        return result;
    }

    private void RemoveCell(PlayfieldPoint p)
    {
        _tiles[p.Column, p.Row].Destroy();
    }

    public GameObject GetAdornment(Cell.States cell)
    {
        switch (cell)
        {
            case Cell.States.Empty:
            case Cell.States.Disabled:
            case Cell.States.Black:
            case Cell.States.White:
            case Cell.States.BlackAndWhite:
                return null;
            case Cell.States.BlackJeweledBoth:
            case Cell.States.WhiteJeweledBoth:
                return prefabJewelBoth;

            case Cell.States.BlackJeweledHorz:
            case Cell.States.WhiteJeweledHorz:
                return prefabJewelHorz;

            case Cell.States.WhiteJeweledVert:
            case Cell.States.BlackJeweledVert:
                return prefabJewelVert;

            default:
                return null;
        }
    }

    public void InstantiateVisual(Square s)
    {
        var sul = ComputeSquarePos(s.Column, s.Row);
        var squarePos = new Vector2(sul.x + SquareSizes.x * 0.5f, sul.y - SquareSizes.y * 0.5f);

        // center:
        {
            var go = Instantiate(prefabCompleting, Vector3.zero, Quaternion.identity) as GameObject;
            go.GetComponent<SpriteRenderer>().color=s.UpperLeft.IsWhite?
                Level.colorWhite : Level.colorBlack;
            go.transform.SetParent(GameObject.Find("Playfield").transform, false);
            go.transform.position = squarePos;
            _squaresCenter[s.Column,s.Row] = go;                              
        }

        // outline:
        {
            var go = Instantiate(prefabOutline, Vector3.zero, Quaternion.identity) as GameObject;
            go.transform.SetParent(GameObject.Find("Playfield").transform, false);
            go.transform.position = squarePos;
            _squaresOutline[s.Column,s.Row] = go;                              
        }

        // show effect:
        var effects = GetComponentInParent<Effects>();
        effects.TriggerSquareHighlight(GameObject.Find("Playfield"), prefabOutline, squarePos);
    }

     private void RemoveSquare(Square s)
    {
        if (_squaresCenter[s.Column,s.Row]!=null) {
            GameObject.Destroy(_squaresCenter[s.Column,s.Row]);
            _squaresCenter[s.Column,s.Row] = null;
        }
        if (_squaresOutline[s.Column,s.Row]!=null) {
            GameObject.Destroy(_squaresOutline[s.Column,s.Row]);
            _squaresOutline[s.Column,s.Row] = null;
        }
    }

    public void PlayCompletedSnd()
    {
        GameObject.Find("CompletedSound1")?.GetComponent<AudioSource>().Play();
    }

    public void PlayMoveSnd()
    {   
        GameObject.Find("MoveSound1")?.GetComponent<AudioSource>().Play();
    }

    private void SquareStateChanged(object sender,System.EventArgs args)
    {
        var s = sender as Square;
        switch (s.State)
        {
        case Logic.Square.States.Completing:
            PlayCompletedSnd();
            InstantiateVisual(s);
            break;

        case Logic.Square.States.None:
            RemoveSquare(s);
            break;
        }
    }

    private void CellStateChanged(object sender,CellStateChangedEventArgs args)
    {
        var p = new PlayfieldPoint(args.Column,args.Row);

        switch (args.NewState)
        {
            case Cell.States.Empty:
                RemoveCell(p);
                break;

            case Cell.States.Disabled:
                break;
            case Cell.States.BlackAndWhite:
                break;

            case Cell.States.Black:
            case Cell.States.BlackJeweledBoth:
            case Cell.States.BlackJeweledHorz:
            case Cell.States.BlackJeweledVert:
            case Cell.States.White:
            case Cell.States.WhiteJeweledBoth:
            case Cell.States.WhiteJeweledHorz:
            case Cell.States.WhiteJeweledVert:
                InstantiateVisual(Playfield.GetCell(p));
                break;

            default:
                break;
        }
    }

    private void CellRemoveStateChanged(object sender,CellRemoveStateChangedEventArgs args)
    {
        var p = new PlayfieldPoint(args.Column,args.Row);
        var go = InstantiateVisual(Playfield.GetCell(p));

        var delay = args.Row * removingDelayMultiplier;
        go?.ScaleTo(0.0f,1.0f,removingAnim, delay);
    }

    private void CellTransferred(object sender, TransferredEventArgs args)
    {
        if (!args.Cell.IsOccupied) 
            return;

        var tile = _tiles[args.Cell.Column, args.Cell.Row];
        if (!tile.IsValid) {
            tile = InstantiateVisual(args.Cell);
        }
        if (tile!=null && tile.IsValid)
        {
            var prevPos = ComputePos(args.PrevPoint);
            var newPos = ComputePos(args.Cell.Point);
            tile.AnimateTo(prevPos,newPos,new TileAnimParams(Ease.InCubic,0.25f));
        }
    }
}