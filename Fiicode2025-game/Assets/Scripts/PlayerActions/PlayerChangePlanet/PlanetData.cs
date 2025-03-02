using UnityEngine;

[System.Serializable]
public class PlanetInfo
{
    public ColourSettings colourSettings;
    public ShapeSettings shapeSettings;
}

[CreateAssetMenu(fileName = "Planet", menuName = "Custom/Planet Data", order = 1)]
public class PlanetData : ScriptableObject
{
    // Array de setări pentru cele 5 planete
    public PlanetInfo[] planets = new PlanetInfo[5];
}
