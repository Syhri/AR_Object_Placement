using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Komponen untuk menempatkan prefab di permukaan AR menggunakan ARFoundation.
// Mendukung dua sistem input (New Input System dan Legacy) dan menempelkan objek ke anchor pada plane.
public class ARPlaceCube : MonoBehaviour
{
 // Manager untuk melakukan raycast dari layar ke dunia AR (plane, dsb.)
 [SerializeField] private ARRaycastManager raycastManager;
 // Manager untuk membuat/menempelkan anchor agar objek stabil di dunia AR
 [SerializeField] private ARAnchorManager anchorManager;
 // Manager yang melacak dan mengelola ARPlane (permukaan terdeteksi)
 [SerializeField] private ARPlaneManager planeManager;

 // Daftar prefab yang bisa di-spawn (set lewat Inspector)
 [SerializeField] private List<GameObject> spawnablePrefabs = new List<GameObject>();

 // Indeks prefab yang dipilih dari daftar untuk ditempatkan
 [SerializeField, Tooltip("Index of the prefab in Spawnable Prefabs to place")] private int selectedPrefabIndex =0;

 // Skala objek yang ditempatkan agar mudah terlihat dan proporsional
 [SerializeField] private float objectScale =0.3f;

 void Awake()
 {
 // Otomatis cari komponen penting jika belum di-assign lewat Inspector
 if (!raycastManager) raycastManager = FindByType<ARRaycastManager>();
 if (!anchorManager) anchorManager = FindByType<ARAnchorManager>();
 if (!planeManager) planeManager = FindByType<ARPlaneManager>();

 // Log agar mudah debugging di device/editor
 Debug.Log($"RaycastManager Found: {raycastManager != null}");
 Debug.Log($"AnchorManager Found: {anchorManager != null}");
 Debug.Log($"PlaneManager Found: {planeManager != null}");
 }

 void Update()
 {
 // Pastikan komponen penting tersedia dan prefab valid
 var prefab = GetSelectedPrefab();
 if (!raycastManager || !anchorManager || prefab == null)
 {
 return;
 }

 // Tangani input berdasarkan sistem input yang aktif
#if ENABLE_INPUT_SYSTEM
 // New Input System (hanya berjalan jika paket aktif)
 if (Touchscreen.current != null)
 {
 var touch = Touchscreen.current.primaryTouch;
 // Deteksi sentuhan pertama kali di frame ini
 if (touch.press.wasPressedThisFrame)
 {
 Debug.Log($"Touch detected at {touch.position.ReadValue()} with prefab index {selectedPrefabIndex}");
 PlaceObject(touch.position.ReadValue());
 }
 }
 // Fallback ke input mouse (berguna saat testing di editor)
 else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
 {
 Debug.Log($"Mouse click detected at {Mouse.current.position.ReadValue()} with prefab index {selectedPrefabIndex}");
 PlaceObject(Mouse.current.position.ReadValue());
 }
#elif ENABLE_LEGACY_INPUT_MANAGER
 // Legacy Input Manager
 if (Input.touchCount >0 && Input.GetTouch(0).phase == TouchPhase.Began)
 {
 Debug.Log($"Touch detected at {Input.GetTouch(0).position} with prefab index {selectedPrefabIndex}");
 PlaceObject(Input.GetTouch(0).position);
 }
#else
 // Tidak ada sistem input, keluar saja
 return;
#endif
 }

