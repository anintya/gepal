using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("--- PENGATURAN SKOR ---")]
    public int skor = 0;
    public TextMeshProUGUI teksSkor;

    [Header("--- PENGATURAN TIMER ---")]
    public float sisaWaktu = 60f;
    public bool timerAktif = false;
    public TextMeshProUGUI teksTimer;

    [Header("--- PENGATURAN NYAWA ---")]
    public int nyawaMaksimal = 3;
    public int nyawaSaatIni = 3;
    public Image[] slotHati;
    public Sprite spriteHatiMerah;
    public Sprite spriteHatiAbu;

    [Header("--- STATUS GAME ---")]
    public bool gameSelesai = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        nyawaSaatIni = nyawaMaksimal;
        gameSelesai = false;
        UpdateUI();
    }

    void Update()
    {
        // 1. Jika game sudah selesai (menang/kalah), hentikan seluruh proses Update di sini
        if (gameSelesai) return;

        // 2. SELALU CEK SAMPAH: Begitu sampah habis di layar, langsung panggil MenangGame()
        CekKondisiMenang();

        // 3. Timer berjalan mundur jika aktif, waktu > 0, dan nyawa masih ada
        if (timerAktif && sisaWaktu > 0 && nyawaSaatIni > 0)
        {
            sisaWaktu -= Time.deltaTime;

            if (sisaWaktu <= 0)
            {
                sisaWaktu = 0;
                timerAktif = false;
                WaktuHabis();
            }

            UpdateTimerUI();
        }
    }

    void CekKondisiMenang()
    {
        // Mendeteksi sisa sampah berdasarkan komponen DragDropWaste yang aktif di layar
        DragDropWaste[] sisaSampah = FindObjectsByType<DragDropWaste>(FindObjectsSortMode.None);

        // Jika game sudah dimulai dan sisa sampah benar-benar 0
        if (timerAktif && sisaSampah.Length <= 0 && !gameSelesai)
        {
            MenangGame();
        }
    }

    public void MulaiGameManager()
    {
        if (!gameSelesai && nyawaSaatIni > 0)
        {
            timerAktif = true;
        }
    }

    public void TambahSkor(int jumlah)
    {
        if (gameSelesai) return;
        skor += jumlah;
        UpdateUI();
    }

    public void KurangiSkor(int jumlah)
    {
        if (gameSelesai) return;
        skor -= jumlah;
        if (skor < 0) skor = 0;
        UpdateUI();
    }

    public void KurangNyawa()
    {
        if (gameSelesai) return;

        if (nyawaSaatIni > 0)
        {
            nyawaSaatIni -= 1;
            UpdateUI();
            UpdateTampilanHati();

            if (nyawaSaatIni <= 0)
            {
                nyawaSaatIni = 0;
                timerAktif = false;
                GameOver();
            }
        }
    }

    void UpdateUI()
    {
        if (teksSkor != null)
        {
            teksSkor.text = skor.ToString();
        }
    }

    void UpdateTimerUI()
    {
        if (teksTimer != null)
        {
            // Ubah sisa waktu ke format menit dan detik
            int totalDetik = Mathf.CeilToInt(sisaWaktu);
            if (totalDetik < 0) totalDetik = 0;

            int menit = totalDetik / 60;
            int detik = totalDetik % 60;

            // Mengubah format angka menjadi 2 digit (contoh: 01:05 atau 00:00)
            teksTimer.text = string.Format("{0:00}:{1:00}", menit, detik);
        }
    }

    void UpdateTampilanHati()
    {
        for (int i = 0; i < slotHati.Length; i++)
        {
            if (i < nyawaSaatIni)
            {
                slotHati[i].sprite = spriteHatiMerah;
            }
            else
            {
                slotHati[i].sprite = spriteHatiAbu;
            }
        }
    }

    void MenangGame()
    {
        gameSelesai = true;
        timerAktif = false;
        UpdateTimerUI(); // Kunci tampilan angka timer di layar
        Debug.Log("SELAMAT! Semua sampah habis dibersihkan (MENANG). Timer otomatis berhenti.");
    }

    void WaktuHabis()
    {
        gameSelesai = true;
        timerAktif = false;
        UpdateTimerUI();
        Debug.Log("Waktu Habis! (KALAH). Timer dihentikan.");
    }

    void GameOver()
    {
        gameSelesai = true;
        timerAktif = false;
        UpdateTimerUI();
        Debug.Log("Game Over! Nyawa habis (KALAH). Timer dihentikan.");
    }

    // =====================================================
    // FUNGSI UNTUK TOMBOL NAVIGASI PANEL (Dipakai Nanti)
    // =====================================================
    public void UlangiGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LanjutKeKuis(string namaSceneKuis)
    {
        SceneManager.LoadScene(namaSceneKuis);
    }
}