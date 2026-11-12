using System.Collections;
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

    [Header("--- WINNER PANEL & NAVIGASI ---")]
    public GameObject winnerPanel;
    public TextMeshProUGUI teksSkorWinner;
    public TextMeshProUGUI teksSisaWaktuWinner;
    public Image[] slotBintangWinner;
    public Sprite[] spriteBintangWinner;
    public Color warnaBintangKuning = new Color(1f, 0.84f, 0.0f, 1f);
    public Color warnaBintangKosong = new Color(0.75f, 0.75f, 0.75f, 1f);
    public Button tombolReplyWinner;
    public Button tombolHomeWinner;
    public Button tombolLanjutKuis;
    public string namaSceneKuis = "Level1_Tugas";
    public Image winnerBlurOverlayImage;
    public Sprite spriteWinnerPanel;
    public Sprite spriteTombolReplyWinner;
    public Sprite spriteTombolHomeWinner;
    public Sprite spriteTombolLanjutKuis;

    [Header("--- GAME OVER PANEL & NAVIGASI ---")]
    public GameObject gameOverPanel;
    public Button tombolRetry;
    public Button tombolHome;

    [Header("--- BACKGROUND & ASSET GAMEPLAY BLUR ---")]
    public SpriteRenderer bgGameplayRenderer;
    public Sprite spriteBgNormal;
    public Sprite spriteBgBlur;
    public Material blurSpriteMaterial;
    public RawImage blurOverlayRawImage;
    public Image blurOverlayImage;

    [Header("--- STATUS GAME ---")]
    public bool gameSelesai = false;

    private Material originalSpriteMaterial;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InisialisasiKomponenOtomatis();
    }

    void Start()
    {
        Time.timeScale = 1f;
        nyawaSaatIni = nyawaMaksimal;
        gameSelesai = false;

        UpdateUI();
        UpdateTampilanHati();

        // Pastikan GameOverPanel dan WinnerPanel tersembunyi di awal permainan
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (winnerPanel != null)
        {
            winnerPanel.SetActive(false);
        }

        // Kembalikan background gameplay ke sprite normal
        if (bgGameplayRenderer != null && spriteBgNormal != null)
        {
            bgGameplayRenderer.sprite = spriteBgNormal;
        }

        // Daftarkan aksi klik tombol
        PasangListenerTombol();
    }

    void Update()
    {
        // 1. Jika game sudah selesai (menang/kalah), hentikan seluruh proses Update
        if (gameSelesai) return;

        // 2. SELALU CEK SAMPAH: Begitu semua sampah habis dimasukkan ke tong, langsung panggil MenangGame()
        CekKondisiMenang();

        // 3. Timer berjalan mundur jika aktif, waktu > 0, dan nyawa masih ada
        if (timerAktif && sisaWaktu > 0 && nyawaSaatIni > 0 && !gameSelesai)
        {
            sisaWaktu -= Time.deltaTime;

            if (sisaWaktu <= 0)
            {
                sisaWaktu = 0;
                timerAktif = false;
                UpdateTimerUI();
                WaktuHabis();
                return;
            }

            UpdateTimerUI();
        }
    }

    GameObject CariObjekDiScene(string namaObjek)
    {
        GameObject obj = GameObject.Find(namaObjek);
        if (obj != null) return obj;

        Transform[] semuaTransform = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in semuaTransform)
        {
            if (t.hideFlags == HideFlags.None && t.name == namaObjek && t.gameObject.scene.isLoaded)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    void InisialisasiKomponenOtomatis()
    {
#if UNITY_EDITOR
        if (spriteWinnerPanel == null)
        {
            spriteWinnerPanel = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Panel/Level 1/Misi Pemilahan Selesai Game Panel.png");
        }
        if (spriteTombolReplyWinner == null)
        {
            spriteTombolReplyWinner = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Button/Retry.png");
        }
        if (spriteTombolHomeWinner == null)
        {
            spriteTombolHomeWinner = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Button/Home.png");
        }
        if (spriteTombolLanjutKuis == null)
        {
            spriteTombolLanjutKuis = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Panel/Level 1/Lanjut ke Kuis Button.png");
        }
        if (spriteBintangWinner == null || spriteBintangWinner.Length < 3)
        {
            spriteBintangWinner = new Sprite[3];
        }
        for (int i = 0; i < 3; i++)
        {
            if (spriteBintangWinner[i] == null)
            {
                spriteBintangWinner[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Panel/Level 1/Bintang_" + (i + 1) + ".png");
            }
        }
#endif

        // 1. Cari GameOverPanel jika belum di-assign di Inspector
        if (gameOverPanel == null)
        {
            gameOverPanel = CariObjekDiScene("GameOverPanel");
        }

        // 2. Cari WinnerPanel jika belum di-assign di Inspector (atau buat otomatis jika belum ada di Hierarchy)
        if (winnerPanel == null)
        {
            winnerPanel = CariObjekDiScene("WinnerPanel");
        }
        if (winnerPanel == null)
        {
            BuatWinnerPanelOtomatis();
        }

        // 3. Cari Background Gameplay SpriteRenderer jika belum di-assign
        if (bgGameplayRenderer == null)
        {
            GameObject bgObj = GameObject.Find("Bg game level 1");
            if (bgObj != null)
            {
                bgGameplayRenderer = bgObj.GetComponent<SpriteRenderer>();
            }
        }

        if (bgGameplayRenderer != null)
        {
            if (spriteBgNormal == null)
            {
                spriteBgNormal = bgGameplayRenderer.sprite;
            }
            if (originalSpriteMaterial == null)
            {
                originalSpriteMaterial = bgGameplayRenderer.sharedMaterial;
            }
        }

        // 4. Cari tombol & overlay di dalam GameOverPanel jika belum di-assign
        if (gameOverPanel != null)
        {
            if (tombolRetry == null)
            {
                Transform tRetry = gameOverPanel.transform.Find("ButtonRetry");
                if (tRetry != null)
                {
                    tombolRetry = tRetry.GetComponent<Button>();
                }
            }

            if (tombolHome == null)
            {
                Transform tHome = gameOverPanel.transform.Find("ButtonHome");
                if (tHome != null)
                {
                    tombolHome = tHome.GetComponent<Button>();
                }
            }

            Transform tBlur = gameOverPanel.transform.Find("BlurOverlay");
            if (tBlur != null)
            {
                if (blurOverlayRawImage == null)
                    blurOverlayRawImage = tBlur.GetComponent<RawImage>();

                if (blurOverlayImage == null)
                    blurOverlayImage = tBlur.GetComponent<Image>();
            }
        }

        // 5. Cari komponen di dalam WinnerPanel jika belum di-assign
        if (winnerPanel != null)
        {
            Transform tBlurWin = winnerPanel.transform.Find("BlurOverlay");
            if (tBlurWin != null && winnerBlurOverlayImage == null)
            {
                winnerBlurOverlayImage = tBlurWin.GetComponent<Image>();
            }

            Button[] semuaButton = winnerPanel.GetComponentsInChildren<Button>(true);
            foreach (Button b in semuaButton)
            {
                string bn = b.gameObject.name.ToLower();
                if (tombolReplyWinner == null && (bn.Contains("reply") || bn.Contains("retry") || bn.Contains("ulang")))
                {
                    tombolReplyWinner = b;
                }
                else if (tombolHomeWinner == null && (bn.Contains("home") || bn.Contains("menu")))
                {
                    tombolHomeWinner = b;
                }
                else if (tombolLanjutKuis == null && (bn.Contains("lanjut") || bn.Contains("kuis") || bn.Contains("quiz")))
                {
                    tombolLanjutKuis = b;
                }
            }

            if (teksSkorWinner == null || teksSisaWaktuWinner == null)
            {
                TextMeshProUGUI[] semuaTeks = winnerPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI tmp in semuaTeks)
                {
                    string n = tmp.gameObject.name.ToLower();
                    if (teksSkorWinner == null && (n.Contains("skor") || n.Contains("score")))
                    {
                        teksSkorWinner = tmp;
                    }
                    else if (teksSisaWaktuWinner == null && (n.Contains("waktu") || n.Contains("timer") || n.Contains("time") || n.Contains("sisa")))
                    {
                        teksSisaWaktuWinner = tmp;
                    }
                }

                // Fallback berdasarkan urutan jika nama objek teks masih default
                if (semuaTeks.Length >= 2)
                {
                    if (teksSkorWinner == null) teksSkorWinner = semuaTeks[0];
                    if (teksSisaWaktuWinner == null) teksSisaWaktuWinner = semuaTeks[1];
                }
                else if (semuaTeks.Length == 1 && teksSkorWinner == null)
                {
                    teksSkorWinner = semuaTeks[0];
                }
            }

            PastikanBintangDanTataLetakWinner();
        }
    }

    void PastikanBintangDanTataLetakWinner()
    {
        if (winnerPanel == null) return;

        Transform dialogTrans = winnerPanel.transform.Find("WinnerDialog");
        if (dialogTrans == null) return;

        RectTransform rtDialog = dialogTrans.GetComponent<RectTransform>();
        if (rtDialog == null) return;

        if (slotBintangWinner == null || slotBintangWinner.Length < 3)
        {
            slotBintangWinner = new Image[3];
        }

        Vector2[] posBintang = new Vector2[]
        {
            new Vector2(-183.5f, -51.5f),
            new Vector2(-1f, -49f),
            new Vector2(180.5f, -52f)
        };
        Vector2[] sizeBintang = new Vector2[]
        {
            new Vector2(162f, 152f),
            new Vector2(165f, 155f),
            new Vector2(162f, 153f)
        };

        for (int i = 0; i < 3; i++)
        {
            string namaBintang = "Bintang_" + (i + 1);
            if (slotBintangWinner[i] == null)
            {
                Transform tStar = rtDialog.Find(namaBintang);
                if (tStar == null) tStar = winnerPanel.transform.Find(namaBintang);
                if (tStar != null)
                {
                    slotBintangWinner[i] = tStar.GetComponent<Image>();
                }
            }

            if (slotBintangWinner[i] == null)
            {
                GameObject starObj = new GameObject(namaBintang, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                starObj.layer = 5;
                RectTransform rtStar = starObj.GetComponent<RectTransform>();
                rtStar.SetParent(rtDialog, false);
                rtStar.anchorMin = new Vector2(0.5f, 0.5f);
                rtStar.anchorMax = new Vector2(0.5f, 0.5f);
                rtStar.anchoredPosition = posBintang[i];
                rtStar.sizeDelta = sizeBintang[i];

                Image imgStar = starObj.GetComponent<Image>();
                if (spriteBintangWinner != null && i < spriteBintangWinner.Length && spriteBintangWinner[i] != null)
                {
                    imgStar.sprite = spriteBintangWinner[i];
                }
                imgStar.preserveAspect = true;
                imgStar.raycastTarget = false;
                imgStar.color = warnaBintangKosong;
                slotBintangWinner[i] = imgStar;
            }
        }
    }

    void BuatWinnerPanelOtomatis()
    {
        Transform parentCanvas = null;
        if (gameOverPanel != null && gameOverPanel.transform.parent != null)
        {
            parentCanvas = gameOverPanel.transform.parent;
        }
        else
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (parentCanvas == null || c.sortingOrder > parentCanvas.GetComponent<Canvas>().sortingOrder)
                {
                    parentCanvas = c.transform;
                }
            }
        }

        if (parentCanvas == null) return;

        // 1. Root WinnerPanel
        winnerPanel = new GameObject("WinnerPanel", typeof(RectTransform), typeof(CanvasRenderer));
        winnerPanel.layer = 5;
        RectTransform rtPanel = winnerPanel.GetComponent<RectTransform>();
        rtPanel.SetParent(parentCanvas, false);
        rtPanel.anchorMin = Vector2.zero;
        rtPanel.anchorMax = Vector2.one;
        rtPanel.anchoredPosition = Vector2.zero;
        rtPanel.sizeDelta = Vector2.zero;

        // 2. BlurOverlay
        GameObject blurObj = new GameObject("BlurOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        blurObj.layer = 5;
        RectTransform rtBlur = blurObj.GetComponent<RectTransform>();
        rtBlur.SetParent(rtPanel, false);
        rtBlur.anchorMin = Vector2.zero;
        rtBlur.anchorMax = Vector2.one;
        rtBlur.anchoredPosition = Vector2.zero;
        rtBlur.sizeDelta = Vector2.zero;
        winnerBlurOverlayImage = blurObj.GetComponent<Image>();
        winnerBlurOverlayImage.color = Color.white;
        winnerBlurOverlayImage.raycastTarget = true;

        // 3. Gambar Dialog Winner (Misi Pemilahan Selesai Game Panel - ukuran asli sprite 1376x1002 dengan skala 0.68)
        GameObject dialogObj = new GameObject("WinnerDialog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dialogObj.layer = 5;
        RectTransform rtDialog = dialogObj.GetComponent<RectTransform>();
        rtDialog.SetParent(rtPanel, false);
        rtDialog.anchorMin = new Vector2(0.5f, 0.5f);
        rtDialog.anchorMax = new Vector2(0.5f, 0.5f);
        rtDialog.anchoredPosition = new Vector2(0f, 55f);
        rtDialog.sizeDelta = new Vector2(1376f, 1002f);
        rtDialog.localScale = new Vector3(0.68f, 0.68f, 1f);
        Image imgDialog = dialogObj.GetComponent<Image>();
        imgDialog.sprite = spriteWinnerPanel;
        imgDialog.preserveAspect = true;
        imgDialog.color = Color.white;

        // 4. Bintang_1, Bintang_2, Bintang_3 (Di atas 3 bintang pada WinnerDialog)
        PastikanBintangDanTataLetakWinner();

        TMP_FontAsset defaultFont = teksSkor != null ? teksSkor.font : (teksTimer != null ? teksTimer.font : null);

        // 5. Teks Skor Winner (Kotak Kiri - tepat di sebelah kanan ikon Piala)
        GameObject skorObj = new GameObject("TeksSkorWinner", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        skorObj.layer = 5;
        RectTransform rtSkor = skorObj.GetComponent<RectTransform>();
        rtSkor.SetParent(rtDialog, false);
        rtSkor.anchorMin = new Vector2(0.5f, 0.5f);
        rtSkor.anchorMax = new Vector2(0.5f, 0.5f);
        rtSkor.anchoredPosition = new Vector2(-197f, -194f);
        rtSkor.sizeDelta = new Vector2(300f, 80f);
        teksSkorWinner = skorObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) teksSkorWinner.font = defaultFont;
        teksSkorWinner.text = "0";
        teksSkorWinner.fontSize = 54f;
        teksSkorWinner.fontStyle = FontStyles.Bold;
        teksSkorWinner.color = new Color(0.25f, 0.1f, 0.05f, 1f);
        teksSkorWinner.alignment = TextAlignmentOptions.Center;

        // 6. Teks Sisa Waktu Winner (Kotak Kanan - tepat di sebelah kanan ikon Stopwatch)
        GameObject waktuObj = new GameObject("TeksSisaWaktuWinner", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        waktuObj.layer = 5;
        RectTransform rtWaktu = waktuObj.GetComponent<RectTransform>();
        rtWaktu.SetParent(rtDialog, false);
        rtWaktu.anchorMin = new Vector2(0.5f, 0.5f);
        rtWaktu.anchorMax = new Vector2(0.5f, 0.5f);
        rtWaktu.anchoredPosition = new Vector2(299f, -194f);
        rtWaktu.sizeDelta = new Vector2(300f, 80f);
        teksSisaWaktuWinner = waktuObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) teksSisaWaktuWinner.font = defaultFont;
        teksSisaWaktuWinner.text = "00:00";
        teksSisaWaktuWinner.fontSize = 54f;
        teksSisaWaktuWinner.fontStyle = FontStyles.Bold;
        teksSisaWaktuWinner.color = new Color(0.25f, 0.1f, 0.05f, 1f);
        teksSisaWaktuWinner.alignment = TextAlignmentOptions.Center;

        // 7. ButtonReply (Mengulang Game - di dalam bagian bawah WinnerDialog sebelah kiri)
        GameObject replyObj = new GameObject("ButtonReply", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        replyObj.layer = 5;
        RectTransform rtReply = replyObj.GetComponent<RectTransform>();
        rtReply.SetParent(rtDialog, false);
        rtReply.anchorMin = new Vector2(0.5f, 0.5f);
        rtReply.anchorMax = new Vector2(0.5f, 0.5f);
        rtReply.anchoredPosition = new Vector2(-115f, -337f);
        rtReply.sizeDelta = new Vector2(175f, 175f);
        Image imgReply = replyObj.GetComponent<Image>();
        imgReply.sprite = spriteTombolReplyWinner;
        imgReply.preserveAspect = true;
        imgReply.color = Color.white;
        tombolReplyWinner = replyObj.GetComponent<Button>();
        tombolReplyWinner.targetGraphic = imgReply;

        // 8. ButtonHome (Kembali ke MenuUtama - di dalam bagian bawah WinnerDialog sebelah kanan)
        GameObject homeObj = new GameObject("ButtonHome", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        homeObj.layer = 5;
        RectTransform rtHome = homeObj.GetComponent<RectTransform>();
        rtHome.SetParent(rtDialog, false);
        rtHome.anchorMin = new Vector2(0.5f, 0.5f);
        rtHome.anchorMax = new Vector2(0.5f, 0.5f);
        rtHome.anchoredPosition = new Vector2(115f, -337f);
        rtHome.sizeDelta = new Vector2(175f, 175f);
        Image imgHome = homeObj.GetComponent<Image>();
        imgHome.sprite = spriteTombolHomeWinner;
        imgHome.preserveAspect = true;
        imgHome.color = Color.white;
        tombolHomeWinner = homeObj.GetComponent<Button>();
        tombolHomeWinner.targetGraphic = imgHome;

        // 9. ButtonLanjutKuis (Ditaruh DI BAWAH asset WinnerDialog)
        GameObject btnObj = new GameObject("ButtonLanjutKuis", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.layer = 5;
        RectTransform rtBtn = btnObj.GetComponent<RectTransform>();
        rtBtn.SetParent(rtDialog, false);
        rtBtn.anchorMin = new Vector2(0.5f, 0.5f);
        rtBtn.anchorMax = new Vector2(0.5f, 0.5f);
        rtBtn.anchoredPosition = new Vector2(0f, -585f);
        rtBtn.sizeDelta = new Vector2(500f, 145f);
        Image imgBtn = btnObj.GetComponent<Image>();
        imgBtn.sprite = spriteTombolLanjutKuis;
        imgBtn.preserveAspect = true;
        imgBtn.color = Color.white;
        tombolLanjutKuis = btnObj.GetComponent<Button>();
        tombolLanjutKuis.targetGraphic = imgBtn;

        winnerPanel.SetActive(false);
    }

    void PasangListenerTombol()
    {
        if (tombolRetry != null)
        {
            tombolRetry.onClick.RemoveListener(CobaLagi);
            tombolRetry.onClick.AddListener(CobaLagi);
        }

        if (tombolHome != null)
        {
            tombolHome.onClick.RemoveListener(KembaliKeMenuUtama);
            tombolHome.onClick.AddListener(KembaliKeMenuUtama);
        }

        if (tombolReplyWinner != null)
        {
            tombolReplyWinner.onClick.RemoveListener(CobaLagi);
            tombolReplyWinner.onClick.AddListener(CobaLagi);
        }

        if (tombolHomeWinner != null)
        {
            tombolHomeWinner.onClick.RemoveListener(KembaliKeMenuUtama);
            tombolHomeWinner.onClick.AddListener(KembaliKeMenuUtama);
        }

        if (tombolLanjutKuis != null)
        {
            tombolLanjutKuis.onClick.RemoveListener(BukaSceneKuis);
            tombolLanjutKuis.onClick.AddListener(BukaSceneKuis);
        }
    }

    void CekKondisiMenang()
    {
        DragDropWaste[] sisaSampah = FindObjectsByType<DragDropWaste>(FindObjectsSortMode.None);

        if (sisaSampah.Length <= 0 && !gameSelesai && nyawaSaatIni > 0)
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

            // Jika nyawa habis (3 kali salah masukkan sampah) -> Munculkan GameOverPanel
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

    string FormatWaktu(float waktuDetik)
    {
        int totalDetik = Mathf.CeilToInt(waktuDetik);
        if (totalDetik < 0) totalDetik = 0;

        int menit = totalDetik / 60;
        int detik = totalDetik % 60;

        return string.Format("{0:00}:{1:00}", menit, detik);
    }

    void UpdateTimerUI()
    {
        if (teksTimer != null)
        {
            teksTimer.text = FormatWaktu(sisaWaktu);
        }
    }

    public int HitungJumlahBintang(int nilaiSkor)
    {
        // Skor 0-50     -> 1 Bintang
        // Skor 60-120   -> 2 Bintang
        // Skor 130-200+ -> 3 Bintang
        if (nilaiSkor > 120)
        {
            return 3;
        }
        else if (nilaiSkor > 50)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    public void UpdateBintangWinner()
    {
        PastikanBintangDanTataLetakWinner();

        if (slotBintangWinner == null) return;

        int jumlahBintang = HitungJumlahBintang(skor);

        for (int i = 0; i < slotBintangWinner.Length; i++)
        {
            if (slotBintangWinner[i] == null) continue;

            if (spriteBintangWinner != null && i < spriteBintangWinner.Length && spriteBintangWinner[i] != null && slotBintangWinner[i].sprite == null)
            {
                slotBintangWinner[i].sprite = spriteBintangWinner[i];
            }

            slotBintangWinner[i].gameObject.SetActive(true);
            slotBintangWinner[i].color = (i < jumlahBintang) ? warnaBintangKuning : warnaBintangKosong;
        }
    }

    public void UpdateWinnerUI()
    {
        // 1. Tampilkan angka skor yang didapat oleh player saat bermain
        if (teksSkorWinner != null)
        {
            teksSkorWinner.text = skor.ToString();
        }

        // 2. Tampilkan angka sisa waktu player bermain (format MM:SS, misal 00:25 atau 00:00)
        if (teksSisaWaktuWinner != null)
        {
            teksSisaWaktuWinner.text = FormatWaktu(sisaWaktu);
        }

        // 3. Warnai bintang secara dinamis sesuai skor (0-50 = 1 Bintang, 60-120 = 2 Bintang, 130-200 = 3 Bintang)
        UpdateBintangWinner();
    }

    public void UpdateTampilanHati()
    {
        if (slotHati == null) return;

        for (int i = 0; i < slotHati.Length; i++)
        {
            if (slotHati[i] == null) continue;

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

    public void MenangGame()
    {
        if (gameSelesai) return;

        gameSelesai = true;
        timerAktif = false;
        UpdateUI();
        UpdateTimerUI();

        if (winnerPanel == null)
        {
            BuatWinnerPanelOtomatis();
            PasangListenerTombol();
        }

        UpdateWinnerUI();

        Debug.Log("Misi Selesai! Skor: " + skor + " | Sisa Waktu: " + FormatWaktu(sisaWaktu) + ". Menampilkan WinnerPanel.");

        // Nonaktifkan interaksi pada sampah yang tersisa (jika dipicu saat waktu habis)
        NonaktifkanInteraksiSampah();

        // Tampilkan WinnerPanel dengan latar belakang blur warna asli
        StartCoroutine(TangkapDanTampilkanPanel(winnerPanel, winnerBlurOverlayImage, true));
    }

    void WaktuHabis()
    {
        if (gameSelesai) return;

        Debug.Log("Waktu Bermain Habis! Menampilkan WinnerPanel dengan skor akhir & sisa waktu 00:00.");
        MenangGame();
    }

    public void GameOver()
    {
        gameSelesai = true;
        timerAktif = false;
        UpdateTimerUI();
        UpdateTampilanHati();
        Debug.Log("Game Over! Ketiga hati habis (KALAH). GameOverPanel muncul di atas gameplay.");

        // Nonaktifkan interaksi collider pada semua sampah
        NonaktifkanInteraksiSampah();

        // Tangkap tampilan utuh scene (background + semua objek) lalu beri efek blur dengan warna asli
        StartCoroutine(TangkapDanTampilkanPanel(gameOverPanel, blurOverlayImage, false));
    }

    void NonaktifkanInteraksiSampah()
    {
        DragDropWaste[] allWaste = FindObjectsByType<DragDropWaste>(FindObjectsSortMode.None);
        foreach (DragDropWaste w in allWaste)
        {
            Collider2D col = w.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    void TerapkanBlurPadaAssetScene()
    {
        // 1. Ubah background gameplay ke versi sprite blur dengan warna asli
        if (bgGameplayRenderer != null && spriteBgBlur != null)
        {
            bgGameplayRenderer.sprite = spriteBgBlur;
        }

        // 2. Terapkan material blur ke seluruh asset SpriteRenderer di scene (sampah, tong, player)
        if (blurSpriteMaterial != null)
        {
            SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            foreach (SpriteRenderer sr in allRenderers)
            {
                sr.material = blurSpriteMaterial;
            }
        }
    }

    Image PastikanBlurOverlayPadaPanel(GameObject targetPanel, Image overlayRef)
    {
        if (overlayRef != null) return overlayRef;
        if (targetPanel == null) return null;

        Transform existing = targetPanel.transform.Find("BlurOverlay");
        if (existing != null)
        {
            Image img = existing.GetComponent<Image>();
            if (img != null) return img;
        }

        // Buat BlurOverlay secara otomatis sebagai child paling belakang di dalam panel jika belum ada
        GameObject blurObj = new GameObject("BlurOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        blurObj.layer = targetPanel.layer;
        RectTransform rt = blurObj.GetComponent<RectTransform>();
        rt.SetParent(targetPanel.transform, false);
        rt.SetAsFirstSibling();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        Image newImg = blurObj.GetComponent<Image>();
        newImg.color = Color.white;
        newImg.raycastTarget = true;
        return newImg;
    }

    IEnumerator TangkapDanTampilkanPanel(GameObject targetPanel, Image targetOverlayImage, bool isWinner)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winnerPanel != null) winnerPanel.SetActive(false);

        // Tunggu hingga akhir frame agar seluruh background dan objek di scene selesai dirender dengan warna aslinya
        yield return new WaitForEndOfFrame();

        int w = Screen.width > 0 ? Screen.width : 1920;
        int h = Screen.height > 0 ? Screen.height : 1080;

        Image activeOverlay = PastikanBlurOverlayPadaPanel(targetPanel, targetOverlayImage);
        if (isWinner && winnerBlurOverlayImage == null)
        {
            winnerBlurOverlayImage = activeOverlay;
        }

        Texture2D screenCapture = ScreenCapture.CaptureScreenshotAsTexture();
        if (screenCapture != null)
        {
            // Multi-pass bilinear pyramid blur (menjaga seluruh objek tetap terlihat jelas posisinya & 100% warna asli):
            int wHalf = Mathf.Max(128, w / 2);
            int hHalf = Mathf.Max(72, h / 2);
            int wQuarter = Mathf.Max(64, w / 4);
            int hQuarter = Mathf.Max(36, h / 4);
            int wEighth = Mathf.Max(32, w / 8);
            int hEighth = Mathf.Max(18, h / 8);

            RenderTexture rtHalf = RenderTexture.GetTemporary(wHalf, hHalf, 0, RenderTextureFormat.ARGB32);
            rtHalf.filterMode = FilterMode.Bilinear;
            RenderTexture rtQuarter = RenderTexture.GetTemporary(wQuarter, hQuarter, 0, RenderTextureFormat.ARGB32);
            rtQuarter.filterMode = FilterMode.Bilinear;
            RenderTexture rtEighth = RenderTexture.GetTemporary(wEighth, hEighth, 0, RenderTextureFormat.ARGB32);
            rtEighth.filterMode = FilterMode.Bilinear;

            // Downsample bertahap lalu ping-pong dan upscale kembali ke resolusi halus (rtHalf)
            Graphics.Blit(screenCapture, rtHalf);
            Graphics.Blit(rtHalf, rtQuarter);
            Graphics.Blit(rtQuarter, rtEighth);
            Graphics.Blit(rtEighth, rtQuarter);
            Graphics.Blit(rtQuarter, rtEighth);
            Graphics.Blit(rtEighth, rtQuarter);
            Graphics.Blit(rtQuarter, rtHalf);

            // Terapkan juga blur ke objek di scene
            TerapkanBlurPadaAssetScene();

            if (!isWinner && blurOverlayRawImage != null)
            {
                blurOverlayRawImage.texture = rtHalf;
                blurOverlayRawImage.color = Color.white;
                blurOverlayRawImage.gameObject.SetActive(true);
            }
            else if (activeOverlay != null)
            {
                // Baca dari rtHalf (halus) agar semua objek dan warna bg tetap utuh namun blur
                RenderTexture.active = rtHalf;
                Texture2D blurTex = new Texture2D(rtHalf.width, rtHalf.height, TextureFormat.RGB24, false);
                blurTex.filterMode = FilterMode.Bilinear;
                blurTex.wrapMode = TextureWrapMode.Clamp;
                blurTex.ReadPixels(new Rect(0, 0, rtHalf.width, rtHalf.height), 0, 0);
                blurTex.Apply();
                RenderTexture.active = null;

                Sprite blurSprite = Sprite.Create(blurTex, new Rect(0, 0, blurTex.width, blurTex.height), new Vector2(0.5f, 0.5f));
                activeOverlay.sprite = blurSprite;
                activeOverlay.color = Color.white; // Murni putih agar warna bg & objek tidak hilang/redup
                activeOverlay.gameObject.SetActive(true);

                RenderTexture.ReleaseTemporary(rtHalf);
            }
            else
            {
                RenderTexture.ReleaseTemporary(rtHalf);
            }

            RenderTexture.ReleaseTemporary(rtQuarter);
            RenderTexture.ReleaseTemporary(rtEighth);
            Destroy(screenCapture);
        }
        else
        {
            // Fallback jika tangkapan layar tidak tersedia: blur langsung di scene tanpa menutupi objek
            TerapkanBlurPadaAssetScene();
            if (activeOverlay != null)
            {
                activeOverlay.sprite = null;
                activeOverlay.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        if (isWinner)
        {
            UpdateWinnerUI();
        }

        // Munculkan panel (WinnerPanel atau GameOverPanel) di atas gameplay yang telah di-blur
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }

    // =====================================================
    // FUNGSI UNTUK TOMBOL NAVIGASI PANEL GAME OVER & WINNER
    // =====================================================
    public void CobaLagi()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UlangiGame()
    {
        CobaLagi();
    }

    public void KembaliKeMenuUtama()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuUtama");
    }

    public void Home()
    {
        KembaliKeMenuUtama();
    }

    public void BukaSceneKuis()
    {
        Time.timeScale = 1f;
        string targetScene = string.IsNullOrEmpty(namaSceneKuis) ? "Level1_Tugas" : namaSceneKuis;
        SceneManager.LoadScene(targetScene);
    }

    public void LanjutKeKuis(string sceneKuis)
    {
        Time.timeScale = 1f;
        string targetScene = string.IsNullOrEmpty(sceneKuis) ? namaSceneKuis : sceneKuis;
        SceneManager.LoadScene(targetScene);
    }
}
