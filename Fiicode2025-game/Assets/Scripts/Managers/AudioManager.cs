using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip[] clips;
    }

    [System.Serializable]
    public class Music
    {
        public string name;
        public AudioClip clip;
    }

    [Header("Sunete (SFX)")]
    public Sound[] sounds;
    public AudioSource soundSource; // Sursa pentru efecte sonore

    [Header("Muzică")]
    public Music[] musics;
    public AudioSource musicSource1; // Prima sursă de muzică
    public AudioSource musicSource2; // A doua sursă, folosită pentru tranziție
    public float musicFadeDuration = 1.0f; // Durata tranziției (fade)

    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Inițializare surse muzică (dacă nu sunt asignate din inspector)
        if (musicSource1 == null || musicSource2 == null)
        {
            musicSource1 = gameObject.AddComponent<AudioSource>();
            musicSource2 = gameObject.AddComponent<AudioSource>();
        }
        activeMusicSource = musicSource1;
        inactiveMusicSource = musicSource2;
    }

    // Abonarea la eveniment se face când obiectul este activ
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Dezabonarea când obiectul este dezactivat pentru a preveni eventualele probleme
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Metoda care va fi chemată automat după ce o scenă este încărcată
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scena încărcată: " + scene.name);
        switch(scene.name)
        {
            case "Hot 1":
            PlayMusic("Hot 1");
            break;
            case "Hot 2":
            PlayMusic("Hot 2");
            break;
            case "Main":
            PlayMusic("Main");
            break;
            case "Perfect 2":
            PlayMusic("Perfect 2");
            break;
            case "Cold 1":
            PlayMusic("Cold 1");
            break;
            case "Cold 2":
            PlayMusic("Cold 2");
            break;
            case "Space":
            PlayMusic("Space");
            break;
            default:
            break;
        }
    }

    /// <summary>
    /// Redă un efect sonor după nume, cu un clip ales aleatoriu și un pitch random.
    /// </summary>
    /// <param name="soundName">Numele sunetului definit în Inspector.</param>
    public void PlaySound(string soundName)
    {
        // Caută sunetul după nume
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sunetul nu a fost găsit: " + soundName);
            return;
        }
        if (s.clips.Length == 0)
        {
            Debug.LogWarning("Nu sunt clipuri asignate pentru: " + soundName);
            return;
        }

        // Alege un clip aleatoriu din lista de clipuri
        AudioClip clip = s.clips[Random.Range(0, s.clips.Length)];

        // Setează un pitch aleatoriu (ex: între 0.8 și 1.2)
        soundSource.pitch = Random.Range(0.8f, 1.2f);
        soundSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Redă o piesă de muzică după nume cu o tranziție (crossfade) între melodii.
    /// </summary>
    /// <param name="musicName">Numele piesei de muzică definit în Inspector.</param>
    public void PlayMusic(string musicName)
    {
        // Caută piesa de muzică după nume
        Music m = System.Array.Find(musics, music => music.name == musicName);
        if (m == null)
        {
            Debug.LogWarning("Muzica nu a fost găsită: " + musicName);
            return;
        }

        // Dacă piesa este deja redată, nu face nimic
        if (activeMusicSource.isPlaying && activeMusicSource.clip == m.clip)
            return;

        // Opresc eventuale tranziții în curs și pornesc o nouă tranziție
        StopAllCoroutines();
        StartCoroutine(CrossFadeMusic(m.clip));
    }

    /// <summary>
    /// Coroutine pentru tranziția între melodii prin crossfade.
    /// </summary>
    /// <param name="newClip">Clipul de muzică nou care va fi redat.</param>
    /// <returns></returns>
    private IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        // Schimb sursele active și inactive
        AudioSource temp = activeMusicSource;
        activeMusicSource = inactiveMusicSource;
        inactiveMusicSource = temp;

        // Setează noul clip pe sursa activă și pornește redarea la volum 0
        activeMusicSource.clip = newClip;
        activeMusicSource.volume = 0;
        activeMusicSource.Play();

        // Presupunem că inactiveMusicSource era la volum maxim (sau la un volum prestabilit)
        float startVolume = inactiveMusicSource.volume;
        float time = 0f;

        // Folosim SmoothStep pentru a obține o tranziție mai lină
        while (time < musicFadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / musicFadeDuration);
            activeMusicSource.volume = Mathf.Lerp(0, 1, t);
            inactiveMusicSource.volume = Mathf.Lerp(startVolume, 0, t);
            yield return null;
        }

        activeMusicSource.volume = 1;
        inactiveMusicSource.Stop();
    }

}
