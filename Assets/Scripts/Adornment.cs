using UnityEngine;
using System;
using System.Collections;
using Logic;
using DG.Tweening;

class Adornment
{
    const int NumLevels = 2;

    readonly GameObject[] _visuals = new GameObject[NumLevels];

    private Vector3 Offset
    {
        get
        {
            if (_visuals[0] != null)
            {
                var bounds = _visuals[0].GetComponent<SpriteRenderer>().bounds.size;
                return new Vector3(bounds.x * 0.5f, -bounds.y * 0.5f, 0.0f);
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public void Destroy()
    {
        for (int i = 0; i < _visuals.Length; i++)
        {
            if (_visuals[i]!=null)
            {
                GameObject.Destroy(_visuals[i]);
                _visuals[i] = null;
            }
        }
    }

    public void UpdatePosition(Vector3 pos)
    {
        var offset = Offset;

        foreach (var v in _visuals)
        {
            if (v!=null)
            {
                v.transform.position = pos + offset;
            }
        }
    }

    public void DOMove(Vector3 newPosition, Ease ease, float animTime)
    {
        var computedPos = newPosition + Offset;
        foreach (var v in _visuals)
        {
            v.transform.DOMove(computedPos, animTime).SetEase(ease);
        }
    }

    public void Instantiate(GameObject prefab,Transform parent,Ease hightlightEase,float highlightAnimTime)
    {
        // original:
        var inst = GameObject.Instantiate(prefab,parent) as GameObject;
        _visuals[0] = inst;

        // highlight:
        inst = GameObject.Instantiate(prefab, parent) as GameObject;
        _visuals[1] = inst;

        inst.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        inst.transform.DOScale(1.5f, highlightAnimTime).SetEase(hightlightEase).SetLoops(-1);
        inst.GetComponent<SpriteRenderer>().DOFade(0.4f, highlightAnimTime).SetEase(Ease.Flash).SetLoops(-1);
    }
}
