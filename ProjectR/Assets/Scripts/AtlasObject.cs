using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasObject : SerializedScriptableObject
{
    private struct AtlasProperty
    {
        private static int size = 0;
        public static int Size()
        {
            if (size == 0)
                size = Marshal.SizeOf<AtlasProperty>();
            return size;
        }

        public int drawOrder;
        public Vector2 uvStart;
        public Vector2 uvEnd;
    }

    [OdinSerialize]
    public SpriteAtlas spriteAtlas { get; private set; }

    [NonSerialized]
    private ComputeBuffer atlasPropertiesBuffer = null;
    public ComputeBuffer AtlasPropertiesBuffer
    {
        get
        {
            if (atlasPropertiesBuffer == null)
            {
                Initialize();
            }
            return atlasPropertiesBuffer;
        }
    }

    [NonSerialized]
    Texture2D atlasTexture;
    public Texture2D AtlasTexture
    {
        get
        {
            if (atlasTexture == null)
            {
                Initialize();
            }
            return atlasTexture;
        }
    }

    [NonSerialized]
    private SmartDictionary<string, int> spriteIndex;

    public void Initialize()
    {
        spriteIndex = new SmartDictionary<string, int>();

        var atlasInfos = TableManager.GetTable<TileAtlasInfoTable>().All();
        int textureCount = atlasInfos.Count();
        AtlasProperty[] atlasProperties = new AtlasProperty[textureCount];
        int index = 0;
        foreach (AtlasInfoDescriptor atlasInfo in atlasInfos)
        {
            AtlasProperty prop = new AtlasProperty();

            prop.drawOrder = atlasInfo.DrawOrder;

            Sprite sprite = spriteAtlas.GetSprite(atlasInfo.Id);
            Vector2[] uvs = SpriteUtility.GetSpriteUVs(sprite, true);
            if (atlasTexture == null)
                atlasTexture = SpriteUtility.GetSpriteTexture(sprite, true);

            Vector2 uvStart = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 uvEnd = new Vector2(float.MinValue, float.MinValue);
            foreach (var uv in uvs)
            {
                if (uv.x <= uvStart.x && uv.y <= uvStart.y)
                    uvStart = uv;

                if (uv.x >= uvEnd.x && uv.y >= uvEnd.y)
                    uvEnd = uv;
            }
            prop.uvStart = uvStart;
            prop.uvEnd = uvEnd;

            spriteIndex[atlasInfo.Id] = index;
            atlasProperties[index++] = prop;
        }

        atlasPropertiesBuffer = new ComputeBuffer(textureCount, AtlasProperty.Size());
        atlasPropertiesBuffer.SetData(atlasProperties);

    }

    public int GetSpriteIndex(string spriteId)
    {
        if (spriteIndex == null)
            Initialize();

        return spriteIndex[spriteId];
    }

    public void Destroy()
    {
        if(atlasPropertiesBuffer != null)
            atlasPropertiesBuffer.Release();
    }

    private void OnDisable()
    {
        Destroy();
    }
}
