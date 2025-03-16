using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // Pentru butonul din Inspector (metodă custom)

public class MapGenerator : MonoBehaviour
{
    // Enum pentru orientarea hărții
    public enum MapOrientation
    {
        Horizontal,
        Vertical
    }

    [Header("Configurare Harta")]
    public MapOrientation orientation = MapOrientation.Horizontal;
    public int rows = 5;         // Numărul de niveluri (rânduri)
    public int cols = 3;         // Numărul total de coloane disponibile
    public float xSpacing = 2f;  // Spațierea pe orizontală
    public float ySpacing = 2f;  // Spațierea pe verticală
    [Tooltip("Numărul maxim de conexiuni pe care le poate avea un nod ca sursă")]
    public int maxConnectionsPerNode = 2;

    [Header("Referințe")]
    public GameObject nodePrefab;   // Prefab pentru noduri (setat în Inspector)
    public Transform mapParent;     // Părinte pentru noduri și conexiuni (opțional)

    // Enum pentru tipurile de noduri
    public enum NodeType
    {
        ParadisePlanet,
        HotPlanet,
        ColdPlanet,
        RandomPlanet,
        Market,
        EnemyPlanet
    }

    // Clasa care definește un nod din hartă
    public class MapNode
    {
        public NodeType type;         // Tipul nodului
        public Vector2 position;      // Poziția în scenă
        public int rowIndex;          // Indexul rândului
        public int columnIndex;       // Indexul coloanei
        public GameObject nodeObject; // Referința la GameObject-ul instanțiat
        public List<MapNode> connections = new List<MapNode>(); // Conexiuni către alte noduri
    }

    // Lista de rânduri, fiecare rând fiind o listă de MapNode
    public List<List<MapNode>> mapNodes = new List<List<MapNode>>();

    void Start()
    {
        GenerateMap();
    }

    // Buton expus în Inspector pentru a regenera harta.
    [ContextMenu("Regenerate Map")]
    public void RegenerateMap()
    {
        ClearMap();
        GenerateMap();
    }

    void ClearMap()
    {
        foreach (Transform child in mapParent)
        {
            Destroy(child.gameObject);
        }
        mapNodes.Clear();
    }

    void GenerateMap()
    {
        // 1. Generăm nodurile (interpretare orizontală sau verticală)
        if (orientation == MapOrientation.Horizontal)
        {
            GenerateNodesHorizontal();
        }
        else
        {
            GenerateNodesVertical();
        }

        // 2. Conectăm nodurile inițial (conexiuni cu restricția de diferență de 1..2)
        if (orientation == MapOrientation.Horizontal)
        {
            ConnectNodesHorizontal();
        }
        else
        {
            ConnectNodesVertical();
        }

        // 3. Asigurăm că graful este conex (pas de unificare, fără restricții de distanță)
        EnsureGraphConnected();

        // 4. După ce graful este conex, eliminăm conexiunile care încalcă regula:
        //    dacă un nod are deja o conexiune și alta către un nod mult prea îndepărtat (diferența > 2),
        //    atunci eliminăm conexiunea ce este prea îndepărtată.
        RemoveFarConnections();
    }

    void GenerateNodesHorizontal()
    {
        for (int r = 0; r < rows; r++)
        {
            List<MapNode> currentRowNodes = new List<MapNode>();
            int nodeCount = Random.Range(1, cols + 1);
            List<int> availableColumns = new List<int>();
            for (int c = 0; c < cols; c++)
            {
                availableColumns.Add(c);
            }
            Shuffle(availableColumns);
            for (int i = 0; i < nodeCount; i++)
            {
                int colIndex = availableColumns[i];
                MapNode newNode = new MapNode();
                newNode.type = GetRandomNodeType();
                newNode.rowIndex = r;
                newNode.columnIndex = colIndex;
                float xPos = colIndex * xSpacing - ((cols - 1) * xSpacing / 2f);
                float yPos = -r * ySpacing;
                newNode.position = new Vector2(xPos, yPos);
                if (nodePrefab != null)
                {
                    GameObject nodeObj = Instantiate(nodePrefab, new Vector3(xPos, yPos, 0), Quaternion.identity, mapParent);
                    nodeObj.name = newNode.type.ToString() + $" (Row {r}, Col {colIndex})";
                    newNode.nodeObject = nodeObj;
                }
                currentRowNodes.Add(newNode);
            }
            mapNodes.Add(currentRowNodes);
        }
        EnsureAllColumnsRepresentedHorizontal();
    }

