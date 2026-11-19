
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// =============================================
// TIPE PEMBICARA
// =============================================

public enum PembicaraLevel1
{
    Raka,
    Gepi
}


// =============================================
// DATA PERCAKAPAN
// =============================================

[System.Serializable]
public class DialogLineLevel1
{
    [Header("Pembicara")]
    public PembicaraLevel1 pembicara;

    [TextArea(2, 5)]
    public string isiDialog;

    [Header("Ekspresi Raka")]
    public Sprite ekspresiRaka;

    [Header("Ekspresi Gepi")]
    public Sprite ekspresiGepi;
}


// =============================================
// DIALOGUE MANAGER LEVEL 1
// =============================================

public class DialogueManagerLevel1 : MonoBehaviour
{
    [Header("1. Referensi Cutscene")]
    [SerializeField]
    private Level1CutsceneController cutsceneController;


    [Header("2. Panel Utama")]
    [SerializeField]
    private GameObject dialoguePanel;


    [Header("3. Panel Raka")]
    [SerializeField]
    private GameObject rakaDialogPanel;

    [SerializeField]
    private TMP_Text rakaNameText;

    [SerializeField]
    private TMP_Text rakaDialogText;


    [Header("4. Panel Gepi")]
    [SerializeField]
    private GameObject gepiDialogPanel;

    [SerializeField]
    private TMP_Text gepiNameText;

    [SerializeField]
    private TMP_Text gepiDialogText;


    [Header("5. Pengaturan Typewriter")]
    [SerializeField]
    [Range(5f, 100f)]
    private float charactersPerSecond = 35f;

    [Tooltip("Klik saat mengetik untuk langsung menampilkan seluruh kalimat")]
    [SerializeField]
    private bool clickToRevealText = true;


    [Header("6. Tombol Lanjut")]
    [SerializeField]
    private Button nextButton;

    [SerializeField]
    private Image nextButtonImage;

    [SerializeField]
    private Sprite nextButtonSprite;

    [SerializeField]
    private TMP_Text nextButtonLabel;

    [SerializeField]
    private string normalButtonText = "Lanjut";

    [SerializeField]
    private string finalButtonText = "Mulai Materi";


    [Header("7. Referensi Karakter")]
    [SerializeField]
    private Image rakaImage;

    [SerializeField]
    private Image gepiImage;


    [Header("8. Ekspresi Awal")]
    [SerializeField]
    private Sprite ekspresiAwalRaka;

    [SerializeField]
    private Sprite ekspresiAwalGepi;


    [Header("9. Scene Materi")]
    [SerializeField]
    private string nextSceneName = "Level1_Materi";


    [Header("10. Daftar Percakapan")]
    [SerializeField]
    private List<DialogLineLevel1> daftarDialog =
        new List<DialogLineLevel1>();


    // =========================================
    // VARIABLE INTERNAL
    // =========================================

    private int currentIndex = -1;

    private bool dialogSudahDimulai = false;

    private bool isTyping = false;

    private TMP_Text activeDialogText;

    private Coroutine typingCoroutine;


    // =========================================
    // AWAKE
    // =========================================

