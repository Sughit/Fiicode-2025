using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerHostile : MonoBehaviour
{
    public int enemiesToKill = 5;  // Numarul de inamici pe care trebuie sa-i omori

    public Text enemiesText;       // Elementul Text din UI unde afișăm starea inamicilor

    void Update()
    {
        // Actualizăm mesajul afișat în funcție de inamicii rămași
        if (enemiesToKill > 0)
        {
            enemiesText.text = "Enemies left: " + enemiesToKill;
        }
        else
        {
            // Dacă nu mai sunt inamici de omorât
            enemiesText.text = "All the enemies are defeated! Press 'T' to leave the planet.";

            // Verificăm dacă jucătorul apasă tasta dorită
            if (Input.GetKeyDown(KeyCode.T))
            {
                LeaveLevel();
            }
        }
    }

    // Apelează această metodă atunci când un inamic este omorât (de exemplu, din scriptul inamicului)
    public void EnemyKilled()
    {
        enemiesToKill--;

        // Prevenim să scadă sub zero printr-o verificare rapidă
        if (enemiesToKill < 0)
            enemiesToKill = 0;
    }

    // Aici poți pune logica de trecere la următorul nivel, încărcarea unei scene noi etc.
    void LeaveLevel()
    {
        Debug.Log("Nivel părăsit. Poți încărca următoarea scenă aici.");
        SceneManager.LoadScene("Map"); 
    }
}