 // Mencoba menaruh objek pada posisi layar (screen space) yang ditap/klik
 void PlaceObject(Vector2 screenPosition)
 {
 // Validasi prefab terpilih
 var prefab = GetSelectedPrefab();
 if (prefab == null)
 {
 Debug.LogWarning($"No valid prefab at index {selectedPrefabIndex}. Check Spawnable Prefabs array.");
 return;
 }

 // Raycast dari layar ke dunia AR untuk mencari plane
 var rayHits = new List<ARRaycastHit>();
 Debug.Log($"Attempting Raycast at {screenPosition} for prefab: {prefab.name}");

 // Hanya terima hit pada polygon plane (bagian dalam plane terdeteksi)
 if (raycastManager.Raycast(screenPosition, rayHits, TrackableType.PlaneWithinPolygon))
 {
 Debug.Log($"Raycast HIT plane! Hit count: {rayHits.Count}");
 var hit = rayHits[0];
 ARAnchor anchor = null;

 // Coba tempelkan anchor pada ARPlane yang kena raycast
 if (planeManager != null)
 {
 var plane = planeManager.GetPlane(hit.trackableId);
 if (plane != null)
 {
 anchor = anchorManager.AttachAnchor(plane, hit.pose);
 Debug.Log($"Anchor attached to plane: {anchor != null}");
 }
 }

 // Jika gagal menempel ke plane, buat anchor mandiri di world
 if (anchor == null)
 {
 var anchorGO = new GameObject("PlacementAnchor");
 anchorGO.transform.SetPositionAndRotation(hit.pose.position, hit.pose.rotation);
 anchor = anchorGO.AddComponent<ARAnchor>();
 Debug.Log($"Standalone Anchor created: {anchor != null}");
 }

 if (anchor != null)
 {
 // Pastikan skala anchor tidak mempengaruhi anaknya
 anchor.transform.localScale = Vector3.one;

 // Spawn objek sebagai child dari anchor agar stabil relatif terhadap dunia AR
 var parent = anchor.transform;
 GameObject spawnedObject = Instantiate(prefab, hit.pose.position, hit.pose.rotation, parent);

 // Paksa aktif dan setel skala yang diinginkan
 spawnedObject.SetActive(true);
 spawnedObject.transform.localScale = Vector3.one * objectScale;

 // Pastikan semua renderer anak aktif agar terlihat
 var renderers = spawnedObject.GetComponentsInChildren<Renderer>();
 foreach (var renderer in renderers)
 {
 renderer.enabled = true;
 }

 Debug.Log($"Object {prefab.name} spawned at {spawnedObject.transform.position}, " +
 $"scale: {spawnedObject.transform.localScale}, active: {spawnedObject.activeSelf}, " +
 $"renderers: {renderers.Length}");
 }
 else
 {
 Debug.LogError("Failed to create or attach anchor!");
 }
 }
 else
 {
 // Tidak ada plane yang terkena raycast pada posisi sentuh/klik
 Debug.Log("Raycast did NOT hit any plane.");
 }
 }

 // Mengambil prefab berdasarkan indeks terpilih dengan validasi
 private GameObject GetSelectedPrefab()
 {
 // Pastikan daftar berisi item
 if (spawnablePrefabs == null || spawnablePrefabs.Count ==0)
 {
 Debug.LogWarning("Spawnable Prefabs list is empty!");
 return null;
 }

 // Clamp indeks agar tetap berada dalam rentang
 if (selectedPrefabIndex <0 || selectedPrefabIndex >= spawnablePrefabs.Count)
 {
 selectedPrefabIndex = Mathf.Clamp(selectedPrefabIndex,0, spawnablePrefabs.Count -1);
 Debug.Log($"Clamped selected index to {selectedPrefabIndex}");
 }

 var prefab = spawnablePrefabs[selectedPrefabIndex];
 if (prefab == null)
 {
 Debug.LogWarning($"Null prefab at index {selectedPrefabIndex}!");
 }

 return prefab;
 }

 // Mengatur indeks prefab yang dipilih dari UI/luar
 public void SetSelectedPrefabIndex(int index)
 {
 Debug.Log($"Setting prefab index to {index}");
 if (spawnablePrefabs == null || spawnablePrefabs.Count ==0) return;
 selectedPrefabIndex = Mathf.Clamp(index,0, spawnablePrefabs.Count -1);
 Debug.Log($"Selected prefab: {spawnablePrefabs[selectedPrefabIndex]?.name ?? "null"}");
 }

 // Helper untuk mencari komponen di scene yang kompatibel lintas versi Unity
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