    private void Awake()
    {
        if (cutsceneController == null)
        {
            cutsceneController =
                GetComponent<Level1CutsceneController>();
        }

        if (nextButton != null && nextButtonImage == null)
        {
            nextButtonImage =
                nextButton.GetComponent<Image>();
        }

        if (nextButton != null && nextButtonLabel == null)
        {
            nextButtonLabel =
                nextButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (nextButtonImage != null && nextButtonSprite != null)
        {
            nextButtonImage.sprite = nextButtonSprite;
        }

        RapikanTataLetakTeksDialog();
    }


    // =========================================
    // RAPIKAN KOTAK TEKS & NAMA SECARA OTOMATIS
    // =========================================

    private void RapikanTataLetakTeksDialog()
    {
        // 1. Rapikan properti teks dialog Raka (tanpa mengubah posisi/ukuran RectTransform dari Inspector)
        if (rakaDialogText != null)
        {
            rakaDialogText.margin = Vector4.zero;
            rakaDialogText.textWrappingMode = TextWrappingModes.Normal;
            rakaDialogText.overflowMode = TextOverflowModes.Overflow;
        }

        // 2. Rapikan properti teks dialog Gepi (tanpa mengubah posisi/ukuran RectTransform dari Inspector)
        if (gepiDialogText != null)
        {
            gepiDialogText.margin = Vector4.zero;
            gepiDialogText.textWrappingMode = TextWrappingModes.Normal;
            gepiDialogText.overflowMode = TextOverflowModes.Overflow;
        }

        // 3. Rapikan label nama Pemain (RakaNameText) agar tidak turun baris jika namanya panjang
        if (rakaNameText != null)
        {
            rakaNameText.margin = Vector4.zero;
            rakaNameText.textWrappingMode = TextWrappingModes.NoWrap;
            rakaNameText.enableAutoSizing = true;
            rakaNameText.fontSizeMin = 18f;
            rakaNameText.fontSizeMax = 36f;
        }

        // 4. Rapikan label nama Gepi (GepiNameText)
        if (gepiNameText != null)
        {
            gepiNameText.margin = Vector4.zero;
            gepiNameText.textWrappingMode = TextWrappingModes.NoWrap;
        }
    }


    // =========================================
    // ON ENABLE
    // =========================================

    private void OnEnable()
    {
        if (cutsceneController != null)
        {
            cutsceneController.onIntroCutsceneSelesai
                .RemoveListener(MulaiDialog);

            cutsceneController.onIntroCutsceneSelesai
                .AddListener(MulaiDialog);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextDialogue);

            nextButton.onClick.AddListener(NextDialogue);
        }
    }


    // =========================================
    // START
    // =========================================

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (rakaDialogPanel != null)
            rakaDialogPanel.SetActive(false);

        if (gepiDialogPanel != null)
            gepiDialogPanel.SetActive(false);

        if (rakaDialogText != null)
            rakaDialogText.text = "";

        if (gepiDialogText != null)
            gepiDialogText.text = "";

