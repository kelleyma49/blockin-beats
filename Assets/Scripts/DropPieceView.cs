﻿using UnityEngine;
using System;
using System.Collections;
using Logic;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class DropPieceView : MonoBehaviour
{
    public float ghostInterval;
    public float holdTime = 2.0f;
    public double downDropRate = 2.0f;
    public double dropRateWhole = 0.5f;
    public double dropRateSplit = 0.5f;
    public Ease jewelEaseType = Ease.Flash;
    public float jewelHighlightAnimTime = 1.0f;
    public float jewelRotateAnimTime = 0.5f;
    public PlayfieldManager playfieldManager;

    public bool UpdateVisuals { get; set; }

    private readonly Logic.DropPiece _logic = new Logic.DropPiece();
    private bool _instantiatePieces = true;
    private Tile[,] _pieces;
    private Vector2 _vecOffset = Vector2.zero;
    private bool _newColumnOrRow;
    private float _RotatingTo = 1.0f;
    private Logic.DropPiece.MoveDirection _moveDirection;
    private Logic.DropPiece.MoveDirection _fastMoveDirection;

    private void Start()
    {
        var pi = GetComponent<PlayerInput>();
        var leftAction = pi.actions.FindAction("Left", true);
        leftAction.started += c => MoveAction(c, isLeft: true);
        leftAction.performed += c => MoveAction(c, isLeft: true);
        var rightAction = pi.actions.FindAction("Right", true);
        rightAction.started += c => MoveAction(c, isLeft: false);
        rightAction.performed += c => MoveAction(c, isLeft: false);
        var downAction = pi.actions.FindAction("Down", true);
        downAction.performed += _ => _moveDirection = Logic.DropPiece.MoveDirection.Down;
        var rotateAction = pi.actions.FindAction("Rotate", true);
        rotateAction.performed += _ => _moveDirection = Logic.DropPiece.MoveDirection.Rotate;

        /*
        GetComponentInChildren<ParticleSystem>().enableEmission = false;
        GetComponentInChildren<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = "PlayfieldHack";*/

        _logic.HoldTime = holdTime;
        _logic.DownDropRate = downDropRate;
        _logic.DropRateWhole = dropRateWhole;
        _logic.DropRateSplit = dropRateSplit;
        _logic.Rotated += this.Rotated;

        _logic.NewColumnOrRow += (sender, args) => _newColumnOrRow = true;
    }

    private void MoveAction(InputAction.CallbackContext context, bool isLeft)
    {
        Debug.Log($"duration: {context.duration} time: {context.time} startTime: {context.startTime}");
        var dir = isLeft ? Logic.DropPiece.MoveDirection.Left : Logic.DropPiece.MoveDirection.Right;
        if (context.interaction is HoldInteraction && context.phase == InputActionPhase.Performed)
        {
            _fastMoveDirection = dir;
        }
        else
        {
            _moveDirection = dir;
        }
    }

   
    void Update()
    {
        _newColumnOrRow = false;

        var playfield = playfieldManager.Playfield;
        if (_fastMoveDirection != Logic.DropPiece.MoveDirection.None)
        {
            _moveDirection = _fastMoveDirection;
        }
        bool moved = _logic.Update(TimeSpan.FromSeconds(Time.deltaTime), _moveDirection,
            (c, r, i) => c >= 0 && c < playfield.NumCellColumns - 1,
            (d) => playfieldManager.PlayMoveSnd(),
            (p) => p.Row >= playfield.NumCellRows || playfield.GetCell(p).IsOccupied,
            playfieldManager.PopulateCell
        );
        if (!moved)
        {
            _fastMoveDirection = DropPiece.MoveDirection.None;
        }

        const int numColumns = Logic.DropPiece.NumColumns;
        const int numRows = Logic.DropPiece.NumRows;

        // show drop piece:
        if (_instantiatePieces)
        {
            var dps = playfieldManager.PopPreview();
            _instantiatePieces = false;
            _logic.DownDropRate = downDropRate;
            _logic.Reset(new PlayfieldPoint(0, 0), dps);
            if (_pieces==null)
            {
                _pieces = new Tile[numColumns, numRows];
                for (var c = 0; c < numColumns; c++)
                {
                    for (var r = 0; r < numRows; r++)
                    {
                        _pieces[c, r] = new Tile();
                    }
                }
            }
            UpdateVisuals = true;
        }

        // instantiate pieces:
        if (UpdateVisuals)
        {
            DestroyVisuals();
            var level = playfieldManager.Level;

            for (var c = 0; c < numColumns; c++)
                for (var r = 0; r < numRows; r++)
                {
                    var cell = _logic.Cells[c][r];
                    GameObject tile = null, adornment = null;
                    Transform pfTransform = null;
                    playfieldManager.GetTileVisuals(cell,out tile,out adornment,out pfTransform);
                    _pieces[c,r].Instantiate(tile,adornment,Vector3.zero,pfTransform,new TileAnimParams(jewelEaseType,jewelHighlightAnimTime));
                }
            UpdateVisuals = false;
        }

        ComputeDropPiecePosition();

        if (_logic.CurrentState == DropPiece.State.Complete)
        {
            /*Vector2 prevPos = playfieldManager.ComputePos(
                           _logic.Positions[0].Column,
                           _logic.Positions[0].Row + 2
                           );
            GetComponentInChildren<ParticleSystem>().transform.position = new Vector3(prevPos.x, prevPos.y, 0.0f);

            var ps = GetComponentInChildren<ParticleSystem>();
            var em = ps.emission;
            em.enabled = true;
            ps.Play();
            */

            //StartCoroutine(StopSparks());
            DestroyVisuals();
            _instantiatePieces = true;
        }

        _moveDirection = DropPiece.MoveDirection.None;
    }

    IEnumerator StopSparks()
    {
        yield return new WaitForSeconds(0.4f);
        GetComponentInChildren<ParticleSystem>().Stop();
    }

    void DestroyVisuals()
    {
        const int numColumns = Logic.DropPiece.NumColumns;
        const int numRows = Logic.DropPiece.NumRows;

        for (var c = 0; c < numColumns; c++)
        {
            for (var r = 0; r < numRows; r++)
            {
                _pieces[c, r].Destroy();
            }
        }
    }

    void ComputeDropPiecePosition()
    {
        var fx = playfieldManager.GetComponentInParent<Effects>();
        var parent = GameObject.Find("Playfield");

        var upLeft = playfieldManager.UpLeft;

        var tileSizes = playfieldManager.TileSizes;
        _vecOffset.y = (float)(_logic.DropTimerLerp) * tileSizes.y;

        // check ghost effect timer:
        bool showGhost = _logic.FastDrop || _newColumnOrRow;

        for (int c = 0; c < Logic.DropPiece.NumColumns; c++) {
            for (int r = 0; r < Logic.DropPiece.NumRows; r++)
            {
                var piece = _pieces[c, r];
                if (piece.IsValid) {
                    if (showGhost && _newColumnOrRow)
                    {
                        fx.TriggerGhost(parent, piece.Prefab, piece.Position);
                    }

                    if ((c == 0 && _logic.CurrentState == DropPiece.State.SplitRight)
                    || (c == 1 && _logic.CurrentState == DropPiece.State.SplitLeft)) {
                        piece.Destroy();
                    } else {
                        Vector2 prevPos = playfieldManager.ComputePos(
                            _logic.Positions[piece.PrevCol].Column,
                            _logic.Positions[piece.PrevCol].Row + piece.PrevRow
                            );

                        Vector2 newPos = playfieldManager.ComputePos(
                            _logic.Positions[c].Column,
                            _logic.Positions[c].Row + r
                            );

                        //float lerp = Mathf.Clamp01(_RotatingTo);
                        float lerp = _RotatingTo;
                        var pos = new Vector2(
                            Mathf.Lerp(prevPos.x, newPos.x, lerp),
                            Mathf.Lerp(prevPos.y, newPos.y, lerp));
                        pos.y -= _vecOffset.y;

                        piece.Position = pos;
                    }
                }
            }
        }
    }

    private void Rotated(object sender, Logic.RotateEventArgs eventArgs)
    {
        // start animation:
        DOTween.To(rt => _RotatingTo = rt, 0, 1, jewelRotateAnimTime);

        for (int c = 0; c < Logic.DropPiece.NumColumns; c++)
        {
            for (int r = 0; r < Logic.DropPiece.NumRows; r++)
            {
                _pieces[c, r].PrevCol = c;
                _pieces[c, r].PrevRow = r;
            }
        }

        if (eventArgs.Direction == DropPiece.RotateDirection.Left)
        {
            var temp = _pieces[0, 0];
            _pieces[0, 0] = _pieces[0, 1];
            _pieces[0, 1] = _pieces[1, 1];
            _pieces[1, 1] = _pieces[1, 0];
            _pieces[1, 0] = temp;
        }
        else
        {
            var temp = _pieces[0, 0];
            _pieces[0, 0] = _pieces[1, 0];
            _pieces[1, 0] = _pieces[1, 1];
            _pieces[1, 1] = _pieces[0, 1];
            _pieces[0, 1] = temp;
        }
    }
}

