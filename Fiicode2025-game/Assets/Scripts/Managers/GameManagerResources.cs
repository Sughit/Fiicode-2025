using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class GameManagerResources : MonoBehaviour
{
    [Header("Setări Timer")]
    public float initialTime = 30f;  // Timpul de start în secunde
    private float currentTime;

    [Header("Referință UI")]
    public Text timerText;          // Asignează în Inspector elementul Text UI

    void Start()
    {
        // Inițializăm timer-ul
        currentTime = initialTime;
    }

    void Update()
    {
        // Scadem timpul pe baza lui DeltaTime
        currentTime -= Time.deltaTime;

        // Forțăm să nu coboare sub zero
        if (currentTime < 0f)
        {
            currentTime = 0f;
            OnTimerEnd();
        }

        // Actualizăm afișarea pe ecran
        UpdateTimerText();
    }

    void OnTimerEnd()
    {
        // Aici pui logica dorită la expirarea timerului
        Debug.Log("Timer terminat! Apelează aici logica dorită, de exemplu încărcarea unui alt nivel.");
        SceneManager.LoadScene("Map"); 
    }

    void UpdateTimerText()
    {
        // Calculăm minutele și secundele rămase
        int minutes = (int)(currentTime / 60);
        int seconds = (int)(currentTime % 60);

        // Formatează în stilul "MM:SS"
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Atribuie textului din UI
        timerText.text = timeString;
    }
}
