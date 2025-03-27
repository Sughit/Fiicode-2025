using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public enum MapOrientation
    {
        Vertical,
        Horizontal
    }

    [Header("Config Parameters")]
    public MapOrientation orientation = MapOrientation.Vertical;
    public int totalLevels = 15;
    public int minNodesPerLevel = 1;
    public int maxNodesPerLevel = 3;
    public float multipleParentsChance = 0.3f;

    [Header("Scene Display Parameters")]
    public GameObject nodePrefab;
    public float distanceX = 2f;
    public float distanceY = 2f;
    public Transform mapParent;
    [SerializeField] private Material material;

    private List<List<MapNode>> mapLevels = new List<List<MapNode>>();
    private Dictionary<MapNode, int> marketCooldown = new Dictionary<MapNode, int>();
    
    private int lastMarketLevel = -3;
    private int marketCount = 0;

    public void GenerateMap()
    {
        ClearMapVisuals();
        mapLevels.Clear();
        marketCount = 0;
        
        for (int lvl = 0; lvl < totalLevels; lvl++)
        {
            int numNodesInLevel = (lvl == 0) ? 1 : (lvl == totalLevels - 1) ? 1 : Random.Range(minNodesPerLevel, maxNodesPerLevel + 1);
            List<MapNode> levelNodes = new List<MapNode>();

            for (int n = 0; n < numNodesInLevel; n++)
            {
                GameObject go = Instantiate(nodePrefab, mapParent ? mapParent : transform);
                MapNode nodeComp = go.GetComponent<MapNode>();
                if (nodeComp == null)
                    nodeComp = go.AddComponent<MapNode>();

                if (orientation == MapOrientation.Vertical)
                {
                    go.transform.localPosition = new Vector3(n * distanceX, -lvl * distanceY, 0f);
                }
                else
                {
                    go.transform.localPosition = new Vector3(lvl * distanceX, -n * distanceY, 0f);
                }

                go.name = $"Node_{lvl}_{n}";
                nodeComp.nodeType = (lvl == 0) ? MapNode.NodeType.Start : DetermineNodeType(lvl, nodeComp);
                levelNodes.Add(nodeComp);
            }

            mapLevels.Add(levelNodes);
            if (lvl > 0) ConnectLevels(mapLevels[lvl - 1], levelNodes);
        }

        EnsureMinimumMarkets(3);
        IntroduceMultipleParents();
        CreateLines();
        Debug.Log($"Map generated with {totalLevels} levels and {marketCount} market nodes.");
    }

    private MapNode.NodeType DetermineNodeType(int level, MapNode node)
    {
        if (level == totalLevels - 1) return MapNode.NodeType.Boss;
        
        float rand = Random.value;
        if (rand < 0.4f) return MapNode.NodeType.Hostile;
        if (rand < 0.75f) return MapNode.NodeType.Resources;
        
        if (level - lastMarketLevel < 3 || marketCount >= 2) return MapNode.NodeType.Hostile;
        lastMarketLevel = level;
        marketCount++;
        return MapNode.NodeType.Market;
    }

    private void EnsureMinimumMarkets(int minMarkets)
    {
        while (marketCount < minMarkets)
        {
            int randomLevel = Random.Range(1, totalLevels - 1);
            List<MapNode> levelNodes = mapLevels[randomLevel];
            if (levelNodes.Count > 0)
            {
                int randomIndex = Random.Range(0, levelNodes.Count);
                if (levelNodes[randomIndex].nodeType != MapNode.NodeType.Market)
                {
                    levelNodes[randomIndex].nodeType = MapNode.NodeType.Market;
                    marketCount++;
                }
            }
        }
    }

    private void ConnectLevels(List<MapNode> previousLevel, List<MapNode> currentLevel)
    {
        foreach (var parent in previousLevel)
        {
            var child = currentLevel[Random.Range(0, currentLevel.Count)];
            CreateParentChildLink(parent, child);
        }

        foreach (var child in currentLevel)
        {
            if (child.parents.Count == 0)
            {
                var randomParent = previousLevel[Random.Range(0, previousLevel.Count)];
                CreateParentChildLink(randomParent, child);
            }
        }
    }

    private void CreateParentChildLink(MapNode parent, MapNode child)
    {
        if (!parent.children.Contains(child)) parent.children.Add(child);
        if (!child.parents.Contains(parent)) child.parents.Add(parent);
    }

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

    private void CreateLines()
    {
        foreach (var level in mapLevels)
        {
            foreach (var parentNode in level)
            {
                foreach (var childNode in parentNode.children)
                {
                    if (childNode == null) continue;

                    GameObject lineObj = new GameObject($"Line_{parentNode.name}_to_{childNode.name}");
                    lineObj.transform.SetParent(mapParent ? mapParent : transform, false);
                    var lr = lineObj.AddComponent<LineRenderer>();
                    lr.positionCount = 2;
                    lr.useWorldSpace = false;
                    lr.startWidth = 0.05f;
                    lr.endWidth = 0.05f;
                    lr.material = material;

                    Vector3 pPos = parentNode.transform.localPosition;
                    Vector3 cPos = childNode.transform.localPosition;
                    lr.SetPosition(0, pPos);
                    lr.SetPosition(1, cPos);
                }
            }
        }
    }

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
}
