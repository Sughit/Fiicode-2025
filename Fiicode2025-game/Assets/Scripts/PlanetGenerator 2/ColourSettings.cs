using UnityEngine;

[CreateAssetMenu()]
public class ColourSettings : ScriptableObject
{
    public Gradient gradient;
    public Material planetMaterial;

    [HideInInspector]
    public Texture2D gradientTexture;

    public const int textureResolution = 256;

    public void UpdateGradientTexture()
    {
        if (gradientTexture == null || gradientTexture.width != textureResolution)
        {
            gradientTexture = new Texture2D(textureResolution, 1, TextureFormat.RGBA32, false);
            gradientTexture.wrapMode = TextureWrapMode.Clamp;
        }

        for (int i = 0; i < textureResolution; i++)
        {
            Color color = gradient.Evaluate(i / (textureResolution - 1f));
            gradientTexture.SetPixel(i, 0, color);
        }
        gradientTexture.Apply();
        planetMaterial.SetTexture("_GradientTex", gradientTexture);
    }
}
