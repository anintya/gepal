using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

public class TampilNamaPemain : MonoBehaviour
{
    private TMP_Text teksUI;

    void Start()
    {
        teksUI = GetComponent<TMP_Text>();
        if (teksUI == null) return;

        // Ambil nama yang tadi disimpan (kalau belum ada, default "Raka")
        string namaTersimpan = PlayerPrefs.GetString("NamaPemain", "Raka").Trim();
        if (string.IsNullOrEmpty(namaTersimpan))
        {
            namaTersimpan = "Raka";
        }

        string isiAwal = teksUI.text.Trim();
        if (string.IsNullOrEmpty(isiAwal) || isiAwal.Equals("RAKA", System.StringComparison.OrdinalIgnoreCase))
        {
            teksUI.text = namaTersimpan.ToUpper();
        }
        else
        {
            // Hanya ganti kata utuh "Raka" menggunakan \b agar tidak mengubah kata seperti "berserakan"
            string hasil = teksUI.text.Replace("{nama}", namaTersimpan);
            hasil = Regex.Replace(hasil, @"\bRaka\b", namaTersimpan);
            hasil = Regex.Replace(hasil, @"\bRAKA\b", namaTersimpan.ToUpper());
            teksUI.text = hasil;
        }
    }
}