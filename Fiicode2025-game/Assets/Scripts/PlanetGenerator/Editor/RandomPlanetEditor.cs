#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomPlanet))]
public class RandomPlanetEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        RandomPlanet rp = (RandomPlanet)target;
        if (GUILayout.Button("Generate Planet")) {
            rp.GeneratePlanet();
        }
    }
}
#endif
