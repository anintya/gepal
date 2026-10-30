using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    public Button Btnlevel1;
    public Button Btnlevel2;
    public Button Btnlevel3;
    public Button Btnlevel4;

    public GameObject gembok2;
    public GameObject gembok3;
    public GameObject gembok4;

    void Start()
    {
        UpdateTombolLevel();
    }

    void UpdateTombolLevel()
    {
        int terbuka = PlayerPrefs.GetInt("LevelTerbuka", 1);

        AturTombol(Btnlevel1, true, null);
        AturTombol(Btnlevel2, terbuka >= 2, gembok2);
        AturTombol(Btnlevel3, terbuka >= 3, gembok3);
        AturTombol(Btnlevel4, terbuka >= 4, gembok4);
    }

    void AturTombol(Button tombol, bool terbuka, GameObject overlayKunci)
    {
        if (tombol == null) return;

        // INI PERBAIKANNYA:
        // Matikan tint bawaan Unity, supaya tombol terkunci
        // tidak jadi pudar / transparan / blur.
        tombol.transition = Selectable.Transition.None;

        // Paksa warna gambar kartu tetap putih penuh (alpha 1),
        // jangan diubah jadi abu-abu.
        Image gambar = tombol.GetComponent<Image>();
        if (gambar != null)
            gambar.color = Color.white;

        tombol.interactable = terbuka;

        // Yang menggelapkan hanya overlay hitam ini saja.
        if (overlayKunci != null)
            overlayKunci.SetActive(!terbuka);
    }

    public void OnLevel1() { SceneManager.LoadScene("Level1"); }

    public void OnLevel2()
    {
        if (PlayerPrefs.GetInt("LevelTerbuka", 1) >= 2)
            SceneManager.LoadScene("Level2");
    }

    public void OnLevel3()
    {
        if (PlayerPrefs.GetInt("LevelTerbuka", 1) >= 3)
            SceneManager.LoadScene("Level3");
    }

    public void OnLevel4()
    {
        if (PlayerPrefs.GetInt("LevelTerbuka", 1) >= 4)
            SceneManager.LoadScene("Level4");
    }

    public void KembaliKeMenuUtama()
    {
        SceneManager.LoadScene("MenuUtama");
    }
}