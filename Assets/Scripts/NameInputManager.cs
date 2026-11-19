using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NameInputManager : MonoBehaviour
{
    public TMP_InputField inputFieldNama;
    public string namaSceneSelanjutnya = "PilihLevel"; // Sesuaikan nama scene tujuan

    public void SimpanDanMulai()
    {
        string namaPemain = inputFieldNama.text.Trim();

        // Jika pemain lupa mengetik nama, otomatis pakai nama default "Raka"
        if (string.IsNullOrEmpty(namaPemain))
        {
            namaPemain = "Raka";
        }

        // Simpan nama ke memori (PlayerPrefs)
        PlayerPrefs.SetString("NamaPemain", namaPemain);
        PlayerPrefs.Save();

        // Pindah ke scene selanjutnya
        SceneManager.LoadScene(namaSceneSelanjutnya);
    }
}