using UnityEngine;
using UnityEngine.SceneManagement;

public class PindahScene : MonoBehaviour
{
    public void KePilihLevel()
    {
        SceneManager.LoadScene("PilihLevel");
    }

    public void KembaliKeMenuUtama()
    {
        SceneManager.LoadScene("MenuUtama");
    }

    public void KeluarGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void KeLevel1Game()
    {
        SceneManager.LoadScene("Level1_Game");
    }
}