    void GenerateNodesVertical()
    {
        // Inversăm rolurile: "rows" devine numărul de coloane, "cols" numărul de rânduri
        for (int c = 0; c < cols; c++)
        {
            List<MapNode> currentRowNodes = new List<MapNode>();
            int nodeCount = Random.Range(1, rows + 1);
            List<int> availableRows = new List<int>();
            for (int r = 0; r < rows; r++)
            {
                availableRows.Add(r);
            }
            Shuffle(availableRows);
            for (int i = 0; i < nodeCount; i++)
            {
                int rowIdx = availableRows[i];
                MapNode newNode = new MapNode();
                newNode.type = GetRandomNodeType();
                newNode.columnIndex = c;
                newNode.rowIndex = rowIdx;
                float xPos = rowIdx * xSpacing - ((rows - 1) * xSpacing / 2f);
                float yPos = -c * ySpacing;
                newNode.position = new Vector2(xPos, yPos);
                if (nodePrefab != null)
                {
                    GameObject nodeObj = Instantiate(nodePrefab, new Vector3(xPos, yPos, 0), Quaternion.identity, mapParent);
                    nodeObj.name = newNode.type.ToString() + $" (Vert Col {c}, Row {rowIdx})";
                    newNode.nodeObject = nodeObj;
                }
                currentRowNodes.Add(newNode);
            }
            mapNodes.Add(currentRowNodes);
        }
        EnsureAllColumnsRepresentedVertical();
    }

    void ConnectNodesHorizontal()
    {
        for (int r = 0; r < mapNodes.Count - 1; r++)
        {
            List<MapNode> currentRow = mapNodes[r];
            List<MapNode> nextRow = mapNodes[r + 1];
            bool[] connected = new bool[nextRow.Count];
            foreach (MapNode source in currentRow)
            {
                List<MapNode> allowedTargets = nextRow.FindAll(t =>
                    Mathf.Abs(t.columnIndex - source.columnIndex) >= 1 &&
                    Mathf.Abs(t.columnIndex - source.columnIndex) <= 2
                );
                if (allowedTargets.Count > 0 && source.connections.Count < maxConnectionsPerNode)
                {
                    MapNode targetNode = allowedTargets[Random.Range(0, allowedTargets.Count)];
                    source.connections.Add(targetNode);
                    int idx = nextRow.IndexOf(targetNode);
                    connected[idx] = true;
                    DrawConnection(source.position, targetNode.position);
                }
            }
            // Fallback minimal
            for (int i = 0; i < nextRow.Count; i++)
            {
                if (!connected[i])
                {
                    MapNode target = nextRow[i];
                    int nextCol = target.columnIndex + 1;
                    if (nextCol < cols)
                    {
                        List<MapNode> nextColSources = currentRow.FindAll(s =>
                            s.columnIndex == nextCol && s.connections.Count < maxConnectionsPerNode
                        );
                        if (nextColSources.Count > 0)
                        {
                            int randCount = Random.Range(1, nextColSources.Count + 1);
                            List<MapNode> subset = GetRandomSubset(nextColSources, randCount);
                            bool connectionMade = false;
                            foreach (MapNode s in subset)
                            {
                                if (s.connections.Count < maxConnectionsPerNode)
                                {
                                    s.connections.Add(target);
                                    DrawConnection(s.position, target.position);
                                    connectionMade = true;
                                }
                            }
                            if (connectionMade) connected[i] = true;
                        }
                    }
                }
            }
        }
    }

