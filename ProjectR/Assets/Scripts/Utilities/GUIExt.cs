using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GUIExt
{
    private static SmartDictionary<Color, GUIStyle> staticTextures = new SmartDictionary<Color, GUIStyle>();

    public static void DrawRect(Rect rect, Color color)
    {
        if (staticTextures.TryGetValue(color, out GUIStyle style) == false)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();

            style = new GUIStyle();
            style.normal.background = texture;

            staticTextures.Add(color, style);
        }

        GUI.Box(rect, GUIContent.none, style);
    }
}