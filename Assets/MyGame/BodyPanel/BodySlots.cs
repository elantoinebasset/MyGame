using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BodySlots : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Slot Settings")]
    public int slotIndex;
    
    private BodyManager bodyManager;
    private InventoryManager inventoryManager;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    

    private GameObject dragObject;
    private Image dragImage;

    [System.Serializable]
public class SlotItemFilter
{
    [Header("Slot Type")]
    public BodyManager.SlotType slotType;
    
    [Header("Allowed Item Names (contains)")]
    public string[] allowedItems;
    
    [Header("Allow All Items")]
    public bool allowAll = false;
}
    
    void Start()
    {
        bodyManager = FindAnyObjectByType<BodyManager>();
        inventoryManager = FindAnyObjectByType<InventoryManager>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        Debug.Log($"BodySlot {slotIndex} initialisé ({bodyManager.GetSlotType(slotIndex)})");
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (bodyManager == null) return;
        
        // Clic droit = menu contextuel ou actions spéciales
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var equippedItem = bodyManager.GetEquippedItem(slotIndex);
            if (equippedItem != null)
            {
                ShowBodyContextMenu();
            }
        }
        // Double-clic gauche = retourner à l'inventaire
        else if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            ReturnToInventory();
        }
    }
    
    private void ShowBodyContextMenu()
    {
        // Utilise le menu contextuel existant du BodyManager
        if (bodyManager != null && bodyManager.contextMenu != null)
        {
            bodyManager.contextMenu.ShowMenuForBodySlot(slotIndex, Input.mousePosition);
            Debug.Log($"Menu contextuel affiché pour slot {slotIndex} ({bodyManager.GetSlotType(slotIndex)})");
        }
    }
    
    private void ReturnToInventory()
    {
        if (bodyManager != null)
        {
            if (bodyManager.ReturnItemToInventory(slotIndex))
            {
                Debug.Log($"Item retiré du slot {slotIndex} et remis dans l'inventaire");
            }
            else
            {
                Debug.Log("Impossible de retourner l'item à l'inventaire (inventaire plein ?)");
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (bodyManager == null) return;
        
        var equippedItem = bodyManager.GetEquippedItem(slotIndex);
        if (equippedItem != null)
        {
            CreateDragObject(equippedItem);
            canvasGroup.alpha = 0.6f; 
            canvasGroup.blocksRaycasts = false;
            
            Debug.Log($"Début du drag de l'item depuis le slot corps {slotIndex} ({bodyManager.GetSlotType(slotIndex)})");
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
        
        Debug.Log($"Fin du drag depuis le slot corps {slotIndex}");
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (bodyManager == null) return;
        
        BodySlots draggedBodySlot = eventData.pointerDrag.GetComponent<BodySlots>();
        if (draggedBodySlot != null && draggedBodySlot != this)
        {
            Debug.Log($"Drop corps vers corps: du slot {draggedBodySlot.slotIndex} vers le slot {this.slotIndex}");
            
            var itemToMove = bodyManager.GetEquippedItem(draggedBodySlot.slotIndex);
            if (itemToMove != null)
            {
                // Vérifier si l'item peut être équipé dans le slot de destination
                if (CanEquipItemInSlot(itemToMove, this.slotIndex))
                {
                    // Si le slot de destination a déjà un item, vérifier aussi si cet item peut aller dans le slot source
                    var destinationItem = bodyManager.GetEquippedItem(this.slotIndex);
                    if (destinationItem != null)
                    {
                        // Il y a un item dans le slot de destination, vérifier s'il peut aller dans le slot source
                        if (CanEquipItemInSlot(destinationItem, draggedBodySlot.slotIndex))
                        {
                            bodyManager.SwapBodySlots(draggedBodySlot.slotIndex, this.slotIndex);
                        }
                        else
                        {
                            Debug.Log($"L'item de destination ne peut pas être équipé dans le slot source {bodyManager.GetSlotType(draggedBodySlot.slotIndex)}");
                        }
                    }
                    else
                    {
                        // Slot de destination vide, simple déplacement
                        bodyManager.SwapBodySlots(draggedBodySlot.slotIndex, this.slotIndex);
                    }
                }
                else
                {
                    Debug.Log($"Cet item ne peut pas être équipé dans le slot {bodyManager.GetSlotType(this.slotIndex)}");
                }
            }
            return;
        }
        

        // Gestion du drop depuis l'inventaire
        InventorySlot draggedInventorySlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (draggedInventorySlot != null && inventoryManager != null)
        {
            Debug.Log($"Drop inventaire vers corps: du slot inventaire {draggedInventorySlot.slotIndex} vers le slot corps {this.slotIndex}");
            
            var itemToMove = inventoryManager.GetItemAtSlot(draggedInventorySlot.slotIndex);
            if (itemToMove != null)
            {
                // Vérifier si l'item peut être équipé dans ce type de slot
                if (CanEquipItemInSlot(itemToMove, slotIndex))
                {
                    bodyManager.TransferFromInventoryToBody(draggedInventorySlot.slotIndex, this.slotIndex);
                }
                else
                {
                    Debug.Log($"Cet item ne peut pas être équipé dans le slot {bodyManager.GetSlotType(slotIndex)}");
                }
            }
            return;
        }
    }
    
    
    

    



        private bool CanEquipItemInSlot(InventoryManager.InventoryItem item, int targetSlot)
    {
            if (bodyManager == null || item == null || item.prefab == null)
    {
        Debug.LogWarning("bodyManager, item ou item.prefab est null");
        return false;
    }

        //                                                                                                          !!!!!!!!!!!!!!!!!!!!!
        // Me permet d'ajouter des tags ou filtres pour les items

        BodyManager.SlotType slotType = bodyManager.GetSlotType(targetSlot);

        string itemName = item.prefab.name;

        switch (slotType)
        {
            case BodyManager.SlotType.Hand:

                return true;

            case BodyManager.SlotType.Head:

                return itemName.Contains("Test");

            case BodyManager.SlotType.Body:

                return itemName.Contains("Armor");

            case BodyManager.SlotType.Feet:

                return itemName.Contains("Boot");

            default:
                return true;
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

        // Taille basée sur la taille de l'item
        float baseSize = 80f;
        float spacing = 5f;

        float dragWidth = baseSize * item.size.width + spacing * (item.size.width - 1);
        float dragHeight = baseSize * item.size.height + spacing * (item.size.height - 1);

        dragRect.sizeDelta = new Vector2(dragWidth, dragHeight);

        Debug.Log($"Objet de drag créé pour item corps {item.size.width}x{item.size.height}");
    }
    
    public void DropItem()
    {
        if (bodyManager != null)
        {
            bodyManager.DropBodyItem(slotIndex);
        }
    }
    
    public bool HasItem()
    {
        return bodyManager != null && bodyManager.GetEquippedItem(slotIndex) != null;
    }
    
    public BodyManager.SlotType GetSlotType()
    {
        return bodyManager != null ? bodyManager.GetSlotType(slotIndex) : BodyManager.SlotType.Hand;
    }
}