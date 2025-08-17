using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Hand Settings")]
    public Transform handPosition;
    public LayerMask interactionLayerMask = 1;   

    [Header("References")]
    public InventoryManager inventoryManager;

    // Variables privées
    private GameObject currentItemInHand;
    private InventoryManager.InventoryItem currentInventoryItem;

    void Update()
    {
        // Ne rien faire si l'inventaire est ouvert ou si on n'a rien en main
        if (IsInventoryOpen() || currentItemInHand == null)
            return;

        if (Input.GetMouseButtonDown(0))
            UseItemInHand();
        else if (Input.GetMouseButtonDown(1))
            ReturnItemToInventory();
    }


    /// Équipe un objet dans la main du joueur
    public void EquipItem(InventoryManager.InventoryItem item)
    {
        // Si on a déjà quelque chose en main, le remettre dans l'inventaire
        if (currentItemInHand != null)
            ReturnItemToInventory();

        if (item.prefab == null)
            return;

        CreateHandItem(item);
        currentInventoryItem = item;
    }

    public void ClearHandItem()
    {
        if (currentItemInHand != null)
        {
            Destroy(currentItemInHand);
            currentItemInHand = null;
            currentInventoryItem = null;
        }
    }

    /// Force le retrait de l'objet sans le remettre dans l'inventaire
    public void ForceRemoveHandItem() => ClearHandItem();

    /// Fait tomber l'objet en main dans le monde
    public void DropItemInWorld()
    {
        if (currentItemInHand == null || currentInventoryItem == null)
            return;

        var spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        var dropped = Instantiate(currentInventoryItem.prefab, spawnPos, Camera.main.transform.rotation);
        dropped.SetActive(true);
        dropped.GetComponent<IDropable>()?.DropItem();

        ClearHandItem();
    }

    // === PROPRIÉTÉS PUBLIQUES  ===
    public bool HasItemInHand() => currentItemInHand != null;
    public GameObject GetCurrentItem() => currentItemInHand;
    public InventoryManager.InventoryItem GetCurrentInventoryItem() => currentInventoryItem;
    public bool CanEquipItem() => currentItemInHand == null;

    // === MÉTHODES PRIVÉES ===

    /// Vérifie si l'inventaire est actuellement ouvert
    private bool IsInventoryOpen() => inventoryManager?.inventoryPanel.activeSelf == true;

    /// Crée l'objet dans la main du joueur
    private void CreateHandItem(InventoryManager.InventoryItem item)
    {
        // Créer l'objet en tant qu'enfant de la position de la main
        currentItemInHand = Instantiate(item.prefab, handPosition);
        currentItemInHand.transform.localPosition = Vector3.zero;
        currentItemInHand.transform.localRotation = Quaternion.identity;
        currentItemInHand.SetActive(true);

        // Configurer la physique pour qu'il reste dans la main
        SetupHandItemPhysics(false);
    }

    /// Configure la physique de l'objet (collisions, gravité, etc.)
    private void SetupHandItemPhysics(bool enablePhysics)
    {
        if (currentItemInHand == null)
            return;

        var collider = currentItemInHand.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = enablePhysics;

        var rigidbody = currentItemInHand.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = !enablePhysics;
            rigidbody.useGravity = enablePhysics;
        }
    }

    /// Utilise l'objet que le joueur a en main
    private void UseItemInHand()
    {
        var usableItem = currentItemInHand.GetComponent<IUsable>();
        if (usableItem != null)
        {
            usableItem.UseItem();
            ClearHandItem();
        }
    }

    /// Remet l'objet dans l'inventaire
    private void ReturnItemToInventory()
    {
        if (currentItemInHand == null || currentInventoryItem == null)
            return;

        // Réactiver la physique temporairement
        SetupHandItemPhysics(true);

        // Essayer de remettre l'objet dans l'inventaire
        if (inventoryManager.AddItem(currentInventoryItem.sprite, currentItemInHand, currentInventoryItem.size))
        {

            currentItemInHand.SetActive(false);
            currentItemInHand = null;
            currentInventoryItem = null;
        }
        else
        {
            SetupHandItemPhysics(false);
            Debug.Log("Impossible de remettre l'objet dans l'inventaire - inventaire plein !");
        }
    }
}