    void ConnectNodesVertical()
    {
        for (int c = 0; c < mapNodes.Count - 1; c++)
        {
            List<MapNode> currentCol = mapNodes[c];
            List<MapNode> nextCol = mapNodes[c + 1];
            bool[] connected = new bool[nextCol.Count];
            foreach (MapNode source in currentCol)
            {
                List<MapNode> allowedTargets = nextCol.FindAll(t =>
                    Mathf.Abs(t.rowIndex - source.rowIndex) >= 1 &&
                    Mathf.Abs(t.rowIndex - source.rowIndex) <= 2
                );
                if (allowedTargets.Count > 0 && source.connections.Count < maxConnectionsPerNode)
                {
                    MapNode targetNode = allowedTargets[Random.Range(0, allowedTargets.Count)];
                    source.connections.Add(targetNode);
                    int idx = nextCol.IndexOf(targetNode);
                    connected[idx] = true;
                    DrawConnection(source.position, targetNode.position);
                }
            }
            // Fallback minimal
            for (int i = 0; i < nextCol.Count; i++)
            {
                if (!connected[i])
                {
                    MapNode target = nextCol[i];
                    int nextRow = target.rowIndex + 1;
                    if (nextRow < rows)
                    {
                        List<MapNode> nextRowSources = currentCol.FindAll(s =>
                            s.rowIndex == nextRow && s.connections.Count < maxConnectionsPerNode
                        );
                        if (nextRowSources.Count > 0)
                        {
                            int randCount = Random.Range(1, nextRowSources.Count + 1);
                            List<MapNode> subset = GetRandomSubset(nextRowSources, randCount);
                            bool connectionMade = false;
                            foreach (MapNode s in subset)
                            {
                                if (s.connections.Count < maxConnectionsPerNode)
                                {
                                    s.connections.Add(target);
                                    DrawConnection(s.position, target.position);
                                    connectionMade = true;
                                }
                            }
                            if (connectionMade) connected[i] = true;
                        }
                    }
                }
            }
        }
    }

    void EnsureAllColumnsRepresentedHorizontal()
    {
        HashSet<int> represented = new HashSet<int>();
        foreach (var rowList in mapNodes)
        {
            foreach (var node in rowList)
            {
                represented.Add(node.columnIndex);
            }
        }
        for (int c = 0; c < cols; c++)
        {
            if (!represented.Contains(c))
            {
                for (int r = 0; r < mapNodes.Count; r++)
                {
                    if (mapNodes[r].Count < cols)
                    {
                        MapNode extra = new MapNode();
                        extra.type = GetRandomNodeType();
                        extra.rowIndex = r;
                        extra.columnIndex = c;
                        float xPos = c * xSpacing - ((cols - 1) * xSpacing / 2f);
                        float yPos = -r * ySpacing;
                        extra.position = new Vector2(xPos, yPos);
                        if (nodePrefab != null)
                        {
                            GameObject nodeObj = Instantiate(nodePrefab, new Vector3(xPos, yPos, 0), Quaternion.identity, mapParent);
                            nodeObj.name = extra.type.ToString() + $" (Row {r}, Col {c}) [Extra]";
                            extra.nodeObject = nodeObj;
                        }
                        mapNodes[r].Add(extra);
                        break;
                    }
                }
            }
        }
    }

    void EnsureAllColumnsRepresentedVertical()
    {
        HashSet<int> represented = new HashSet<int>();
        foreach (var colList in mapNodes)
        {
            foreach (var node in colList)
            {
                represented.Add(node.rowIndex);
            }
        }
        for (int r = 0; r < rows; r++)
        {
            if (!represented.Contains(r))
            {
                for (int c = 0; c < mapNodes.Count; c++)
                {
                    if (mapNodes[c].Count < rows)
                    {
                        MapNode extra = new MapNode();
                        extra.type = GetRandomNodeType();
                        extra.columnIndex = c;
                        extra.rowIndex = r;
                        float xPos = r * xSpacing - ((rows - 1) * xSpacing / 2f);
                        float yPos = -c * ySpacing;
                        extra.position = new Vector2(xPos, yPos);
                        if (nodePrefab != null)
                        {
                            GameObject nodeObj = Instantiate(nodePrefab, new Vector3(xPos, yPos, 0), Quaternion.identity, mapParent);
                            nodeObj.name = extra.type.ToString() + $" (Vert Col {c}, Row {r}) [Extra]";
                            extra.nodeObject = nodeObj;
                        }
                        mapNodes[c].Add(extra);
                        break;
                    }
                }
            }
        }
    }

