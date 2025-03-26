using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapNode : MonoBehaviour
{
    public enum NodeType { Resources, Hostile, Market }

    [Header("Tipul nodului")]
    public NodeType nodeType;

    [Header("Conexiuni")]
    public List<MapNode> parents = new List<MapNode>();
    public List<MapNode> children = new List<MapNode>();

    // Nodul pe care jucătorul se află ACUM (în harta curentă)
    public static MapNode currentNode;

    // Ultimul nod pe care s-a dat click (pentru a focaliza camera la întoarcere)
    public static MapNode lastNode;

    private void OnMouseDown()
    {
        // Dacă nu există un nod curent, înseamnă că e primul nod accesat
        if (currentNode == null)
        {
            currentNode = this;
            lastNode = this;
            CinemachineFocusManager.Instance.FocusOn(this.transform);

            LoadSceneForThisNode();
            return;
        }

        // Dacă există deja un nod curent, putem alege fie pe el (același),
        // fie un copil al lui (mergem "în jos").
        bool isCurrentNode = (this == currentNode);
        bool isChildOfCurrent = currentNode.children.Contains(this);

        if (isCurrentNode)
        {
            lastNode = this;
            CinemachineFocusManager.Instance.FocusOn(this.transform);
            LoadSceneForThisNode();
        }
        else if (isChildOfCurrent)
        {
            currentNode = this;
            lastNode = this;
            CinemachineFocusManager.Instance.FocusOn(this.transform);
            LoadSceneForThisNode();
        }
        else
        {
            // Nod inaccesibil (nu e current, nu e child)
            Debug.Log("Nu poți să mergi la acest nod – trebuie să urmezi căile 'în jos' (copii).");
        }
    }

    private void LoadSceneForThisNode()
    {
        switch (nodeType)
        {
            case NodeType.Resources:
                SceneManager.LoadScene("SceneResources"); 
                break;
            case NodeType.Hostile:
                SceneManager.LoadScene("SceneHostile");
                break;
            case NodeType.Market:
                SceneManager.LoadScene("SceneMarket");
                break;
            default:
                Debug.LogWarning("Tipul nodului nu are scenă asociată!");
                break;
        }
    }
}
