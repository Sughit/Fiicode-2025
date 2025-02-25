using UnityEngine;

public class ColourGenerator
{
    ColourSettings settings;

    public void UpdateSettings(ColourSettings settings)
    {
        this.settings = settings;
        settings.UpdateGradientTexture();
    }

    public void UpdateElevation(MinMax elevationMinMax)
    {
        settings.planetMaterial.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
    }

    public void UpdateColours()
    {
        settings.UpdateGradientTexture();
    }
}
