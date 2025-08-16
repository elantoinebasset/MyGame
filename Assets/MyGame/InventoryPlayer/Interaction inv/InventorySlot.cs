using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Slot Settings")]
    public int slotIndex;
    
    private InventoryManager inventoryManager;
    private BodyManager bodyManager; // AJOUTÉ : Référence au BodyManager
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    
    // Objets pour le drag & drop
    private GameObject dragObject;
    private Image dragImage;
    
    void Start()
    {
        inventoryManager = FindAnyObjectByType<InventoryManager>();
        bodyManager = FindAnyObjectByType<BodyManager>(); // AJOUTÉ : Récupération du BodyManager
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        // Ajoute CanvasGroup si pas présent
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Clic droit = menu contextuel
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var item = inventoryManager.GetItemAtSlot(slotIndex);
            if (item != null)
            {
                inventoryManager.ShowContextMenu(slotIndex, Input.mousePosition);
            }
        }
        // AJOUTÉ : Double-clic pour équiper dans la main (slot 0 du corps)
        else if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            QuickEquipToHand();
        }
    }
    
    // AJOUTÉ : Méthode pour équiper rapidement dans la main
    private void QuickEquipToHand()
    {
        if (bodyManager == null || inventoryManager == null) return;
        
        var item = inventoryManager.GetItemAtSlot(slotIndex);
        if (item != null)
        {
            if (bodyManager.TransferFromInventoryToBody(slotIndex, 0)) // Slot 0 = main
            {
                Debug.Log("Item équipé rapidement dans la main");
            }
            else
            {
                Debug.Log("Impossible d'équiper l'item dans la main");
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        var item = inventoryManager.GetItemAtSlot(slotIndex);
        if (item != null)
        {
            CreateDragObject(item);
            canvasGroup.alpha = 0.6f; 
            canvasGroup.blocksRaycasts = false;
            
            Debug.Log($"Début du drag de l'objet {item.size.width}x{item.size.height} depuis le slot inventaire {slotIndex}");
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            dragObject.transform.position = Input.mousePosition;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            Destroy(dragObject);
            dragObject = null;
        }
        
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        Debug.Log($"Fin du drag depuis le slot inventaire {slotIndex}");
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // Gestion du drop depuis un autre slot d'inventaire
        InventorySlot draggedSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (draggedSlot != null && draggedSlot != this)
        {
            Debug.Log($"Drop inventaire vers inventaire: du slot {draggedSlot.slotIndex} vers le slot {this.slotIndex}");
            
            var itemToMove = inventoryManager.GetItemAtSlot(draggedSlot.slotIndex);
            if (itemToMove != null)
            {
                inventoryManager.SwapItems(draggedSlot.slotIndex, this.slotIndex);
            }
            else
            {
                Debug.Log("Aucun objet à déplacer");
            }
            return;
        }
        
        // AJOUTÉ : Gestion du drop depuis les slots du corps
        BodySlots draggedBodySlot = eventData.pointerDrag.GetComponent<BodySlots>();
        if (draggedBodySlot != null && bodyManager != null)
        {
            Debug.Log($"Drop corps vers inventaire: du slot corps {draggedBodySlot.slotIndex} vers le slot inventaire {this.slotIndex}");
            
            var itemToMove = bodyManager.GetEquippedItem(draggedBodySlot.slotIndex);
            if (itemToMove != null)
            {
                // Transférer l'item du corps vers l'inventaire
                TransferFromBodyToInventory(draggedBodySlot.slotIndex);
            }
            return;
        }
    }
    
    // AJOUTÉ : Méthode pour transférer depuis le corps vers l'inventaire
    private void TransferFromBodyToInventory(int bodySlotIndex)
    {
        if (bodyManager == null || inventoryManager == null) return;
        
        var bodyItem = bodyManager.GetEquippedItem(bodySlotIndex);
        if (bodyItem == null) return;
        
        var currentItem = inventoryManager.GetItemAtSlot(slotIndex);
        
        if (currentItem == null)
        {
            // Slot libre, simplement transférer
            if (inventoryManager.AddItem(bodyItem.sprite, bodyItem.prefab, bodyItem.size))
            {
                // Vider le slot du corps après transfert réussi
                bodyManager.ClearBodySlot(bodySlotIndex);
                Debug.Log($"Item transféré du corps vers l'inventaire (slot {slotIndex})");
            }
        }
        else
        {
            Debug.Log("Slot d'inventaire occupé, échange non implémenté pour l'instant");
            // Tu peux implémenter l'échange si nécessaire
        }
    }
    
    private void CreateDragObject(InventoryManager.InventoryItem item)
    {
        dragObject = new GameObject("DragObject");
        dragObject.transform.SetParent(canvas.transform, false);
        dragObject.transform.SetAsLastSibling();
        
        dragImage = dragObject.AddComponent<Image>();
        dragImage.sprite = item.sprite;
        dragImage.color = new Color(1, 1, 1, 0.8f);
        dragImage.raycastTarget = false;
        
        RectTransform dragRect = dragObject.GetComponent<RectTransform>();
        
        float baseSize = 80f; // Taille de base d'un slot
        float spacing = 5f;   // Espacement
        
        float dragWidth = baseSize * item.size.width + spacing * (item.size.width - 1);
        float dragHeight = baseSize * item.size.height + spacing * (item.size.height - 1);
        
        dragRect.sizeDelta = new Vector2(dragWidth, dragHeight);
        
        Debug.Log($"Objet de drag créé avec taille {dragWidth}x{dragHeight} pour un item {item.size.width}x{item.size.height}");
    }
}