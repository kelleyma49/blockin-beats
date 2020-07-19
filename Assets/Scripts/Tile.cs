using DG.Tweening;
using Logic;
using UnityEngine;

public class Tile
{
    private readonly GameObject _prefab;
    private GameObject _visual;
    private Adornment _adornment;

    public GameObject Prefab { get; private set; }
    public bool IsValid => _visual != null;

    public int PrevCol;
    public int PrevRow;
    public float RotatingTo = 1.0f;
    public bool IsAnimating => DOTween.IsTweening(_visual.transform);

    public Vector3 Position
    {
        get { return _visual.transform.position; }

        set
        {
            _visual.transform.position = value;
            _adornment?.UpdatePosition(value);
        }
    }

    public void Destroy()
    {
        Prefab = null;
        if (_visual!=null)
        {
            Object.Destroy(_visual);
            _visual = null;
        }

        _adornment?.Destroy();
        _adornment = null;
    }

    public void Instantiate(GameObject prefab, GameObject prefabAdornment, Vector3 position, Transform parent,TileAnimParams animParams)
    {
        Prefab = prefab;
        if (Prefab != null)
        {
            _visual = UnityEngine.Object.Instantiate(Prefab, Vector3.zero, Quaternion.identity);
            _visual.transform.SetParent(parent, false);
            _visual.transform.position = position;
        }

        if (prefabAdornment != null)
        {
            _adornment = new Adornment();
            _adornment.Instantiate(prefabAdornment,parent,animParams.easeType,animParams.animTime);
            _adornment.UpdatePosition(position);
        }
    }

    public void AnimateTo(Vector3 prevPos,Vector3 newPos,TileAnimParams animParams)
    {
        Position = prevPos;
        _visual.transform.DOMove(newPos, animParams.animTime).SetEase(animParams.easeType);

        _adornment?.DOMove(newPos, animParams.easeType, animParams.animTime);
    }

    public void ScaleTo(float from, float to, TileAnimParams animaParams, float? delay)
    {
        _visual.transform.localScale = new Vector3(from, from, from);
        var tween = _visual.transform.DOScale(to, animaParams.animTime).SetEase(animaParams.easeType);
        if (delay != null)
        {
            tween.SetDelay((float) delay);
        }
    }
}

[System.Serializable]
public struct TileAnimParams
{
    public TileAnimParams(Ease ease, float time)
    {
        easeType = ease;
        animTime = time;
    }

    public Ease easeType;
    public float animTime;
}