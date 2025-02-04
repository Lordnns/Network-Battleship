using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseEndPanel : MonoBehaviour
{
    private InGameUIController ui;
    private void Start()
    {
        ui = Object.FindObjectOfType<InGameUIController>();
    }
    public void ResumeGame()
    {
        ui.SetPausePanelState(false);

    }

    public void MainMenu()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        Application.Quit();
    }
}
