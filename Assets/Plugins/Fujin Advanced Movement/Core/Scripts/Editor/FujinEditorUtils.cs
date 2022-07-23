using UnityEngine;

/// <summary>
/// Collection of custom Editor Utilities
/// </summary>
public class FujinEditorUtils
{
    /// <summary>
    /// MakeTex
    /// Generates a <code>Texture2D</code> from the provided params
    /// </summary>
    /// <param name="width">The width of the desired texture</param>
    /// <param name="height">The height of the desired texture</param>
    /// <param name="col">The color of the desired texture</param>
    /// <returns></returns>
    public static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