    void EnsureGraphConnected()
    {
        List<MapNode> allNodes = new List<MapNode>();
        foreach (var rowList in mapNodes)
        {
            allNodes.AddRange(rowList);
        }
        if (allNodes.Count == 0) return;
        HashSet<MapNode> visited = new HashSet<MapNode>();
        DFS(allNodes[0], visited);
        while (visited.Count < allNodes.Count)
        {
            MapNode unvisited = allNodes.Find(n => !visited.Contains(n));
            MapNode connector = new List<MapNode>(visited)[Random.Range(0, visited.Count)];
            connector.connections.Add(unvisited); // Conexiune forțată, fără restricții
            DrawConnection(connector.position, unvisited.position);
            DFS(unvisited, visited);
        }
    }

    void DFS(MapNode node, HashSet<MapNode> visited)
    {
        if (visited.Contains(node)) return;
        visited.Add(node);
        foreach (MapNode neighbor in node.connections)
        {
            DFS(neighbor, visited);
        }
        foreach (var rowList in mapNodes)
        {
            foreach (MapNode potential in rowList)
            {
                if (potential.connections.Contains(node))
                {
                    DFS(potential, visited);
                }
            }
        }
    }

    // Metoda care elimină conexiunile prea îndepărtate.
    // Dacă un nod are mai multe conexiuni și unele au o diferență (pe coloane în modul orizontal,
    // sau pe rânduri în modul vertical) mai mare de 2, atunci se elimină conexiunile ce nu respectă limita.
    void RemoveFarConnections()
    {
        foreach (var rowList in mapNodes)
        {
            foreach (MapNode node in rowList)
            {
                // Aplicăm regula doar dacă nodul are cel puțin 2 conexiuni.
                if (node.connections.Count > 1)
                {
                    // Facem o copie a listei pentru a itera fără probleme.
                    List<MapNode> connsCopy = new List<MapNode>(node.connections);
                    foreach (MapNode target in connsCopy)
                    {
                        if (orientation == MapOrientation.Horizontal)
                        {
                            if (Mathf.Abs(node.columnIndex - target.columnIndex) > 2)
                            {
                                node.connections.Remove(target);
                                // Opțional: poți șterge și linia desenată, dacă păstrezi referințe la ea.
                            }
                        }
                        else // Vertical
                        {
                            if (Mathf.Abs(node.rowIndex - target.rowIndex) > 2)
                            {
                                node.connections.Remove(target);
                            }
                        }
                    }
                }
            }
        }
    }

    List<MapNode> GetRandomSubset(List<MapNode> list, int count)
    {
        List<MapNode> copy = new List<MapNode>(list);
        List<MapNode> subset = new List<MapNode>();
        for (int i = 0; i < count && copy.Count > 0; i++)
        {
            int rand = Random.Range(0, copy.Count);
            subset.Add(copy[rand]);
            copy.RemoveAt(rand);
        }
        return subset;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            T temp = list[r];
            list[r] = list[i];
            list[i] = temp;
        }
    }

    NodeType GetRandomNodeType()
    {
        int enumLength = System.Enum.GetValues(typeof(NodeType)).Length;
        int randomIndex = Random.Range(0, enumLength);
        return (NodeType)randomIndex;
    }

    void DrawConnection(Vector2 start, Vector2 end)
    {
        GameObject lineObj = new GameObject("Connection");
        lineObj.transform.parent = mapParent;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(start.x, start.y, 0));
        lr.SetPosition(1, new Vector3(end.x, end.y, 0));
        lr.widthMultiplier = 0.1f;
        // Poți seta materialul sau culoarea aici, dacă este necesar.
    }
}
