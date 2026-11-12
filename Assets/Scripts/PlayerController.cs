using UnityEngine;
using TMPro; // === TAMBAHAN UNTUK TEXTMESHPRO ===

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;

    private Vector2 moveInput = Vector2.zero;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private GameObject sampahTerdekat = null;
    private GameObject sampahYangDibawa = null;

    private string arahHadap = "Kanan";

    private bool sedangMemungut = false;
    private bool gameDimulai = false;

    // === TAMBAHAN VARIABEL SKOR ===
    public int skor = 0;
    public TextMeshProUGUI teksSkor;
    // ==============================

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        moveInput = Vector2.zero;
        sedangMemungut = false;
        gameDimulai = false;

        // === TAMBAHAN TAMPILAN AWAL SKOR ===
        UpdateTeksSkor();
        // ===================================

        // AWAL GAME WAJIB IDLE
        anim.Play("Idle", 0, 0f);
        anim.Update(0f);

        // Bekukan Animator supaya tidak pindah ke Walk sendiri
        anim.enabled = false;
    }


    void Update()
    {
        // Jika game selesai (Game Over / Menang), hentikan gerakan pemain
        if (GameManager.instance != null && GameManager.instance.gameSelesai)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // ==========================================
        // BELUM ADA TOMBOL DITEKAN = TETAP IDLE
        // ==========================================
        if (!gameDimulai)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }


        // ==========================================
        // SEDANG PICKUP = BERHENTI SEBENTAR
        // ==========================================
        if (sedangMemungut)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }


        // ==========================================
        // GERAK
        // ==========================================
        if (moveInput != Vector2.zero)
        {
            rb.linearVelocity = moveInput * moveSpeed;

            // KANAN
            if (moveInput.x > 0)
            {
                arahHadap = "Kanan";

                if (!anim.GetCurrentAnimatorStateInfo(0)
                         .IsName("PlayerWalkRight"))
                {
                    anim.Play("PlayerWalkRight");
                }
            }

            // KIRI
            else if (moveInput.x < 0)
            {
                arahHadap = "Kiri";

                if (!anim.GetCurrentAnimatorStateInfo(0)
                         .IsName("PlayerWalkLeft"))
                {
                    anim.Play("PlayerWalkLeft");
                }
            }

            // ATAS / BAWAH
            else if (moveInput.y != 0)
            {
                // Mengikuti arah hadap terakhir
                if (arahHadap == "Kanan")
                {
                    if (!anim.GetCurrentAnimatorStateInfo(0)
                             .IsName("PlayerWalkRight"))
                    {
                        anim.Play("PlayerWalkRight");
                    }
                }
                else
                {
                    if (!anim.GetCurrentAnimatorStateInfo(0)
                             .IsName("PlayerWalkLeft"))
                    {
                        anim.Play("PlayerWalkLeft");
                    }
                }
            }
        }
        else
        {
            // Tombol dilepas:
            // karakter berhenti bergerak
            // tapi animasi tetap Walk
            rb.linearVelocity = Vector2.zero;
        }
    }


    // =====================================================
    // TOMBOL KANAN
    // =====================================================

    public void TekanKanan()
    {
        if (GameManager.instance != null && GameManager.instance.gameSelesai) return;
        MulaiGame();

        // SELALU ubah arah ke kanan
        arahHadap = "Kanan";

        // SELALU gerak ke kanan
        moveInput = new Vector2(1, 0);

        // Kalau TIDAK sedang pickup,
        // langsung paksa animasi menghadap kanan
        if (!sedangMemungut)
        {
            anim.Play("PlayerWalkRight");
        }
    }


    // =====================================================
    // TOMBOL KIRI
    // =====================================================

    public void TekanKiri()
    {
        if (GameManager.instance != null && GameManager.instance.gameSelesai) return;
        MulaiGame();

        // SELALU ubah arah ke kiri
        arahHadap = "Kiri";

        // SELALU gerak ke kiri
        moveInput = new Vector2(-1, 0);

        // Kalau TIDAK sedang pickup,
        // langsung paksa animasi menghadap kiri
        if (!sedangMemungut)
        {
            anim.Play("PlayerWalkLeft");
        }
    }


    // =====================================================
    // TOMBOL ATAS
    // =====================================================

    public void TekanAtas()
    {
        if (GameManager.instance != null && GameManager.instance.gameSelesai) return;
        MulaiGame();

        moveInput = new Vector2(0, 1);

        if (!sedangMemungut)
        {
            if (arahHadap == "Kanan")
            {
                anim.Play("PlayerWalkRight");
            }
            else
            {
                anim.Play("PlayerWalkLeft");
            }
        }
    }


    // =====================================================
    // TOMBOL BAWAH
    // =====================================================

    public void TekanBawah()
    {
        if (GameManager.instance != null && GameManager.instance.gameSelesai) return;
        MulaiGame();

        moveInput = new Vector2(0, -1);

        if (!sedangMemungut)
        {
            if (arahHadap == "Kanan")
            {
                anim.Play("PlayerWalkRight");
            }
            else
            {
                anim.Play("PlayerWalkLeft");
            }
        }
    }


    // =====================================================
    // GAME DIMULAI SAAT TOMBOL PERTAMA DITEKAN
    // =====================================================

    void MulaiGame()
    {
        if (!gameDimulai)
        {
            gameDimulai = true;

            // Baru hidupkan Animator
            anim.enabled = true;
        }
    }


    // =====================================================
    // TOMBOL DILEPAS
    // =====================================================

    public void LepasTombol()
    {
        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        // TIDAK kembali Idle
        // Animasi tetap Walk sesuai arah terakhir
    }


    // =====================================================
    // DETEKSI SAMPAH DAN TONG
    // =====================================================

    void OnTriggerEnter2D(Collider2D collision)
    {
        // ==========================================
        // MENYENTUH SAMPAH
        // ==========================================
        if ((collision.CompareTag("Sampah Organik") ||
             collision.CompareTag("Sampah Anorganik")) &&
             sampahYangDibawa == null &&
             !sedangMemungut)
        {
            sampahTerjenisKelaminTerdekat(
                collision.gameObject
            );
        }


        // ==========================================
        // MENYENTUH TONG
        // ==========================================
        if (collision.CompareTag("Tong Organik") ||
            collision.CompareTag("Tong Anorganik"))
        {
            if (sampahYangDibawa != null)
            {
                string tagSampah = sampahYangDibawa.tag;
                string tagTong = collision.tag;


                if (
                    (tagSampah == "Sampah Organik" &&
                     tagTong == "Tong Organik")
                    ||
                    (tagSampah == "Sampah Anorganik" &&
                     tagTong == "Tong Anorganik")
                   )
                {
                    Debug.Log(
                        "Yeay! Sampah berhasil dimasukkan ke tong yang benar!"
                    );


                    sampahYangDibawa.transform.SetParent(null);

                    Destroy(sampahYangDibawa);

                    sampahYangDibawa = null;


                    spriteRenderer.sortingOrder = 1;


                    // === TAMBAHAN SKOR +10 ===
                    skor += 10;
                    UpdateTeksSkor();
                    // =========================


                    // Tetap Walk sesuai arah terakhir
                    if (arahHadap == "Kanan")
                    {
                        anim.Play(
                            "PlayerWalkRight",
                            0,
                            0f
                        );
                    }
                    else
                    {
                        anim.Play(
                            "PlayerWalkLeft",
                            0,
                            0f
                        );
                    }
                }
                else
                {
                    Debug.Log(
                        "Oops! Salah memasukkan jenis tong sampah!"
                    );

                    // Kurangi skor dan nyawa saat salah memasukkan tong
                    if (GameManager.instance != null)
                    {
                        GameManager.instance.KurangiSkor(10);
                        GameManager.instance.KurangNyawa();
                    }

                    if (teksSkor != null && GameManager.instance != null)
                    {
                        skor = GameManager.instance.skor;
                        UpdateTeksSkor();
                    }

                    // Lepas sampah yang dibawa dan kembalikan ke lantai
                    if (sampahYangDibawa != null)
                    {
                        sampahYangDibawa.transform.SetParent(null);
                        Collider2D colSampah = sampahYangDibawa.GetComponent<Collider2D>();
                        if (colSampah != null) colSampah.enabled = true;
                        SpriteRenderer sampahSR = sampahYangDibawa.GetComponent<SpriteRenderer>();
                        if (sampahSR != null) sampahSR.sortingOrder = 1;
                        sampahYangDibawa = null;
                        spriteRenderer.sortingOrder = 1;
                    }
                }
            }
        }
    }


    // === TAMBAHAN FUNGSI UPDATE UI TEKS SKOR ===
    void UpdateTeksSkor()
    {
        if (teksSkor != null)
        {
            teksSkor.text = skor.ToString();
        }
    }
    // ===========================================


    // =====================================================
    // SAMPAH TERDEKAT
    // =====================================================

    void sampahTerjenisKelaminTerdekat(GameObject obj)
    {
        sampahTerdekat = obj;

        AmbilSampah();
    }


    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == sampahTerdekat)
        {
            sampahTerdekat = null;
        }
    }


    // =====================================================
    // AMBIL SAMPAH
    // =====================================================

    void AmbilSampah()
    {
        if (sampahTerdekat == null ||
            sedangMemungut)
        {
            return;
        }


        sedangMemungut = true;

        rb.linearVelocity = Vector2.zero;


        // ==========================================
        // SIMPAN ARAH SAAT MULAI PICKUP
        // ==========================================

        string animasiPickup;


        if (arahHadap == "Kanan")
        {
            animasiPickup = "PlayerPickUpRight";
        }
        else
        {
            animasiPickup = "PlayerPickUpLeft";
        }


        // Pickup dimainkan satu kali
        anim.Play(
            animasiPickup,
            0,
            0f
        );


        // ==========================================
        // SAMPAH DIAMBIL
        // ==========================================

        sampahYangDibawa = sampahTerdekat;


        Collider2D colSampah =
            sampahYangDibawa.GetComponent<Collider2D>();


        if (colSampah != null)
        {
            colSampah.enabled = false;
        }


        // Sampah ikut Player
        sampahYangDibawa.transform.SetParent(
            transform
        );


        sampahYangDibawa.transform.localPosition =
            new Vector3(
                0.2f,
                -0.15f,
                0f
            );


        sampahYangDibawa.transform.localScale =
            new Vector3(
                0.35f,
                0.35f,
                1f
            );


        spriteRenderer.sortingOrder = 10;


        SpriteRenderer sampahSR =
            sampahYangDibawa.GetComponent<SpriteRenderer>();


        if (sampahSR != null)
        {
            sampahSR.sortingOrder = 11;
        }


        sampahTerdekat = null;


        Debug.Log(
            "Sampah berhasil diangkat!"
        );


        // ==========================================
        // TUNGGU PICKUP SELESAI
        // ==========================================

        float durasiPickup =
            AmbilDurasiAnimasi(
                animasiPickup
            );


        CancelInvoke(
            nameof(SelesaiMemungut)
        );


        Invoke(
            nameof(SelesaiMemungut),
            durasiPickup
        );
    }


    // =====================================================
    // DURASI ANIMASI PICKUP
    // =====================================================

    float AmbilDurasiAnimasi(string namaAnimasi)
    {
        RuntimeAnimatorController controller =
            anim.runtimeAnimatorController;


        foreach (AnimationClip clip
                 in controller.animationClips)
        {
            if (clip.name == namaAnimasi)
            {
                return clip.length;
            }
        }


        return 0.35f;
    }


    // =====================================================
    // PICKUP SELESAI
    // =====================================================

    void SelesaiMemungut()
    {
        sedangMemungut = false;


        // ==========================================
        // PENTING:
        // CEK ARAH TERBARU
        //
        // BUKAN arah ketika pickup dimulai.
        // ==========================================

        if (arahHadap == "Kanan")
        {
            // Setelah pickup kalau terakhir tekan kanan
            // langsung menghadap kanan
            anim.Play(
                "PlayerWalkRight",
                0,
                0f
            );
        }
        else
        {
            // Setelah pickup kalau terakhir tekan kiri
            // langsung menghadap kiri
            anim.Play(
                "PlayerWalkLeft",
                0,
                0f
            );
        }


        // ==========================================
        // Kalau tombol masih ditekan,
        // langsung lanjut bergerak.
        // ==========================================

        if (moveInput != Vector2.zero)
        {
            rb.linearVelocity =
                moveInput * moveSpeed;
        }
        else
        {
            rb.linearVelocity =
                Vector2.zero;
        }
    }
}