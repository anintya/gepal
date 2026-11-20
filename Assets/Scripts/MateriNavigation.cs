
using UnityEngine;

public class MateriNavigation : MonoBehaviour
{
    [Header("Panel Materi")]
    public GameObject panelOrganik;
    public GameObject panelAnorganik;

    // Menampilkan materi organik
    public void TampilkanOrganik()
    {
        panelOrganik.SetActive(true);
        panelAnorganik.SetActive(false);
    }

    // Menampilkan materi anorganik
    public void TampilkanAnorganik()
    {
        panelOrganik.SetActive(false);
        panelAnorganik.SetActive(true);
    }
}
