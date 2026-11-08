using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(WasteItem))]
public class DragDropWaste : MonoBehaviour
{
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private Vector3 startPosition;
    private Vector3 dragOffset;

    private bool isDragging;
    private int originalSortingOrder;

    private WasteItem wasteItem;

    private void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        wasteItem = GetComponent<WasteItem>();

        if (spriteRenderer != null)
            originalSortingOrder = spriteRenderer.sortingOrder;
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void OnMouseDown()
    {
        // JIKA GAME SUDAH SELESAI (Kalah/Menang), sampah tidak bisa di-drag lagi!
        if (GameManager.instance != null && GameManager.instance.gameSelesai)
            return;

        if (mainCamera == null) return;

        Vector3 pointerWorld = GetPointerWorldPosition();
        dragOffset = transform.position - pointerWorld;
        isDragging = true;

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 100;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
    }

    private void OnMouseDrag()
    {
        if (GameManager.instance != null && GameManager.instance.gameSelesai)
            return;

        if (!isDragging || mainCamera == null) return;

        transform.position = GetPointerWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (GameManager.instance != null && GameManager.instance.gameSelesai)
            return;

        if (!isDragging) return;

        isDragging = false;

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = originalSortingOrder;

        if (rb != null)
            rb.simulated = true;

        Vector3 dropPosition = GetPointerWorldPosition();
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(dropPosition);

        WasteBin targetBin = null;

        foreach (Collider2D hit in hitColliders)
        {
            if (hit == null) continue;

            targetBin = hit.GetComponent<WasteBin>();
            if (targetBin == null)
                targetBin = hit.GetComponentInParent<WasteBin>();

            if (targetBin != null) break;
        }

        if (targetBin == null)
        {
            ReturnToStart();
            return;
        }

        // JIKA MASUK TONG YANG BENAR (+10 Skor)
        if (targetBin.Accepts(wasteItem))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.TambahSkor(10);
            }

            Destroy(gameObject);
        }
        // JIKA SALAH MASUK TONG (-10 Skor & -1 Nyawa)
        else
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.KurangiSkor(10);
                GameManager.instance.KurangNyawa();
            }

            ReturnToStart();
        }
    }

    private Vector3 GetPointerWorldPosition()
    {
        Vector3 pointerPosition = Input.mousePosition;
        pointerPosition.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(pointerPosition);
        worldPosition.z = transform.position.z;
        return worldPosition;
    }

    private void ReturnToStart()
    {
        transform.position = startPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = true;
        }
    }
}