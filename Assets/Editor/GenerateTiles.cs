using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class GenerateTiles : MonoBehaviour
{
    [MenuItem("BlockinBeats/Generate Level Tiles")]
    static void Generate()
    {
        var levelsPath = $"Assets\\Resources\\Levels\\";
        foreach (var dir in Directory.GetDirectories(levelsPath))
        {
            Func<string,Texture2D> loadTile = which =>
            {
                var tile = Directory.GetFiles(dir, $"{which}.png").Single();
                if (tile == null)
                {
                    Debug.LogError($"Failed to find {which} tile .png");
                    return null;
                }

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(tile);
                return GetTextureCopy(tex);
            };
            var black = loadTile("Black");
            var white = loadTile("White");
            if (black == null || white == null)
            {
                return;
            }

            Debug.Log($"Generating tiles in level '{Path.GetFileName(dir)}'");

            // generate white/black:
            var blackWhite = new Texture2D(black.width, black.height);

            // fill top triangle with black, bottom with white:
            for (int x = 0; x < blackWhite.width; x++)
            {
                for (int y = 0; y < blackWhite.height; y++)
                {
                    if (x > y)
                        blackWhite.SetPixel(x, y, black.GetPixel(x, y));
                    else
                        blackWhite.SetPixel(x, y, white.GetPixel(x, y));
                }
            }

            var blackWhitePath = Path.Combine(dir, "BlackWhite.png");
            var png = blackWhite.EncodeToPNG();
            File.WriteAllBytes(blackWhitePath, png);
        }
    }

    static Texture2D GetTextureCopy(Texture2D texture)
    {
        // Create a temporary RenderTexture of the same size as the texture
        RenderTexture tmp = RenderTexture.GetTemporary(
                            texture.width,
                            texture.height,
                            0,
                            RenderTextureFormat.Default,
                            RenderTextureReadWrite.Linear);

        // Blit the pixels on texture to the RenderTexture
        Graphics.Blit(texture, tmp);
        // Backup the currently set RenderTexture
        RenderTexture previous = RenderTexture.active;
        // Set the current RenderTexture to the temporary one we created
        RenderTexture.active = tmp;
        // Create a new readable Texture2D to copy the pixels to it
        Texture2D myTexture2D = new Texture2D(texture.width, texture.height);
        // Copy the pixels from the RenderTexture to the new Texture
        myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        myTexture2D.Apply();
        // Reset the active RenderTexture
        RenderTexture.active = previous;
        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(tmp);

        return myTexture2D;
    }
}