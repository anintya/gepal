using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Level1CutsceneController : MonoBehaviour
{
    [Header("1. Referensi Canvas Karakter & Dialog")]
    [Tooltip("Canvas Karakter (berisi Player & AI_Companion dialog) yang disembunyikan saat cutscene berjalan dan muncul saat cutscene berhenti")]
    public GameObject canvasKarakter;

    [Tooltip("Panel atau GameObject sistem dialog yang akan diaktifkan setelah cutscene berhenti")]
    public GameObject panelSistemDialog;

    [Header("2. Referensi Player Hadap Belakang (Saat Cutscene Berjalan)")]
    [Tooltip("GameObject Player hadap belakang di tengah bawah layar saat kamera zoom in/out")]
    public GameObject playerObject;

    [Tooltip("Sprite Player menghadap ke belakang (ke arah gedung sekolah)")]
    public Sprite spriteHadapBelakang;

    [Tooltip("Ukuran Player UI (Width = 600, Height = 800)")]
    public Vector2 ukuranPlayerUI = new Vector2(600f, 800f);

    [Tooltip("Posisi Anchored Player UI di tengah bawah layar (terlihat setengah badan: kepala hingga pinggang/tas)")]
    public Vector2 posisiPlayerUI = new Vector2(0f, -140f);

    [Tooltip("Offset tinggi dari batas bawah layar agar terlihat setengah badan (kepala, bahu, dan tas punggung)")]
    public float offsetDariBawahLayar = 165f;

    [Tooltip("Posisi Player jika menggunakan SpriteRenderer World Space")]
    public Vector3 posisiPlayerCutscene = new Vector3(0f, -3.2f, 0f);

    [Tooltip("Skala Player jika menggunakan SpriteRenderer World Space (setara 600x800)")]
    public Vector3 skalaPlayerCutscene = new Vector3(0.606f, 0.606f, 1f);

    [Header("3. Konfigurasi Animasi Kamera (Tahap A - D)")]
    [Tooltip("Kamera utama cutscene (otomatis menggunakan Camera.main jika kosong)")]
    public Camera cutsceneCamera;

    [Tooltip("Tahap A & C: Posisi kamera saat tampilan awal / wide shot")]
    public Vector3 posisiKameraWide = new Vector3(0f, 0f, -10f);

    [Tooltip("Tahap A & C: Ukuran Orthographic kamera saat wide shot")]
    public float ukuranKameraWide = 5f;

    [Tooltip("Tahap B: Posisi fokus kamera saat zoom in ke area Player dan gedung sekolah")]
    public Vector3 posisiKameraZoomIn = new Vector3(0f, -1.25f, -10f);

    [Tooltip("Tahap B: Ukuran Orthographic kamera saat zoom in")]
    public float ukuranKameraZoomIn = 3.5f;

    [Header("4. Durasi Gerakan Kamera (Detik)")]
    [Tooltip("Durasi jeda awal pada Tahap A (wide shot) sebelum mulai zoom in")]
    public float durasiTampilanAwal = 1.5f;

    [Tooltip("Durasi gerakan kamera saat Tahap B (Zoom In halus)")]
    public float durasiZoomIn = 5.0f;

    [Tooltip("Durasi jeda singkat saat puncak Zoom In sebelum kembali Zoom Out")]
    public float durasiTahanZoomIn = 1.5f;

    [Tooltip("Durasi gerakan kamera saat Tahap C (Zoom Out halus kembali ke wide shot)")]
    public float durasiZoomOut = 5.0f;

    [Header("5. Status & Event Selesai Cutscene")]
    [Tooltip("Status apakah animasi kamera pembuka sudah selesai dan siap masuk ke dialog")]
    public bool siapUntukDialog = false;

    [Tooltip("Event yang dipanggil satu kali saat Tahap D selesai")]
    public UnityEvent onIntroCutsceneSelesai;

    private bool animasiSudahDijalankan = false;

    void Awake()
    {
        if (cutsceneCamera == null)
        {
            cutsceneCamera = GetComponent<Camera>();
            if (cutsceneCamera == null)
            {
                cutsceneCamera = Camera.main;
            }
        }

        // Cari otomatis Canvas Karakter di Hierarchy jika belum dihubungkan di Inspector
        if (canvasKarakter == null)
        {
            canvasKarakter = GameObject.Find("Karakter");
        }

        // Hilangkan Canvas Karakter dan Panel Dialog saat cutscene dimulai
        if (canvasKarakter != null)
        {
            canvasKarakter.SetActive(false);
        }

        if (panelSistemDialog != null)
        {
            panelSistemDialog.SetActive(false);
        }

        AturPosisiDanSpritePlayer();
        AturKameraTahapAwal();
    }

    void Start()
    {
        if (canvasKarakter != null)
        {
            canvasKarakter.SetActive(false);
        }

        if (panelSistemDialog != null)
        {
            panelSistemDialog.SetActive(false);
        }

        if (playerObject != null)
        {
            playerObject.SetActive(true);
        }

        if (!animasiSudahDijalankan)
        {
            animasiSudahDijalankan = true;
            StartCoroutine(JalankanUrutanKameraPembuka());
        }
    }

    /// <summary>
    /// Menampilkan karakter Player hadap belakang di tengah bawah layar (W=600, H=800, setengah badan)
    /// selama cutscene kamera berjalan.
    /// </summary>
    public void AturPosisiDanSpritePlayer()
    {
        // Jika playerObject kosong atau menunjuk ke Player di dalam Canvas Karakter (yang disembunyikan),
        // cari Player_HadapBelakang atau buat otomatis pada Canvas utama agar tetap tampil saat cutscene berjalan
        if (playerObject == null || (canvasKarakter != null && playerObject.transform.IsChildOf(canvasKarakter.transform)))
        {
            GameObject existingBackPlayer = GameObject.Find("Player_HadapBelakang");
            if (existingBackPlayer != null)
            {
                playerObject = existingBackPlayer;
            }
            else if (spriteHadapBelakang != null)
            {
                Canvas targetCanvas = null;
                if (panelSistemDialog != null)
                {
                    targetCanvas = panelSistemDialog.GetComponentInParent<Canvas>();
                }

                if (targetCanvas == null)
                {
                    GameObject canvasObj = GameObject.Find("Canvas");
                    if (canvasObj != null)
                    {
                        targetCanvas = canvasObj.GetComponent<Canvas>();
                    }
                }

                if (targetCanvas != null)
                {
                    playerObject = new GameObject("Player_HadapBelakang", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    playerObject.layer = 5;
                    playerObject.transform.SetParent(targetCanvas.transform, false);
                    playerObject.transform.SetAsFirstSibling();
                }
            }
        }

        if (playerObject == null)
        {
            return;
        }

        playerObject.SetActive(true);

        PlayerController controller = playerObject.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        Animator animator = playerObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        RectTransform rectTransform = playerObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = ukuranPlayerUI; // W = 600, H = 800

            Canvas parentCanvas = playerObject.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                if (canvasRect != null && canvasRect.rect.height > 0f)
                {
                    float posYSetengahBadan = -(canvasRect.rect.height * 0.5f) + offsetDariBawahLayar;
                    rectTransform.anchoredPosition = new Vector2(0f, posYSetengahBadan);
                }
                else
                {
                    rectTransform.anchoredPosition = posisiPlayerUI;
                }
            }
            else
            {
                rectTransform.anchoredPosition = posisiPlayerUI;
            }

            Image uiImage = playerObject.GetComponent<Image>();
            if (uiImage != null)
            {
                if (spriteHadapBelakang != null)
                {
                    uiImage.sprite = spriteHadapBelakang;
                }
                uiImage.raycastTarget = false;
            }

            return;
        }

        if (skalaPlayerCutscene.x > 0f && skalaPlayerCutscene.y > 0f)
        {
            playerObject.transform.localScale = skalaPlayerCutscene;
        }

        SpriteRenderer sr = playerObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (spriteHadapBelakang != null)
            {
                sr.sprite = spriteHadapBelakang;
            }
            sr.flipX = false;
            sr.flipY = false;
        }

        playerObject.transform.position = posisiPlayerCutscene;
    }

    private void AturKameraTahapAwal()
    {
        if (cutsceneCamera == null)
        {
            return;
        }

        cutsceneCamera.orthographic = true;
        cutsceneCamera.transform.position = posisiKameraWide;
        cutsceneCamera.orthographicSize = ukuranKameraWide;
    }

    private IEnumerator JalankanUrutanKameraPembuka()
    {
        if (cutsceneCamera == null)
        {
            yield break;
        }

        // ==========================================
        // TAHAP A – Tampilan Awal (Wide Shot)
        // ==========================================
        siapUntukDialog = false;
        cutsceneCamera.transform.position = posisiKameraWide;
        cutsceneCamera.orthographicSize = ukuranKameraWide;

        if (durasiTampilanAwal > 0f)
        {
            yield return new WaitForSeconds(durasiTampilanAwal);
        }

        // ==========================================
        // TAHAP B – Zoom In (SmoothStep)
        // ==========================================
        float waktuBerjalan = 0f;
        float durasiIn = Mathf.Max(0.01f, durasiZoomIn);

        while (waktuBerjalan < durasiIn)
        {
            waktuBerjalan += Time.deltaTime;
            float t = Mathf.Clamp01(waktuBerjalan / durasiIn);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            cutsceneCamera.transform.position = Vector3.Lerp(posisiKameraWide, posisiKameraZoomIn, smoothT);
            cutsceneCamera.orthographicSize = Mathf.Lerp(ukuranKameraWide, ukuranKameraZoomIn, smoothT);

            yield return null;
        }

        cutsceneCamera.transform.position = posisiKameraZoomIn;
        cutsceneCamera.orthographicSize = ukuranKameraZoomIn;

        if (durasiTahanZoomIn > 0f)
        {
            yield return new WaitForSeconds(durasiTahanZoomIn);
        }

        // ==========================================
        // TAHAP C – Zoom Out (SmoothStep)
        // ==========================================
        waktuBerjalan = 0f;
        float durasiOut = Mathf.Max(0.01f, durasiZoomOut);

        while (waktuBerjalan < durasiOut)
        {
            waktuBerjalan += Time.deltaTime;
            float t = Mathf.Clamp01(waktuBerjalan / durasiOut);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            cutsceneCamera.transform.position = Vector3.Lerp(posisiKameraZoomIn, posisiKameraWide, smoothT);
            cutsceneCamera.orthographicSize = Mathf.Lerp(ukuranKameraZoomIn, ukuranKameraWide, smoothT);

            yield return null;
        }

        // ==========================================
        // TAHAP D – Cutscene Berhenti (Munculkan Canvas Karakter & Dialog)
        // ==========================================
        cutsceneCamera.transform.position = posisiKameraWide;
        cutsceneCamera.orthographicSize = ukuranKameraWide;
        siapUntukDialog = true;

        // Sembunyikan Player hadap belakang pembuka saat masuk ke mode dialog
        if (playerObject != null)
        {
            playerObject.SetActive(false);
        }

        // Tampilkan kembali Canvas Karakter (Player kiri & AI_Companion kanan) saat cutscene berhenti
        if (canvasKarakter != null)
        {
            canvasKarakter.SetActive(true);
        }

        if (panelSistemDialog != null)
        {
            panelSistemDialog.SetActive(true);
        }

        if (onIntroCutsceneSelesai != null)
        {
            onIntroCutsceneSelesai.Invoke();
        }
    }
}
