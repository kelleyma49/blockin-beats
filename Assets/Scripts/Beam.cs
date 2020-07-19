using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Beam : MonoBehaviour {

    public GameObject prefabBeam;
    public GameObject prefabFlag;
    public GameObject prefabHighlight;
    public GameObject prefabTrail;

    public PlayfieldManager playfieldManager;
    public int beamWidth = 3;
    public int flagTextHeight = 50;
    public Vector2 flagDims;
    public Color trailColor;
    public Color beatColor;
    public Color restingColor;

    private float beamColorLerp;
    private GameObject text;
    private GameObject beam;
    private GameObject highlight;
    private GameObject trail;
    private GameObject flag;
    private GameObject mainCamera;
    
    private SpriteRenderer _beamRenderer;
    private SpriteRenderer _flagRenderer;
    private SpriteRenderer _highlightRenderer;
    private SpriteRenderer _trailRenderer;

    // Use this for initialization
    void Start () {

        var beamGo = GameObject.Find("Beam");
        // beam
        beam = Instantiate(prefabBeam, Vector3.zero, Quaternion.identity) as GameObject;
        beam.transform.SetParent(beamGo.transform, false);
        _beamRenderer = beam.GetComponent<SpriteRenderer>();
        beamColorLerp = 0.0f;

        // highlight
        highlight = Instantiate(prefabHighlight, Vector3.zero, Quaternion.identity) as GameObject;
        highlight.transform.SetParent(beamGo.transform, false);
        _highlightRenderer = highlight.GetComponent<SpriteRenderer>();
        
        // flag 
        flag = Instantiate(prefabFlag, Vector3.zero, Quaternion.identity) as GameObject;
        flag.transform.SetParent(beamGo.transform, false);
        _flagRenderer = flag.GetComponent<SpriteRenderer>();
        
        // trail
        trail = Instantiate(prefabTrail, Vector3.zero, Quaternion.identity) as GameObject;
        trail.transform.SetParent(beamGo.transform, false);
        _trailRenderer = trail.GetComponent<SpriteRenderer>();
        _trailRenderer.color = trailColor;

        // text 
        text = GameObject.Find("BeamText");
        mainCamera = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update () {
        var pf = playfieldManager;
        beamColorLerp += Time.deltaTime;
        if (beamColorLerp > 1.0f)
            beamColorLerp -= 1.0f;

        var currX = pf.BotLeft.x + (float)pf.Playfield.Timeline.Position * (pf.BotRight.x - pf.BotLeft.x);
        var beamRect = _beamRenderer.sprite.textureRect;

        // beam:
        beam.transform.position = new Vector2(currX, pf.UpLeft.y);
        beam.transform.localScale = new Vector3(
            (float)beamWidth / (float)beamRect.width, 
            (float)pf.BackgroundTextureRect.height / (float)beamRect.height, 1.0f);

        // highlight:
        var highlightRect = _highlightRenderer.sprite.textureRect;
        _highlightRenderer.color = Color.Lerp(restingColor, beatColor, beamColorLerp);
        highlight.transform.position = new Vector2(currX, pf.UpLeft.y);
        highlight.transform.localScale = new Vector3(
            0.6f,
            (float)pf.BackgroundTextureRect.height / (float)highlightRect.height, 1.0f);

        // flag:
        var flagRect = _beamRenderer.sprite.textureRect;
        float xScale = (float)flagDims.x / (float)flagRect.width;
        float yScale = (float)flagDims.y / (float)flagRect.height;
        flag.transform.position = new Vector2(currX, pf.UpLeft.y);
        flag.transform.localScale = new Vector3(xScale, yScale, 1.0f);
        
        // trail:
        var trailRect = _trailRenderer.sprite.textureRect;
        trail.transform.position = new Vector2(currX, pf.UpLeft.y);
        trail.transform.localScale = new Vector3(
            1.0f,
            (float)pf.BackgroundTextureRect.height/(float)trailRect.height, 1.0f);
    }

    private void OnGUI()
    {
        var cam = mainCamera.GetComponent<Camera>();
        Vector3 screenPos = cam.WorldToScreenPoint(flag.transform.position);
        var flagRect = _beamRenderer.sprite.textureRect;
        screenPos.y += flagDims.y;
        screenPos.x += flagDims.x;

        var go = text.GetComponent<RectTransform>();
        go.transform.position = screenPos;

        var textComp = text.GetComponent<Text>();
        textComp.text = playfieldManager.Playfield.Stats.FrameSquaresRemoved.ToString();
    }
}
