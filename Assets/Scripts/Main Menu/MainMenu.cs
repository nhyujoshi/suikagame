using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    /* -------------------------------- Variables ------------------------------- */
    public GameObject panelMenu;
    public GameObject pauseMenuUI;

    /* ------------------------------- Button Func ------------------------------ */
    // Button : Play
    public void Play() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Button : Exit
    public void QuitGame() {
        Application.Quit(); // Close game
    }


    public void Settings()
    {
        pauseMenuUI.SetActive(true);
        /*
        Time.timeScale = 0f; // Freeze time
        GAME_PAUSED = true;*/
    }

}
