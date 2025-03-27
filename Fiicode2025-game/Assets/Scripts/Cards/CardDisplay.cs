using UnityEngine;
using UnityEngine.UI;
public class CardDisplay : MonoBehaviour
{
    public Card card;
    [SerializeField] private Image cardImage;
    [SerializeField] private Text cardName;
    [SerializeField] private Text cardDescription;

    void Awake()
    {
        cardImage.sprite = card.cardImage;
        cardName.text = card.cardName;
        cardDescription.text = card.cardDescription;
    } 
}
