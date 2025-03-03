using UnityEngine;

public class ClickPePlaneta : MonoBehaviour
{
    public void Click()
    {
        // Verifică dacă s-a făcut un click de mouse
        if (Input.GetMouseButtonDown(0))  // 0 reprezintă click-ul stâng al mouse-ului
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  // Creează un ray din poziția mouse-ului

            if (Physics.Raycast(ray, out hit))  // Verifică dacă ray-ul lovește un obiect
            {
                // Dacă obiectul lovit are un collider, acesta este apăsat
                if (hit.collider != null)
                {
                    // Aici poți apela funcția sau logica dorită pentru obiectul apăsat
                    Debug.Log("Ai apăsat pe: " + hit.collider.gameObject.name);

                }
            }
        }
    }
}