using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public enum MapOrientation
    {
        Vertical,
        Horizontal
    }

    [Header("Parametrii de configurare")]
    public MapOrientation orientation = MapOrientation.Vertical;

    [Tooltip("Dacă orientarea e Verticală, 'numberOfColumns' = câte niveluri sus-jos")]
    public int numberOfColumns = 5; // ex: 5 niveluri
    [Tooltip("Dacă orientarea e Verticală, 'numberOfRows' = ignorat, sau invers pt orizontal")]
    public int numberOfRows = 5;

    [Tooltip("Minim noduri pe fiecare nivel (coloană/rând)")]
    public int minNodesPerLevel = 1;

    [Tooltip("Maxim noduri pe fiecare nivel (coloană/rând)")]
    public int maxNodesPerLevel = 3;

    [Range(0f, 1f)]
    [Tooltip("Probabilitatea de a avea încă un părinte (pentru un nod din nivelul curent)")]
    public float multipleParentsChance = 0.3f;

    [Header("Parametrii de afișare în scenă")]
    [Tooltip("Prefab ce conține scriptul MapNode + un Collider")]
    public GameObject nodePrefab;

    [Tooltip("Distanța între niveluri pe axa X sau Y, în funcție de orientare")]
    public float distanceX = 2f;
    public float distanceY = 2f;

    [Tooltip("Părintele (empty) sub care vom pune nodurile și liniile")]
    public Transform mapParent;

    // Reținem nodurile generate, pe niveluri
    private List<List<MapNode>> mapLevels = new List<List<MapNode>>();

    /// <summary>
    /// Apelează asta din Editor (prin buton) sau din cod, pentru a genera harta.
    /// </summary>
    public void GenerateMap()
    {
        // 1. Curățăm ce exista anterior
        ClearMapVisuals();
        mapLevels.Clear();

        // 2. Determinăm câte niveluri facem, pe baza orientării
        int totalLevels = (orientation == MapOrientation.Vertical) ? numberOfColumns : numberOfRows;

        // 3. Generăm nodurile (GameObject-uri cu MapNode) nivel cu nivel
        for (int lvl = 0; lvl < totalLevels; lvl++)
        {
            int numNodesInLevel = Random.Range(minNodesPerLevel, maxNodesPerLevel + 1);
            List<MapNode> levelNodes = new List<MapNode>();

            for (int n = 0; n < numNodesInLevel; n++)
            {
                // Instanțiem prefab-ul
                GameObject go = Instantiate(nodePrefab, mapParent ? mapParent : transform);

                // Scriptul MapNode
                MapNode nodeComp = go.GetComponent<MapNode>();
                if (nodeComp == null)
                    nodeComp = go.AddComponent<MapNode>(); // în caz că lipsește

                // Poziționăm
                if (orientation == MapOrientation.Vertical)
                {
                    // lvl -> Y (vertical), n -> X
                    go.transform.localPosition = new Vector3(
                        n * distanceX, 
                        -lvl * distanceY, 
                        0f
                    );
                }
                else // Horizontal
                {
                    // lvl -> X, n -> Y
                    go.transform.localPosition = new Vector3(
                        lvl * distanceX,
                        -n * distanceY,
                        0f
                    );
                }

                go.name = $"Node_{lvl}_{n}";
                levelNodes.Add(nodeComp);
            }

            mapLevels.Add(levelNodes);

            // Conectăm la nivelul anterior
            if (lvl > 0)
            {
                ConnectLevels(mapLevels[lvl - 1], levelNodes);
            }
        }

        // 4. Introducem "multiple parents" cu o probabilitate
        IntroduceMultipleParents();

        // 5. Desenăm line renderer între nodurile părinte-copil
        CreateLines();

        Debug.Log($"Map generated with {totalLevels} levels.");
    }

    /// <summary>
    /// Conectăm nodurile de la nivelul anterior cu noduri random din nivelul curent,
    /// apoi ne asigurăm că fiecare nod din nivelul curent are minim un părinte.
    /// </summary>
    private void ConnectLevels(List<MapNode> previousLevel, List<MapNode> currentLevel)
    {
        // a) Fiecare nod din previousLevel -> un nod random din currentLevel
        foreach (var parent in previousLevel)
        {
            var child = currentLevel[Random.Range(0, currentLevel.Count)];
            CreateParentChildLink(parent, child);
        }

        // b) Asigurăm că fiecare nod din currentLevel are cel puțin 1 părinte
        foreach (var child in currentLevel)
        {
            if (child.parents.Count == 0)
            {
                var randomParent = previousLevel[Random.Range(0, previousLevel.Count)];
                CreateParentChildLink(randomParent, child);
            }
        }
    }

    /// <summary>
    /// Creează relația părinte-copil la nivel de MapNode.
    /// </summary>
    private void CreateParentChildLink(MapNode parent, MapNode child)
    {
        if (!parent.children.Contains(child))
            parent.children.Add(child);
        if (!child.parents.Contains(parent))
            child.parents.Add(parent);
    }

    /// <summary>
    /// Cu o anumită probabilitate, mai adăugăm un părinte din nivelul anterior
    /// pentru fiecare nod (de la al doilea nivel în sus).
    /// </summary>
    private void IntroduceMultipleParents()
    {
        for (int lvl = 1; lvl < mapLevels.Count; lvl++)
        {
            var currentLevel = mapLevels[lvl];
            var previousLevel = mapLevels[lvl - 1];

            foreach (var node in currentLevel)
            {
                if (Random.value < multipleParentsChance)
                {
                    var additionalParent = previousLevel[Random.Range(0, previousLevel.Count)];
                    if (!node.parents.Contains(additionalParent))
                    {
                        CreateParentChildLink(additionalParent, node);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Desenează linii (LineRenderer) între părinte și copil, pentru fiecare nod.
    /// </summary>
    private void CreateLines()
    {
        // Parcurgem toate nivelurile
        for (int lvl = 0; lvl < mapLevels.Count; lvl++)
        {
            // Parcurgem fiecare nod (părinte)
            foreach (MapNode parentNode in mapLevels[lvl])
            {
                // Desenăm linie pentru fiecare copil
                foreach (MapNode childNode in parentNode.children)
                {
                    if (childNode == null) continue;

                    GameObject lineObj = new GameObject(
                        $"Line_{parentNode.name}_to_{childNode.name}"
                    );

                    // Punem linia sub același părinte (mapParent) pentru organizare
                    lineObj.transform.SetParent(mapParent ? mapParent : transform, false);

                    var lr = lineObj.AddComponent<LineRenderer>();
                    lr.positionCount = 2;
                    lr.useWorldSpace = false;
                    lr.startWidth = 0.05f;
                    lr.endWidth = 0.05f;

                    // Poziții locale
                    Vector3 pPos = parentNode.transform.localPosition;
                    Vector3 cPos = childNode.transform.localPosition;
                    lr.SetPosition(0, pPos);
                    lr.SetPosition(1, cPos);
                }
            }
        }
    }

    /// <summary>
    /// Șterge tot ce a fost generat anterior (noduri, linii etc.).
    /// </summary>
    private void ClearMapVisuals()
    {
        Transform p = mapParent ? mapParent : transform;
        var toDestroy = new List<Transform>();
        foreach (Transform child in p)
            toDestroy.Add(child);

        foreach (var c in toDestroy)
        {
            DestroyImmediate(c.gameObject);
        }

        mapLevels.Clear();
    }

    /// <summary>
    /// Accesezi nodurile, dacă ai nevoie de ele din alt script.
    /// </summary>
    public List<List<MapNode>> GetMapLevels()
    {
        return mapLevels;
    }
}
