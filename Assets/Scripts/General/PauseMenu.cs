using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public bool isPaused = false;

    private void Update()
    {
        if (isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        gameObject.SetActive(true);
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false; 
        gameObject.SetActive(false);
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
