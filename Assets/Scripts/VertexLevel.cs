using UnityEngine;

public class VertexLevel : AbstractLevel
{
    public int triCount;
    public Shader meshShader;
    public MeshTopology topology;

    // mesh parameters:
    private Vector3[] _pos;
    private Vector2[] _uv;
    private int[] _tris;
    private MeshRenderer _meshRenderer;

    public VertexLevel(GameObject background) : base(background)
    {
        gameObject.layer = 8; // RenderTarget layer
    }

    public virtual void Start()
    {
        gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = new Material(meshShader);
        AllocateMesh();
        var bgCam = GameObject.Find("Background Camera");
        backgroundMaterial.mainTexture = bgCam.GetComponent<Camera>().targetTexture;
    }

    public override void Update()
    {
        if (AudioTex!=null)
        {
            _meshRenderer.material.SetTexture("_AudioTex", AudioTex);
        }

        // update mesh if it's changed:
        if (_tris.Length != triCount * 3)
            AllocateMesh();

        base.Update();
    }

    private void AllocateMesh()
    {
        _meshRenderer.material.SetInt("VertexCount", triCount * 3);

        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        var vertCount = triCount * 3;
        _pos = new Vector3[vertCount]; // don't need to initialize
        _uv = new Vector2[vertCount];
        _tris = new int[vertCount];
        for (int i = 0; i < vertCount; i++)
        {
            _pos[i] = new Vector3((float)i, (float)i, (float)i);
            _uv[i] = new Vector2((float)i,0.0f);
            _tris[i] = i;
        }

        mesh.vertices = _pos;
        mesh.uv = _uv;
        mesh.triangles = _tris;
        mesh.SetIndices(_tris, topology, 0);
        mesh.UploadMeshData(false);
    }
}
