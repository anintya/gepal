
using UnityEngine;
using UnityEngine.SceneManagement;

public class PindahScene : MonoBehaviour
{
    // =========================================
    // PINDAH KE SCENE INPUT NAMA
    // =========================================

    public void KeInputNama()
    {
        SceneManager.LoadScene("InputNama");
    }


    // =========================================
    // PINDAH KE SCENE PILIH LEVEL
    // =========================================

    public void KePilihLevel()
    {
        SceneManager.LoadScene("PilihLevel");
    }


    // =========================================
    // KEMBALI KE MENU UTAMA
    // =========================================

    public void KembaliKeMenuUtama()
    {
        SceneManager.LoadScene("MenuUtama");
    }


    // =========================================
    // KELUAR DARI GAME
    // =========================================

    public void KeluarGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    // =========================================
    // MASUK KE CUTSCENE LEVEL 1
    // =========================================

    public void KeLevel1Cutscene()
    {
        SceneManager.LoadScene("Level1_Cutscene");
    }


    // =========================================
    // MASUK KE GAMEPLAY LEVEL 1
    // =========================================

    public void KeLevel1Game()
    {
        SceneManager.LoadScene("Level1_Game");
    }
}