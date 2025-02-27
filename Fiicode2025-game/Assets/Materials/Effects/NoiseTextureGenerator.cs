using UnityEngine;

public class NoiseTextureGenerator : MonoBehaviour
{
    [Header("Noise Texture Settings")]
    public int textureWidth = 256;
    public int textureHeight = 256;
    public float scale = 20f;

    [Header("Material Settings")]
    // Materialul în care vrei să aplici textura de zgomot.
    public Material targetMaterial;

    void Start()
    {
        // Generează textura de zgomot.
        Texture2D noiseTex = GenerateNoiseTexture(textureWidth, textureHeight, scale);

        // Setează textura în material, la parametrul _NoiseTex.
        if (targetMaterial != null)
        {
            targetMaterial.SetTexture("_NoiseTex", noiseTex);
        }
    }

    Texture2D GenerateNoiseTexture(int width, int height, float scale)
    {
        Texture2D texture = new Texture2D(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculăm coordonatele în funcție de scala dată.
                float xCoord = (float)x / width * scale;
                float yCoord = (float)y / height * scale;
                // Folosim Perlin Noise pentru a genera valoarea de zgomot.
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                Color color = new Color(sample, sample, sample);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }
}
