using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Hand Settings")]
    public Transform handPosition;
    public LayerMask interactionLayerMask = 1;

    private GameObject currentItemInHand;
    private InventoryManager.InventoryItem currentInventoryItem;

    [Header("References")]
    public InventoryManager inventoryManager;
    public BodyManager bodyManager; // AJOUTÉ : Référence au BodyManager

    void Update()
    {
        // Vérifier si l'inventaire OU le panneau du corps est ouvert
        bool inventoryOpen = inventoryManager != null && inventoryManager.inventoryPanel.activeSelf;
        
        if (!inventoryOpen)
        {
            if (Input.GetMouseButtonDown(0) && currentItemInHand != null)
            {
                UseItemInHand();
            }
            else if (Input.GetMouseButtonDown(1) && currentItemInHand != null)
            {
                ReturnItemToInventory();
            }
        }
    }

    public void EquipItem(InventoryManager.InventoryItem item)
    {
        // Si on a déjà un item en main, le retourner à l'inventaire
        if (currentItemInHand != null)
        {
            ReturnItemToInventory();
        }

        // Créer l'objet dans la main
        if (item.prefab != null)
        {
            currentItemInHand = Instantiate(item.prefab, handPosition);
            currentItemInHand.transform.localPosition = Vector3.zero;
            currentItemInHand.transform.localRotation = Quaternion.identity;
            currentItemInHand.SetActive(true);

            // Désactiver la physique pour l'item en main
            Collider itemCollider = currentItemInHand.GetComponent<Collider>();
            if (itemCollider != null)
                itemCollider.enabled = false;

            Rigidbody itemRigidbody = currentItemInHand.GetComponent<Rigidbody>();
            if (itemRigidbody != null)
            {
                itemRigidbody.isKinematic = true;
                itemRigidbody.useGravity = false;
            }

            // Stocker la référence de l'item
            currentInventoryItem = item;

            Debug.Log($"{item.prefab.name} équipé en main !");
        }
    }

    private void UseItemInHand()
    {
        if (currentItemInHand != null)
        {
            IUsable usableItem = currentItemInHand.GetComponent<IUsable>();
            if (usableItem != null)
            {
                usableItem.UseItem();

                // MODIFIÉ : Synchroniser avec le BodyManager
                if (bodyManager != null)
                {
                    bodyManager.ClearBodySlot(0); // Vider le slot main (index 0)
                }

                Destroy(currentItemInHand);
                currentItemInHand = null;
                currentInventoryItem = null;

                Debug.Log("Objet utilisé et détruit !");
            }
            else
            {
                Debug.Log("Cet objet ne peut pas être utilisé");
            }
        }
    }

    private void ReturnItemToInventory()
    {
        if (currentItemInHand != null && currentInventoryItem != null)
        {
            // Réactiver la physique avant de remettre dans l'inventaire
            Collider itemCollider = currentItemInHand.GetComponent<Collider>();
            if (itemCollider != null)
                itemCollider.enabled = true;

            Rigidbody itemRigidbody = currentItemInHand.GetComponent<Rigidbody>();
            if (itemRigidbody != null)
            {
                itemRigidbody.isKinematic = false;
                itemRigidbody.useGravity = true;
            }

            // Essayer de remettre l'item dans l'inventaire
            bool itemReturned = inventoryManager.AddItem(
                currentInventoryItem.sprite,
                currentItemInHand,
                currentInventoryItem.size
            );

            if (itemReturned)
            {
                // MODIFIÉ : Synchroniser avec le BodyManager
                if (bodyManager != null)
                {
                    bodyManager.ClearBodySlot(0); // Vider le slot main (index 0)
                }

                currentItemInHand.SetActive(false);
                currentItemInHand = null;
                currentInventoryItem = null;
                Debug.Log("Objet remis dans l'inventaire !");
            }
            else
            {
                // Remettre la physique comme avant si échec
                if (itemCollider != null)
                    itemCollider.enabled = false;
                if (itemRigidbody != null)
                {
                    itemRigidbody.isKinematic = true;
                    itemRigidbody.useGravity = false;
                }
                
                Debug.Log("Inventaire plein ! Impossible de remettre l'objet.");
            }
        }
    }

    // AJOUTÉ : Méthode pour déposer l'item dans le monde
    public void DropItemInWorld()
    {
        if (currentItemInHand != null && currentInventoryItem != null)
        {
            Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            Quaternion spawnRot = Camera.main.transform.rotation;

            // Créer une nouvelle instance de l'objet dans le monde
            GameObject droppedObject = Instantiate(currentInventoryItem.prefab, spawnPos, spawnRot);
            droppedObject.SetActive(true);
            
            IDropable dropableItem = droppedObject.GetComponent<IDropable>();
            if (dropableItem != null)
            {
                dropableItem.DropItem();
            }

            // Synchroniser avec le BodyManager
            if (bodyManager != null)
            {
                bodyManager.ClearBodySlot(0); // Vider le slot main (index 0)
            }

            // Détruire l'item en main
            Destroy(currentItemInHand);
            currentItemInHand = null;
            currentInventoryItem = null;

            Debug.Log("Item déposé dans le monde depuis la main !");
        }
    }

    public void ClearHandItem()
    {
        if (currentItemInHand != null)
        {
            Destroy(currentItemInHand);
            currentItemInHand = null;
            currentInventoryItem = null;
            Debug.Log("Main vidée");
        }
    }

    // Méthode pour forcer le vidage sans retour à l'inventaire (utilisée par BodyManager)
    public void ForceRemoveHandItem()
    {
        if (currentItemInHand != null)
        {
            Destroy(currentItemInHand);
            currentItemInHand = null;
            currentInventoryItem = null;
        }
    }

    public bool HasItemInHand()
    {
        return currentItemInHand != null;
    }

    public GameObject GetCurrentItem()
    {
        return currentItemInHand;
    }

    // AJOUTÉ : Obtenir l'item d'inventaire actuel
    public InventoryManager.InventoryItem GetCurrentInventoryItem()
    {
        return currentInventoryItem;
    }

    // AJOUTÉ : Vérifier si on peut équiper un nouvel item
    public bool CanEquipItem()
    {
        return currentItemInHand == null;
    }
}