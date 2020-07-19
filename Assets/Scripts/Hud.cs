using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Hud : MonoBehaviour {
	public PlayfieldManager PlayfieldManager { private get; set; }

    public GameObject arrowText;
    public GameObject bonusText;
    public GameObject scoreValue;
    public int scoreValueVibrato;
    public float scoreValueElasticity;
    public int bonusTextFontSize = 65;
    public int colorOrEmptyTextFontSize = 28;

    private float _initialTextSize;
    private Vector3 _arrowTranslate;
    private Vector3 _bonusTranslate;

	// Use this for initialization
	void Start () {
		var go = GameObject.Find("Playfield");
		PlayfieldManager = go.GetComponent<PlayfieldManager>();

#if TEXTPRO_CRAP
        _arrowTranslate = arrowText.GetComponent<RectTransform>().position;
        arrowText.SetActive(false);
        _bonusTranslate = bonusText.GetComponent<RectTransform>().position;
        bonusText.SetActive(false);
        _initialTextSize = scoreValue.GetComponent<TextMeshPro>().fontSize;
#endif

        PlayfieldManager.Playfield.Stats.OnBonusMultiplier += Stats_OnBonusMultiplier;
        PlayfieldManager.Playfield.Stats.OnColorBonus += Stats_OnColorBonus;
        PlayfieldManager.Playfield.Stats.OnEmptyBonus += Stats_OnEmptyBonus;
    }

    private void Stats_OnEmptyBonus()
    {
        ShowBonusArrowAndText(colorOrEmptyTextFontSize, "EMPTY" + System.Environment.NewLine + "BONUS");
    }

    private void Stats_OnColorBonus()
    {
        ShowBonusArrowAndText(colorOrEmptyTextFontSize, "SINGLE COLOR" + System.Environment.NewLine + "BONUS");
    }

    private void Stats_OnBonusMultiplier(int multiplier)
    {
        ShowBonusArrowAndText(bonusTextFontSize, "x" + multiplier.ToString());
    }

    private void ShowBonusArrowAndText(int fontSize,string text)
    {
#if TEXTPRO_CRAP
        var tmc = bonusText.GetComponent<TextMeshPro>();
        tmc.fontSize = fontSize;
        tmc.SetText(text);
        {
            var seq = DOTween.Sequence();
            arrowText.transform.position = new Vector3(_arrowTranslate.x - 5.0f, _arrowTranslate.y, _arrowTranslate.z);
            seq.Append(arrowText.transform.DOMoveX(_arrowTranslate.x, 1.0f).SetEase(Ease.OutBack));
            seq.AppendInterval(2.0f);
            seq.Append(arrowText.transform.DOMoveX(_arrowTranslate.x + 10.0f, 1.0f).SetEase(Ease.InBack));
            arrowText.SetActive(true);
            seq.AppendCallback(() => arrowText.SetActive(false));
        }

        {
            var seq = DOTween.Sequence();
            bonusText.transform.position = new Vector3(_bonusTranslate.x - 5.0f, _bonusTranslate.y, _bonusTranslate.z);
            seq.Append(bonusText.transform.DOMoveX(_bonusTranslate.x, 1.0f).SetEase(Ease.OutBack));
            seq.AppendInterval(2.0f);
            seq.Append(bonusText.transform.DOMoveX(_bonusTranslate.x + 10.0f, 1.0f).SetEase(Ease.InBack));
            seq.AppendCallback(() => bonusText.SetActive(false));
            bonusText.SetActive(true);
        }
#endif
    }

    // Update is called once per frame
    void Update () {
        // update score:
		var sv = scoreValue.GetComponent<TextMeshPro>();
        var newText = PlayfieldManager.Playfield.Stats.TotalScore.ToString();
#if TEXTPRO_CRAP
        if (newText != sv.text)
        {
            sv.text = newText;
            DOTween.Punch(() => new Vector3(sv.fontSize, sv.fontSize, sv.fontSize),
                v => sv.fontSize = v.x, new Vector3(1.0f,0.1f,1.0f), 0.5f, 
                scoreValueVibrato,scoreValueElasticity);
        }
#endif
    }
}
