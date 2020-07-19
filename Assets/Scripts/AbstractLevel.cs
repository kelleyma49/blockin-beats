using UnityEngine;

public abstract class AbstractLevel : MonoBehaviour
{
    public Color colorWhite;
    public Color colorBlack;
    public GameObject prefabWhite;
    public GameObject prefabBlack;
    public float timelineRate;
    public Material backgroundMaterial;
    public Color clearColor;

    public Texture2D AudioTex { get; set; }

    private readonly GameObject _background;

    protected AbstractLevel(GameObject background)
    {
        this._background = background;
    }

    public virtual void Update() {
        if (backgroundMaterial!=null) {
            backgroundMaterial.SetInt("ResolutionX",Screen.width);
            backgroundMaterial.SetInt("ResolutionY",Screen.height);
            backgroundMaterial.SetTexture("_AudioTex", AudioTex);
        }
    }
}
