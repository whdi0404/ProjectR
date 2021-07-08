using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.U2D;

public class TileGroup : MonoBehaviour
{
    private struct MeshProperty
    {
        private static int size = 0;
        public static int Size()
        {
            if (size == 0)
                size = Marshal.SizeOf<MeshProperty>();
            return size;
        }

        public int tex;
    }

    private static Mesh quadMesh = null;
    private static Mesh QuadMesh
    {
        get
        {
            if (quadMesh == null)
            {
                quadMesh = new Mesh();

                quadMesh.SetVertices(new Vector3[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0)
                });
                quadMesh.SetUVs(0, new Vector2[]
                {
                    new Vector2(0,0),
                    new Vector2(1,0),
                    new Vector2(0,1),
                    new Vector2(1,1),
                });
                quadMesh.SetIndices(new int[] { 0, 2, 1, 1, 2, 3 }, MeshTopology.Triangles, 0);
            }

            return quadMesh;
        }
    }

    private AtlasObject atlasObject;

    private Material cloneMat;

    private Bounds drawBounds;
    // Start is called before the first frame update

    private int width, height;

    private MeshProperty[] meshProperties;
    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;

    private Vector3 prevPos;

    void Awake()
    {
    }

    public void Initialize(AtlasObject atlasObject, Material material, int width, int height, string defaultTile = null)
    {
        this.atlasObject = atlasObject;
        cloneMat = new Material(material);
        this.width = width;
        this.height = height;

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = QuadMesh.GetIndexCount(0);
        args[1] = (uint)(width * height);
        args[2] = QuadMesh.GetIndexStart(0);
        args[3] = QuadMesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        meshProperties = new MeshProperty[width * height];

        int r = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                MeshProperty props = new MeshProperty();
                if (string.IsNullOrEmpty(defaultTile) == true)
                {
                    props.tex = 0;
                }
                else
                {
                    props.tex = atlasObject.GetSpriteIndex(defaultTile);
                }
                meshProperties[x + y * width] = props;
            }
        }

        meshPropertiesBuffer = new ComputeBuffer(width * height, MeshProperty.Size());
        meshPropertiesBuffer.SetData(meshProperties);
        cloneMat.SetBuffer("_Properties", meshPropertiesBuffer);
        cloneMat.SetTexture("_AtlasTexture", atlasObject.AtlasTexture);
        cloneMat.SetBuffer("_AtlasProperties", atlasObject.AtlasPropertiesBuffer);
        cloneMat.SetInt("_Width", width);
        cloneMat.SetInt("_Height", height);
        OnPositionChanged();
    }

    public void SetTile(string spriteId, int x, int y)
    {
        int arrayIndex = x + y * width;
        meshProperties[arrayIndex].tex = atlasObject.GetSpriteIndex(spriteId);
    }

    public void SetAllTile(string spriteId)
    {
        int spriteIndex = atlasObject.GetSpriteIndex(spriteId);
        for (int i = 0; i < meshProperties.Length; ++i)
            meshProperties[i].tex = spriteIndex;
    }

    public void Apply()
    {
        meshPropertiesBuffer.SetData(meshProperties);
    }

    private void Update()
    {
        if (prevPos != transform.position)
            OnPositionChanged();
        
        Graphics.DrawMeshInstancedIndirect(QuadMesh, 0, cloneMat, drawBounds, argsBuffer);
    }

    private void OnPositionChanged()
    {
        drawBounds.SetMinMax(transform.position, transform.position + new Vector3(width, height, 0));
        cloneMat.SetVector("_Pos", transform.position);
        prevPos = transform.position;
    }

    private void OnDestroy()
    {
        if (meshPropertiesBuffer != null)
        {
            meshPropertiesBuffer.Release();
        }
        meshPropertiesBuffer = null;

        if (argsBuffer != null)
        {
            argsBuffer.Release();
        }
        argsBuffer = null;
    }
}