        PeriksaReferensi();
    }


    // =========================================
    // PEMERIKSAAN REFERENSI
    // =========================================

    private void PeriksaReferensi()
    {
        if (cutsceneController == null)
            Debug.LogError("Cutscene Controller belum dihubungkan!");

        if (dialoguePanel == null)
            Debug.LogError("Dialogue Panel belum dihubungkan!");

        if (rakaDialogPanel == null)
            Debug.LogError("Raka Dialog Panel belum dihubungkan!");

        if (gepiDialogPanel == null)
            Debug.LogError("Gepi Dialog Panel belum dihubungkan!");

        if (rakaNameText == null)
            Debug.LogError("Raka Name Text belum dihubungkan!");

        if (gepiNameText == null)
            Debug.LogError("Gepi Name Text belum dihubungkan!");

        if (rakaDialogText == null)
            Debug.LogError("Raka Dialog Text belum dihubungkan!");

        if (gepiDialogText == null)
            Debug.LogError("Gepi Dialog Text belum dihubungkan!");

        if (nextButton == null)
            Debug.LogError("Next Button belum dihubungkan!");

        if (rakaImage == null)
            Debug.LogError("Image Raka belum dihubungkan!");

        if (gepiImage == null)
            Debug.LogError("Image Gepi belum dihubungkan!");
    }


    // =========================================
    // MEMULAI DIALOG
    // =========================================

    public void MulaiDialog()
    {
        if (dialogSudahDimulai)
            return;

        if (daftarDialog == null || daftarDialog.Count == 0)
        {
            Debug.LogError("Daftar percakapan masih kosong!");
            return;
        }

        dialogSudahDimulai = true;

        currentIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = true;
        }

        if (rakaImage != null && ekspresiAwalRaka != null)
            rakaImage.sprite = ekspresiAwalRaka;

        if (gepiImage != null && ekspresiAwalGepi != null)
            gepiImage.sprite = ekspresiAwalGepi;

        TampilkanDialog();

        Debug.Log("Percakapan Raka dan Gepi dimulai!");
    }


    // =========================================
    // MENAMPILKAN DIALOG
    // =========================================

    private void TampilkanDialog()
    {
        if (currentIndex < 0 ||
            currentIndex >= daftarDialog.Count)
            return;

        DialogLineLevel1 data =
            daftarDialog[currentIndex];

        bool rakaSpeaking =
            data.pembicara == PembicaraLevel1.Raka;

        bool gepiSpeaking =
            data.pembicara == PembicaraLevel1.Gepi;


        // -------------------------------------
        // PERGANTIAN PANEL
        // -------------------------------------

        if (rakaDialogPanel != null)
            rakaDialogPanel.SetActive(rakaSpeaking);

        if (gepiDialogPanel != null)
            gepiDialogPanel.SetActive(gepiSpeaking);


        // -------------------------------------
        // NAMA KARAKTER
        // -------------------------------------

        string namaPemain = PlayerPrefs.GetString("NamaPemain", "Raka").Trim();
        if (string.IsNullOrEmpty(namaPemain))
        {
            namaPemain = "Raka";
        }

        if (rakaNameText != null)
            rakaNameText.text = rakaSpeaking ? namaPemain.ToUpper() : "";

        if (gepiNameText != null)
            gepiNameText.text = gepiSpeaking ? "GEPI" : "";


        // -------------------------------------
        // EKSPRESI KARAKTER
        // -------------------------------------

        if (rakaImage != null && data.ekspresiRaka != null)
            rakaImage.sprite = data.ekspresiRaka;

        if (gepiImage != null && data.ekspresiGepi != null)
            gepiImage.sprite = data.ekspresiGepi;


        // -------------------------------------
        // PILIH DIALOG TEXT SESUAI PEMBICARA
        // -------------------------------------

        if (rakaSpeaking)
        {
            activeDialogText = rakaDialogText;
        }
        else
        {
            activeDialogText = gepiDialogText;
        }


        // -------------------------------------
        // KOSONGKAN TEKS PANEL LAIN
        // -------------------------------------

        if (rakaDialogText != null)
            rakaDialogText.text = "";

        if (gepiDialogText != null)
            gepiDialogText.text = "";


        // -------------------------------------
        // ATUR TOMBOL
        // -------------------------------------

        if (nextButtonLabel != null)
        {
            bool dialogTerakhir =
                currentIndex == daftarDialog.Count - 1;

            nextButtonLabel.text =
                dialogTerakhir
                ? finalButtonText
                : normalButtonText;
        }


        // -------------------------------------
        // JALANKAN TYPEWRITER (SESUAIKAN NAMA PEMAIN)
        // -------------------------------------

        string kalimatDialog = SesuaikanNamaPemain(data.isiDialog, namaPemain);
        MulaiEfekMengetik(kalimatDialog);


        Debug.Log(
            "Dialog " + (currentIndex + 1) +
            "/" + daftarDialog.Count +
            " | Pembicara: " + data.pembicara
        );
    }


    // =========================================
    // SESUAIKAN NAMA PEMAIN DI TEKS DIALOG
    // =========================================

    private string SesuaikanNamaPemain(string teksAsli, string namaPemain)
    {
        if (string.IsNullOrEmpty(teksAsli))
            return "";

        // Gunakan Regex \b (word boundary) agar hanya mengganti kata utuh "Raka"
        // dan tidak mengubah kata seperti "berserakan" atau "gerakan"
        string hasil = teksAsli
            .Replace("{nama}", namaPemain)
            .Replace("{Nama}", namaPemain)
            .Replace("{NAMA}", namaPemain.ToUpper());

        hasil = Regex.Replace(hasil, @"\bRaka\b", namaPemain);
        hasil = Regex.Replace(hasil, @"\bRAKA\b", namaPemain.ToUpper());

        // Rapikan enter/baris baru atau spasi ganda dari Inspector agar kalimat mengalir rapi
        hasil = Regex.Replace(hasil, @"\s+", " ").Trim();

        return hasil;
    }


    // =========================================
    // MEMULAI TYPEWRITER
    // =========================================

    private void MulaiEfekMengetik(string kalimat)
    {
        HentikanEfekMengetik();

        if (activeDialogText == null)
        {
            Debug.LogError("Dialog Text aktif belum ditemukan!");
            return;
        }

        activeDialogText.text = kalimat;

        activeDialogText.maxVisibleCharacters = 0;

        activeDialogText.ForceMeshUpdate();

        typingCoroutine =
            StartCoroutine(JalankanTypewriter());
    }


    // =========================================
    // PROSES TYPEWRITER
    // =========================================

    private IEnumerator JalankanTypewriter()
    {
        isTyping = true;

        activeDialogText.ForceMeshUpdate();

        int totalKarakter =
            activeDialogText.textInfo.characterCount;

        float jeda =
            1f / Mathf.Max(1f, charactersPerSecond);

        for (int i = 1; i <= totalKarakter; i++)
        {
            activeDialogText.maxVisibleCharacters = i;

            if (i < totalKarakter)
            {
                yield return new WaitForSecondsRealtime(jeda);
            }
        }

        // Pastikan seluruh kalimat terlihat
        activeDialogText.maxVisibleCharacters =
            totalKarakter;

        isTyping = false;

        typingCoroutine = null;

        Debug.Log("Pengetikan dialog selesai!");
    }


    // =========================================
    // HENTIKAN TYPEWRITER
    // =========================================

    private void HentikanEfekMengetik()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }


    // =========================================
    // TAMPILKAN SELURUH KALIMAT
    // =========================================

    private void TampilkanSemuaTeks()
    {
        if (activeDialogText == null)
            return;

        HentikanEfekMengetik();

        activeDialogText.ForceMeshUpdate();

        activeDialogText.maxVisibleCharacters =
            activeDialogText.textInfo.characterCount;

        Debug.Log("Seluruh dialog ditampilkan.");
    }


    // =========================================
    // TOMBOL LANJUT
    // =========================================

    public void NextDialogue()
    {
        if (!dialogSudahDimulai)
            return;

        if (currentIndex < 0 ||
            currentIndex >= daftarDialog.Count)
            return;


        // Jika teks masih berjalan:
        // Klik pertama langsung menampilkan semuanya.
        if (isTyping && clickToRevealText)
        {
            TampilkanSemuaTeks();
            return;
        }


        if (string.IsNullOrWhiteSpace(
            daftarDialog[currentIndex].isiDialog))
        {
            Debug.LogWarning("Isi dialog belum diisi!");
            return;
        }


        // Pindah ke dialog berikutnya
        if (currentIndex + 1 < daftarDialog.Count)
        {
            currentIndex++;

            TampilkanDialog();
        }
        else
        {
            MasukKeMateri();
        }
    }


    // =========================================
    // PINDAH KE MATERI
    // =========================================

    private void MasukKeMateri()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError("Nama scene materi belum diisi!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError(
                "Scene " + nextSceneName +
                " belum terdaftar di Build Profiles!"
            );

            return;
        }

        dialogSudahDimulai = false;

        HentikanEfekMengetik();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Debug.Log("Masuk ke Materi Level 1.");

        SceneManager.LoadScene(nextSceneName);
    }


    // =========================================
    // ON DISABLE
    // =========================================

    private void OnDisable()
    {
        HentikanEfekMengetik();

        if (cutsceneController != null)
        {
            cutsceneController.onIntroCutsceneSelesai
                .RemoveListener(MulaiDialog);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextDialogue);
        }
    }
}
