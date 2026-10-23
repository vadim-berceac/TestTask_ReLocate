using System.IO;
using UnityEditor;
using UnityEngine;

public static class GhostNoiseGenerator
{
    private const int Size = 256;

    [MenuItem("Tools/Generate Ghost Noise")]
    public static void Generate()
    {
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false, true);

        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var u = x / (float)Size;
                var v = y / (float)Size;

                var a = Fbm(u, v);
                var b = Fbm(u - 1f, v);
                var c = Fbm(u, v - 1f);
                var d = Fbm(u - 1f, v - 1f);

                var n = Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
                tex.SetPixel(x, y, new Color(n, n, n, 1f));
            }
        }

        tex.Apply();
        File.WriteAllBytes("Assets/GhostNoise.png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.Refresh();
    }

    private static float Fbm(float u, float v)
    {
        var p1 = Mathf.PerlinNoise((u + 10f) * 4f, (v + 10f) * 4f);
        var p2 = Mathf.PerlinNoise((u + 10f) * 8f + 37f, (v + 10f) * 8f + 37f);
        return p1 * 0.65f + p2 * 0.35f;
    }
}