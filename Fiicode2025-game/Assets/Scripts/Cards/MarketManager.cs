using UnityEngine;

public class MarketManager : MonoBehaviour
{
    [SerializeField] private Card[] cards;
    [SerializeField] private GameObject cardGO;
    [SerializeField] private Transform cardParent;
    [SerializeField] private PriceMode[] priceModes;
    [SerializeField] private PriceType[] priceTypes;
 
    void Start()
    {
        for(int i=0; i<3; i++)
        {
            GameObject go = Instantiate(cardGO, cardParent);
            go.GetComponent<CardDisplay>().card = cards[Random.Range(0, cards.Length)];
            priceTypes[i] = go.GetComponent<CardDisplay>().card.priceType;
            priceModes[i] = (PriceMode)Random.Range(0, 4);
        }
        GetPrice();
    }

    void GetPrice()
    {
        for(int i=0; i<3; i++)
        {
            SetPrice(priceModes[i], priceTypes[i], i);
        }
    }

    void SetPrice(PriceMode priceMode, PriceType priceType, int index)
    {
        switch(priceType)
        {
            case PriceType.cheap:
                switch(priceMode)
                {
                    case PriceMode.discovery:
                        Debug.Log("Cheap card with discovery price mode");
                        break;
                    case PriceMode.kills:
                        Debug.Log("Cheap card with kills price mode");
                        break;
                    case PriceMode.time:
                        Debug.Log("Cheap card with time price mode");
                        break;
                    case PriceMode.resources:
                        Debug.Log("Cheap card with resources price mode");
                        break;
                }
                break;
            case PriceType.medium:
                switch(priceMode)
                {
                    case PriceMode.discovery:
                        Debug.Log("Medium card with discovery price mode");
                        break;
                    case PriceMode.kills:
                        Debug.Log("Medium card with kills price mode");
                        break;
                    case PriceMode.time:
                        Debug.Log("Medium card with time price mode");
                        break;
                    case PriceMode.resources:
                        Debug.Log("Medium card with resources price mode");
                        break;
                }
                break;
            case PriceType.expensive:
                switch(priceMode)
                {
                    case PriceMode.discovery:
                        Debug.Log("Expensive card with discovery price mode");
                        break;
                    case PriceMode.kills:
                        Debug.Log("Expensive card with kills price mode");
                        break;
                    case PriceMode.time:
                        Debug.Log("Expensive card with time price mode");
                        break;
                    case PriceMode.resources:
                        Debug.Log("Expensive card with resources price mode");
                        break;
                }
                break;
        }
    }
}
