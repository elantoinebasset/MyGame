using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HandSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Hand Slot UI")]
    public Image slotImage;
    public Color emptyColor = Color.gray;
    public Color filledColor = Color.white;

    [Header("References")]
    public HandManager handManager;
    public InventoryManager inventoryManager;
    public ContextMenu contextMenu;

    // Variables privées
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private GameObject dragObject;

    void Start()
    {
        if (handManager == null)
            handManager = FindAnyObjectByType<HandManager>();
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        if (contextMenu == null)
            contextMenu = FindAnyObjectByType<ContextMenu>();

        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        UpdateVisual();
    }

    void Update()
    {

        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (slotImage == null || handManager == null) return;

        var currentItem = handManager.GetCurrentInventoryItem();

        if (currentItem != null)
        {
            // Il y a un objet en main
            slotImage.sprite = currentItem.sprite;
            slotImage.color = filledColor;
        }
        else
        {
            // Main vide
            slotImage.sprite = null;
            slotImage.color = emptyColor;
        }
    }
        // === GESTION DES CLICS ===
    
    // Gestion des clics sur le slot de main
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Clic droit : menu contextuel si objet en main
            if (handManager.HasItemInHand())
                ShowContextMenuForHand();
        }
    }

    // === GESTION DU DRAG & DROP ===

    // Début du drag depuis le slot de main
    public void OnBeginDrag(PointerEventData eventData)
    {
        var item = handManager.GetCurrentInventoryItem();
        if (item != null)
        {
            CreateDragObject(item);
            SetDragState(true);
        }
    }

    // Pendant le drag
    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
            dragObject.transform.position = Input.mousePosition;
    }

    // Fin du drag
    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            Destroy(dragObject);
            dragObject = null;
        }
        SetDragState(false);
    }

    // Drop d'un objet sur le slot de main
    public void OnDrop(PointerEventData eventData)
    {
        var draggedInventorySlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        
        if (draggedInventorySlot != null)
        {
            // Objet vient de l'inventaire vers la main
            HandleInventoryToHand(draggedInventorySlot);
        }
        else
        {
            var draggedHandSlot = eventData.pointerDrag.GetComponent<HandSlot>();
            if (draggedHandSlot != null && draggedHandSlot != this)
            {
                // Échange entre deux slots de main (si vous en avez plusieurs)
                HandleHandToHand(draggedHandSlot);
            }
        }
    }

    // === MÉTHODES PRIVÉES ===

    // Gère le transfert d'un objet de l'inventaire vers la main
    private void HandleInventoryToHand(InventorySlot inventorySlot)
    {
        var item = inventoryManager.GetItemAtSlot(inventorySlot.slotIndex);
        if (item != null)
        {
            // Si on a déjà quelque chose en main, l'échanger
            if (handManager.HasItemInHand())
            {
                var currentHandItem = handManager.GetCurrentInventoryItem();
                
                // Remettre l'objet de la main dans l'inventaire
                handManager.ClearHandItem();
                
                // Équiper le nouvel objet
                inventoryManager.RemoveItem(item);
                handManager.EquipItem(item);
                
                // Essayer de mettre l'ancien objet dans l'inventaire
                if (!inventoryManager.AddItem(currentHandItem.sprite, currentHandItem.prefab, currentHandItem.size))
                {
                    Debug.Log("Impossible d'échanger - inventaire plein !");
                    // Remettre l'ancien objet en main si échec
                    handManager.EquipItem(currentHandItem);
                    inventoryManager.AddItem(item.sprite, item.prefab, item.size);
                }
            }
            else
            {
                // Main vide, équiper directement
                inventoryManager.RemoveItem(item);
                handManager.EquipItem(item);
            }
        }
    }

    // Gère l'échange entre deux slots de main
    private void HandleHandToHand(HandSlot otherHandSlot)
    {
        // Pour le moment, ne rien faire car on a qu'un slot de main
        // Cette méthode sera utile si vous ajoutez plusieurs slots de main
    }

    // Affiche le menu contextuel pour l'objet en main
    private void ShowContextMenuForHand()
    {
        if (contextMenu != null)
            contextMenu.ShowMenuForHandSlot(Input.mousePosition);
    }

    // Crée l'objet visuel pendant le traînement
    private void CreateDragObject(InventoryManager.InventoryItem item)
    {
        dragObject = new GameObject("DragObjectFromHand");
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

    // Change l'apparence pendant le traînement
    private void SetDragState(bool dragging)
    {
        canvasGroup.alpha = dragging ? 0.6f : 1f;
        canvasGroup.blocksRaycasts = !dragging;
    }
}
