using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager instance;

    [Header("UI Elements")]
    [Tooltip("Text-ul în care se va afișa notificarea curentă.")]

    [SerializeField] private GameObject notificationMenu;
    [SerializeField] private Text notificationText;
    [SerializeField] private Image notificationBg;
    [SerializeField] private Animator notificationAnim;

    [Header("Settings")]
    [Tooltip("Durata în secunde cât timp rămâne vizibilă notificarea.")]
    [SerializeField] private float notificationDuration = 2f;

    // Coada de mesaje de notificare
    private Queue<string> notificationQueue = new Queue<string>();
    private Coroutine currentNotificationRoutine;

    private void Awake()
    {
        // Singleton simplu
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ascundem notificarea la pornire
        if (notificationMenu != null)
            notificationMenu.SetActive(false);
    }

    /// <summary>
    /// Adaugă un mesaj în coada de notificări. Dacă nu există o notificare afișată în acel moment, o afișează imediat.
    /// </summary>
    public void ShowNotification(string message)
    {
        // Dacă o notificare este deja afișată sau există mesaje în coadă, le adăugăm pe cele noi în coadă.
        if (currentNotificationRoutine != null || notificationQueue.Count > 0)
        {
            notificationQueue.Enqueue(message);
        }
        else
        {
            currentNotificationRoutine = StartCoroutine(NotificationRoutine(message));
        }
    }

    /// <summary>
    /// Coroutine ce afișează mesajul, așteaptă timpul specificat, apoi ascunde mesajul.
    /// Dacă în coadă există mesaje, le afișează pe rând.
    /// </summary>
    private IEnumerator NotificationRoutine(string message)
    {
        // Setăm mesajul și afișăm textul
        if (notificationText != null)
        {
            Color alpha = notificationBg.color;
            alpha.a = 1;
            notificationBg.color = alpha;

            alpha = notificationText.color;
            alpha.a = 1;
            notificationText.color = alpha;
            notificationText.text = message;
            notificationMenu.SetActive(true);
            yield return new WaitForSeconds(notificationDuration - 0.5f);
            notificationAnim.SetTrigger("trigger");
        }

        // Așteptăm durata notificării
        yield return new WaitForSeconds(0.5f);

        // Ascundem textul
        if (notificationMenu != null)
            notificationMenu.SetActive(false);

        currentNotificationRoutine = null;

        // Dacă există mesaje în coadă, afișăm următorul
        if (notificationQueue.Count > 0)
        {
            string nextMessage = notificationQueue.Dequeue();
            currentNotificationRoutine = StartCoroutine(NotificationRoutine(nextMessage));
        }
    }
}
