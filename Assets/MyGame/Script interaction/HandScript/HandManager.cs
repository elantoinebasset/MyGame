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



    void Update()
    {
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
        
        if (currentItemInHand != null)
        {
            ReturnItemToInventory();
        }
        
        // Pour créer mon objet dans la main
        if (item.prefab != null)
        {
            currentItemInHand = Instantiate(item.prefab, handPosition);
            currentItemInHand.transform.localPosition = Vector3.zero;
            currentItemInHand.transform.localRotation = Quaternion.identity;
            currentItemInHand.SetActive(true);
            
            Collider itemCollider = currentItemInHand.GetComponent<Collider>();
            if (itemCollider != null)
                itemCollider.enabled = false;

            Rigidbody itemRigidbody = currentItemInHand.GetComponent<Rigidbody>();
            if (itemRigidbody != null)
            {
                itemRigidbody.isKinematic = true;
                itemRigidbody.useGravity = false; 
            }

            
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
                
                
                Destroy(currentItemInHand);
                currentItemInHand = null;
                currentInventoryItem = null;
                
                Debug.Log("Objet utilisé et détruit !");
            }
        }
    }
    
    private void ReturnItemToInventory()
    {
        if (currentItemInHand != null && currentInventoryItem != null)
        {
            bool itemReturned = inventoryManager.AddItem(
                currentInventoryItem.sprite, 
                currentItemInHand, 
                currentInventoryItem.size
            );

            Collider itemCollider = currentItemInHand.GetComponent<Collider>();
            if (itemCollider != null)
                itemCollider.enabled = true;

            Rigidbody itemRigidbody = currentItemInHand.GetComponent<Rigidbody>();
            if (itemRigidbody != null)
            {
                itemRigidbody.isKinematic = false;
                itemRigidbody.useGravity = true; 
            }
            
            if (itemReturned)
            {
                currentItemInHand.SetActive(false);
                currentItemInHand = null;
                currentInventoryItem = null;
                Debug.Log("Objet remis dans l'inventaire !");
            }
            else
            {
                Debug.Log("Inventaire plein ! Impossible de remettre l'objet.");
            }
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
}