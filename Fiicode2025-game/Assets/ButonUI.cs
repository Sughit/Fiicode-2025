using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButonUI: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Color original;
    void Start()
    {
        original = this.gameObject.transform.GetChild(0).GetComponent<Text>().color;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        this.gameObject.transform.GetChild(0).GetComponent<Text>().color = new Color(0.4395247f, 0.7830189f, 0.4646718f, 1);
        this.gameObject.transform.GetChild(0).localScale = new Vector3(1.2f, 1.2f, 1.2f);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        this.gameObject.transform.GetChild(0).GetComponent<Text>().color = original;
        this.gameObject.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
    }


}
