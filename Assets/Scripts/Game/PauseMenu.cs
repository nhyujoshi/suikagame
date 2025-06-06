using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
    /* -------------------------------- Variables ------------------------------- */
    public static bool GAME_PAUSED = false;
    public KeyCode keyPause = KeyCode.Escape;
    public GameObject pauseMenuUI;

    [SerializeField] Sprite toggleOn;
    [SerializeField] Sprite toggleOff;

    [SerializeField] GameObject btnMusic;
    [SerializeField] GameObject btnSFX;


    // Private
    private GameManager gameManager;


    /* ------------------------------- Unity Func ------------------------------- */
    // Start is called before the first frame update
    void Start() {
        // Get game manager
        gameManager = FindObjectOfType<GameManager>();
    }
    
    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(keyPause)) {
            if (GAME_PAUSED) Resume();
            else Pause();
        }
    }


    /* -------------------------------- Functions ------------------------------- */
    public void Resume() {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Unfreeze time
        GAME_PAUSED = false;
    }

    public void Pause() {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze time
        GAME_PAUSED = true;
    }


    /* ------------------------------- Button Func ------------------------------ */
    // Button : Restart (In pause menu)
    public void Restart() {
        gameManager.Restart();
        Resume();
    }
    
    // Button : Exit (In pause menu)
    public void QuitGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        Resume();
    }

    private bool toggleSFX = true;
    private bool toggleMusic = true;

    public void OnToggleMusic()
    {
        if (toggleMusic)
        {
            btnMusic.GetComponent<Image>().sprite = toggleOff;
            toggleMusic = false;
        }
        else
        {
            btnMusic.GetComponent<Image>().sprite = toggleOn;
            toggleMusic = true;
        }
    }

    public void OnToggleSFX()
    {
        if (toggleSFX)
        {
            btnSFX.GetComponent<Image>().sprite = toggleOff;
            toggleSFX = false;
        }
        else
        {
            btnSFX.GetComponent<Image>().sprite = toggleOn;
            toggleSFX = true;
        }
    }
}
