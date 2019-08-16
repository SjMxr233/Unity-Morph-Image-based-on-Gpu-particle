using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GpuParticle : MonoBehaviour{
    struct Particle
    {
        public Vector3 position;
        public Vector3 CustomPos;
        public Vector2 uv;
    }
    int ThreadBlockSize = 256;
    int blockPerGrid;
    ComputeBuffer ParticleBuffer,argsBuffer;
    private uint[] _args;
    private float time;
  
    public Texture2D initTexture;
    public int N;
    private int width, height;
    [Range(0, 1)]
    public float lerpt;
    [Range(0.001f, 0.008f)]
    public float size=0.01f;
    [SerializeField]
    private Mesh Particle_Mesh;
    [SerializeField]
    ComputeShader _computeShader;
    [SerializeField]
    private Material _material;
    void Start(){
        if(initTexture!=null)
        {
            width = initTexture.width;
            height = initTexture.height;
            N = width * height;
        }
        Particle[] particles= new Particle[N];
        blockPerGrid = (N + ThreadBlockSize - 1) / ThreadBlockSize;
        time = 4f;
        ParticleBuffer = new ComputeBuffer(N, 32);
        _args = new uint[5] { 0, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                int id = i * height + j;
                float x = (float)i/(width-1);
                float y = (float)j/(height-1);
                particles[id].position = new Vector3((x-0.5f),(y-0.5f),0);
                particles[id].CustomPos = new Vector3((x-0.5f),(y-0.5f) ,0);
                particles[id].uv = new Vector2(x,y);
            }
        }
        ParticleBuffer.SetData(particles);  
    }
    void Update(){
        time -= Time.deltaTime;
        transform.Rotate(new Vector3(0f, Time.deltaTime * 10f, 0f));
        if (time < 0)
            lerpt = Mathf.Lerp(lerpt, 1, 0.008f);
        updatebuffer();
        argsBuffer.SetData(_args);
        Graphics.DrawMeshInstancedIndirect(Particle_Mesh,0, _material, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)), argsBuffer);
    }
    void updatebuffer()
    {
        int kernelId = _computeShader.FindKernel("MainCS");
        _computeShader.SetFloat("_Time", time);
        _computeShader.SetBuffer(kernelId, "_ParticleBuffer", ParticleBuffer);
        _computeShader.Dispatch(kernelId, blockPerGrid, 1, 1);
        _args[0] = (uint)Particle_Mesh.GetIndexCount(0);
        _args[1] = (uint)N;
        _args[2] = (uint)Particle_Mesh.GetIndexStart(0);
        _args[3] = (uint)Particle_Mesh.GetBaseVertex(0);

        _material.SetBuffer("_ParticleBuffer", ParticleBuffer);
        _material.SetMatrix("_GameobjectMatrix", transform.localToWorldMatrix);
        _material.SetFloat("_Size", size);
        _material.SetFloat("_lerp", lerpt);
    }
}