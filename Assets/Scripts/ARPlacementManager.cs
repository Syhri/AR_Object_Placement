using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

// Manager UI untuk memilih prefab yang akan ditempatkan dan mengatur visibilitas plane AR.
// Melakukan auto-find referensi jika belum di-assign di Inspector dan menyediakan UI fallback runtime.
public class ARPlacementManager : MonoBehaviour
{
    [Header("Scene refs (optional, auto-find if null)")]
    [SerializeField] private ARPlaceCube placement; // Komponen penempatan objek AR
    [SerializeField] private ARPlaneManager planeManager; // Mengelola deteksi plane AR
    [SerializeField] private UIDocument uiDocument; // Dokumen UI Toolkit di scene

    // Referensi elemen UI Toolkit (opsional, diambil via query name)
    private VisualElement imageButtonAnimal1, imageButtonAnimal2, imageButtonAnimal3;
    private Button buttonTogglePlane;
    private Label titleLabel;

    // Status visibilitas plane saat ini
    private bool arePlanesVisible = true;

    void Awake()
    {
        // Auto-find AR components jika belum diset lewat Inspector (prioritaskan komponen pada GameObject ini)
        if (!placement)
        {
            placement = GetComponent<ARPlaceCube>();
            if (!placement) placement = FindByType<ARPlaceCube>();
        }
        if (!planeManager)
        {
            planeManager = GetComponent<ARPlaneManager>();
            if (!planeManager) planeManager = FindByType<ARPlaneManager>();
        }
        if (!uiDocument) uiDocument = FindByType<UIDocument>();

        // Inisialisasi binding UI jika dokumen tersedia
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;

            imageButtonAnimal1 = root.Q<VisualElement>("ImageButtonAnimal1");
            imageButtonAnimal2 = root.Q<VisualElement>("ImageButtonAnimal2");
            imageButtonAnimal3 = root.Q<VisualElement>("ImageButtonAnimal3");
            buttonTogglePlane = root.Q<Button>("ButtonTogglePlane");
            titleLabel = root.Q<Label>("Label");

            // Pasang handler klik untuk memilih prefab sesuai indeks
            imageButtonAnimal1?.RegisterCallback<ClickEvent>(_ => SelectPrefabToSpawn(0));
            imageButtonAnimal2?.RegisterCallback<ClickEvent>(_ => SelectPrefabToSpawn(1));
            imageButtonAnimal3?.RegisterCallback<ClickEvent>(_ => SelectPrefabToSpawn(2));
            // Tombol untuk menampilkan/menyembunyikan plane
            buttonTogglePlane?.RegisterCallback<ClickEvent>(_ => TogglePlaneVisibility());
        }

        // Jika tidak ada UI/elemen ditemukan, bangun UI minimal saat runtime (berguna di device tanpa UXML)
        if (uiDocument == null || (imageButtonAnimal1 == null && imageButtonAnimal2 == null && imageButtonAnimal3 == null && buttonTogglePlane == null))
        {
            CreateFallbackUI();
        }

        // Sinkronkan teks tombol dengan status awal
        UpdatePlaneButtonText();

        // Terapkan visibilitas plane awal
        SetAllPlanesActive(arePlanesVisible);
    }

    // Membangun UI fallback sederhana sepenuhnya dari script
    private void CreateFallbackUI()
    {
        // Pastikan ada UIDocument aktif; buat baru jika belum ada
        if (uiDocument == null)
        {
            var go = new GameObject("RuntimeUIDocument");
            go.transform.SetParent(transform, false);
            uiDocument = go.AddComponent<UIDocument>();
            // Buat PanelSettings runtime agar UI tampil pada Play maupun device
            var ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.name = "RuntimePanelSettings";
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(1080, 1920);
            ps.match = 0.5f;
            ps.sortingOrder = 1000; // pastikan UI berada di atas
            uiDocument.panelSettings = ps;
        }

        var root = uiDocument.rootVisualElement;
        root.style.flexDirection = FlexDirection.Column;
        root.style.alignItems = Align.FlexStart;
        root.style.paddingLeft = 12;
        root.style.paddingTop = 12;
        // Catatan: 'gap' mungkin tidak tersedia di beberapa versi Unity; gunakan margin per-child

        // Judul UI
        titleLabel = new Label("AR Placement");
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        titleLabel.style.fontSize = 20;
        titleLabel.style.marginBottom = 8;
        root.Add(titleLabel);

        // Tiga tombol fallback untuk memilih prefab index0/1/2
        var b1 = new Button(() => SelectPrefabToSpawn(0)) { text = "Animal1" };
        var b2 = new Button(() => SelectPrefabToSpawn(1)) { text = "Animal2" };
        var b3 = new Button(() => SelectPrefabToSpawn(2)) { text = "Animal3" };
        b1.style.width = b2.style.width = b3.style.width = 140;
        b1.style.marginBottom = 6; b2.style.marginBottom = 6; b3.style.marginBottom = 6;
        root.Add(b1);
        root.Add(b2);
        root.Add(b3);

        // Tombol untuk toggle visibilitas plane
        buttonTogglePlane = new Button(TogglePlaneVisibility) { text = "Hide Planes" };
        buttonTogglePlane.style.marginTop = 16;
        buttonTogglePlane.style.width = 160;
        root.Add(buttonTogglePlane);
    }

    // Dipanggil dari UI untuk memilih prefab yang akan ditempatkan
    public void SelectPrefabToSpawn(int prefabIndex)
    {
        if (placement == null)
        {
            Debug.LogWarning("ARPlaceCube tidak ditemukan di scene.");
            return;
        }
        placement.SetSelectedPrefabIndex(prefabIndex);
    }

    // Toggle tampil/tidaknya plane dan update teks tombolnya
    public void TogglePlaneVisibility()
    {
        arePlanesVisible = !arePlanesVisible;
        SetAllPlanesActive(arePlanesVisible);
        UpdatePlaneButtonText();
    }

    // Mengaktif/nonaktifkan semua plane yang sedang dilacak dan hentikan/lanjutkan update-nya
    private void SetAllPlanesActive(bool value)
    {
        if (planeManager == null)
        {
            planeManager = FindByType<ARPlaneManager>();
            if (planeManager == null) return;
        }

        // Enable/disable manager agar deteksi berhenti/lanjut
        planeManager.enabled = value;
        foreach (var plane in planeManager.trackables)
        {
            if (plane && plane.gameObject)
                plane.gameObject.SetActive(value);
        }
    }

    // Menyesuaikan teks tombol sesuai status visibilitas plane
    private void UpdatePlaneButtonText()
    {
        if (buttonTogglePlane != null)
        {
            buttonTogglePlane.text = arePlanesVisible ? "Hide Planes" : "Show Planes";
        }
    }

    // Helper lintas versi Unity untuk menemukan komponen di scene
    private static T FindByType<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<T>();
#else
#pragma warning disable0618
        return Object.FindObjectOfType<T>();
#pragma warning restore0618
#endif
    }
}
