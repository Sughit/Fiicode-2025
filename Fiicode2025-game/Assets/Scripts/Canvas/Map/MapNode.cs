using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class MapNode : MonoBehaviour
{
    public enum NodeType { Resources, Hostile, Market, Boss }

    [Header("Tipul nodului")]
    public NodeType nodeType;

    [Header("Conexiuni")]
    public List<MapNode> parents = new List<MapNode>();
    public List<MapNode> children = new List<MapNode>();
    [SerializeField]private  Sprite[] imagine;
    private CinemachineVirtualCamera vcam;
    // Nodul pe care jucătorul se află ACUM (în harta curentă)
    public static MapNode currentNode = null;

    // Ultimul nod pe care s-a dat click (pentru a focaliza camera la întoarcere)
    public static MapNode lastNode;
    private Vector3 transOriginal;
    bool isChildOfCurrent, primul = false;
    void Start()
    {
        transOriginal = new Vector3 (transform.localScale.x, transform.localScale.y, transform.localScale.z);
        vcam = GameObject.Find("vcam").GetComponent<CinemachineVirtualCamera>();
        vcam.LookAt = null;
       //currentNode = GameObject.Find("Node_0_0").GetComponent<MapNode>();
        switch(nodeType)
        {
            case NodeType.Resources:
            GetComponent<SpriteRenderer>().sprite = imagine[0];
            break;

            case NodeType.Hostile:
            GetComponent<SpriteRenderer>().sprite = imagine[1];
            break;
    
            case NodeType.Market:
            GetComponent<SpriteRenderer>().sprite = imagine[2];
            break;

            case NodeType.Boss:
            GetComponent<SpriteRenderer>().sprite = imagine[3];
            break;

            default:
            break;

        }
    }
    private void OnMouseEnter()
    {
        if(this.gameObject.name == "Node_0_0")
            transform.localScale = new Vector3(transform.localScale.x+0.3f, transform.localScale.y+0.3f, transform.localScale.z+0.3f);
        else if(currentNode.children.Contains(this))
            transform.localScale = new Vector3(transform.localScale.x+0.3f, transform.localScale.y+0.3f, transform.localScale.z+0.3f);
    }
    private void OnMouseExit()
    {
        transform.localScale = transOriginal;
    }

    private void OnMouseDown()
    {

        // Dacă nu există un nod curent, înseamnă că e primul nod accesat
        if (this.gameObject.name == "Node_0_0")
        {
            primul = true;
            currentNode = this;
            lastNode = this;
            vcam.Follow = this.transform;

            LoadSceneForThisNode();
            return;
        }

        // Dacă există deja un nod curent, putem alege fie pe el (același),
        // fie un copil al lui (mergem "în jos").
        bool isCurrentNode = (this == currentNode);
        isChildOfCurrent = currentNode.children.Contains(this);

        if (isCurrentNode)
        {
            lastNode = this;
            vcam.Follow = this.transform;
            LoadSceneForThisNode();
        }
        else if (isChildOfCurrent)
        {
            currentNode = this;
            lastNode = this;
            vcam.Follow = this.transform;
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
                //SceneManager.LoadScene("SceneResources"); 
                break;
            case NodeType.Hostile:
                //SceneManager.LoadScene("SceneHostile");
                break;
            case NodeType.Market:
               // SceneManager.LoadScene("SceneMarket");
                break;
            case NodeType.Boss:
                //SceneManager.LoadScene("SceneBoss");
                break;
            default:
                Debug.LogWarning("Tipul nodului nu are scenă asociată!");
                break;
        }
    }
}
