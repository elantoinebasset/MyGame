using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Slot Settings")]
    public int slotIndex;


    private InventoryManager inventoryManager;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private GameObject dragObject;

    void Start()
    {

        inventoryManager = FindAnyObjectByType<InventoryManager>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();


        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }


    /// Appelé quand on clique sur le slot

    public void OnPointerClick(PointerEventData eventData)
    {
        // Clic droit = afficher le menu contextuel
        if (eventData.button == PointerEventData.InputButton.Right)
            ShowContextMenu();
    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        var item = inventoryManager.GetItemAtSlot(slotIndex);
        if (item != null)
        {
            CreateDragObject(item);
            SetDragState(true);
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
            dragObject.transform.position = Input.mousePosition;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            Destroy(dragObject);
            dragObject = null;
        }
        SetDragState(false);  // Restaurer l'apparence normale
    }


        public void OnDrop(PointerEventData eventData)
    {
        var draggedInventorySlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        var draggedHandSlot = eventData.pointerDrag.GetComponent<HandSlot>();

        if (draggedInventorySlot != null && draggedInventorySlot != this)
        {
            HandleInventoryToInventory(draggedInventorySlot);
        }
        else if (draggedHandSlot != null)
        {
            HandleHandToInventory(draggedHandSlot);
        }
    }

    private void HandleInventoryToInventory(InventorySlot draggedSlot)
    {
        var itemToMove = inventoryManager.GetItemAtSlot(draggedSlot.slotIndex);
        if (itemToMove != null)
            inventoryManager.SwapItems(draggedSlot.slotIndex, slotIndex);
    }


    private void ShowContextMenu()
    {
        var item = inventoryManager.GetItemAtSlot(slotIndex);
        if (item != null)
            inventoryManager.ShowContextMenu(slotIndex, Input.mousePosition);
    }


    private void CreateDragObject(InventoryManager.InventoryItem item)
    {
        // Créer un GameObject temporaire pour le drag
        dragObject = new GameObject("DragObject");
        dragObject.transform.SetParent(canvas.transform, false);
        dragObject.transform.SetAsLastSibling();


        var dragImage = dragObject.AddComponent<Image>();
        dragImage.sprite = item.sprite;
        dragImage.color = new Color(1, 1, 1, 0.8f);
        dragImage.raycastTarget = false;


        var dragRect = dragObject.GetComponent<RectTransform>();
        float baseSize = 80f;
        float spacing = 5f;
        dragRect.sizeDelta = new Vector2(
            baseSize * item.size.width + spacing * (item.size.width - 1),
            baseSize * item.size.height + spacing * (item.size.height - 1)
        );
    }


    private void SetDragState(bool dragging)
    {
        canvasGroup.alpha = dragging ? 0.6f : 1f;
        canvasGroup.blocksRaycasts = !dragging;
    }
    






// Nouvelle méthode pour gérer le transfert de la main vers l'inventaire
private void HandleHandToInventory(HandSlot handSlot)
{
    var handManager = FindAnyObjectByType<HandManager>();
    var inventoryManager = FindAnyObjectByType<InventoryManager>();
    
    if (handManager.HasItemInHand())
    {
        var currentHandItem = handManager.GetCurrentInventoryItem();
        var existingItem = inventoryManager.GetItemAtSlot(slotIndex);
        
        if (existingItem != null)
        {
            // Il y a déjà un objet dans ce slot, faire un échange
            handManager.ClearHandItem();
            inventoryManager.RemoveItem(existingItem);
            
            // Mettre l'objet de la main dans l'inventaire
            if (inventoryManager.AddItem(currentHandItem.sprite, currentHandItem.prefab, currentHandItem.size))
            {
                // Équiper l'ancien objet de l'inventaire
                handManager.EquipItem(existingItem);
            }
            else
            {
                // Échec, remettre tout comme avant
                handManager.EquipItem(currentHandItem);
                inventoryManager.AddItem(existingItem.sprite, existingItem.prefab, existingItem.size);
            }
        }
        else
        {
            // Slot vide, mettre directement
            handManager.ClearHandItem();
            inventoryManager.AddItem(currentHandItem.sprite, currentHandItem.prefab, currentHandItem.size);
        }
    }
}
}