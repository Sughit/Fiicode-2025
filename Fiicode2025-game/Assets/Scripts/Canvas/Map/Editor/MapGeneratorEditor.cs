using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Desenăm Inspector-ul standard
        base.OnInspectorGUI();

        // Referința la componenta noastră
        MapGenerator generator = (MapGenerator)target;

        // Butonul de generare
        if (GUILayout.Button("Generate Map"))
        {
            generator.GenerateMap();

            // Marcam obiectul ca “dirty” pentru a salva eventualele schimbări în scenă
            EditorUtility.SetDirty(generator);
        }
    }
}
