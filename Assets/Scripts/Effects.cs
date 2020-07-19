using UnityEngine;
using System;
using System.Collections;
using Logic;
using DG.Tweening;
using System.Collections.Generic;

public class Effects : MonoBehaviour
{
    public float startScale = 4.0f;
    public TileAnimParams startScaleAnim;
    public float endScale = 1.5f;
    public TileAnimParams endScaleAnim;

    public TileAnimParams ghostAnim;

    private readonly List<GameObject> _highlights = new List<GameObject>();
    private readonly List<GameObject> _ghosts = new List<GameObject>();

    public void TriggerSquareHighlight(GameObject parent, GameObject highlightPrefab, Vector2 position)
    {
        var go = Instantiate(highlightPrefab, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = position;
        go.transform.localScale = new Vector3(startScale, startScale, startScale);

        // setup sequence animation:
        var mySequence = DOTween.Sequence();
        var step1 = go.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), startScaleAnim.animTime);
        step1.SetEase(startScaleAnim.easeType);
        mySequence.Append(step1);
        var scaleVec = new Vector3(endScale, endScale, endScale);
        var step2 = go.transform.DOScale(scaleVec, endScaleAnim.animTime).OnComplete(() => this.RemoveHighlight(go));
        step2.SetEase(endScaleAnim.easeType);
        var alphaFade = go.GetComponent<SpriteRenderer>().DOFade(0.0f,endScaleAnim.animTime);
        mySequence.Append(step2);
        mySequence.Join(alphaFade);
        _highlights.Add(go);
    }

    private void RemoveHighlight(GameObject go)
    {
        _highlights.Remove(go);
        GameObject.Destroy(go);
    }

    public void TriggerGhost(GameObject parent,GameObject prefab, Vector2 position)
    {
        var go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = position;

        // set animation:
        var sr = go.GetComponent<SpriteRenderer>();
        var origColor = sr.color;
        origColor.a = 0.5f;
        sr.DOFade(0.0f, ghostAnim.animTime).SetEase(ghostAnim.easeType).OnComplete( () => this.RemoveGhost(go));
        _ghosts.Add(go);
    }

    private void RemoveGhost(GameObject go)
    {
        _ghosts.Remove(go);
        GameObject.Destroy(go);
    }
}