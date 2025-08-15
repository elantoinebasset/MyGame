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
    private RectTransform rectTransform;
    
    // Objets pour le drag & drop
    private GameObject dragObject;
    private Image dragImage;
    
    void Start()
    {
        inventoryManager = FindAnyObjectByType<InventoryManager>();
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
            // CORRIGÉ : Utilise la nouvelle méthode GetItemAtSlot
            var item = inventoryManager.GetItemAtSlot(slotIndex);
            if (item != null)
            {
                inventoryManager.ShowContextMenu(slotIndex, Input.mousePosition);
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // CORRIGÉ : Utilise la nouvelle méthode GetItemAtSlot au lieu de l'ancien système
        var item = inventoryManager.GetItemAtSlot(slotIndex);
        if (item != null)
        {
            CreateDragObject(item);
            canvasGroup.alpha = 0.6f; 
            canvasGroup.blocksRaycasts = false;
            
            Debug.Log($"Début du drag de l'objet {item.size.width}x{item.size.height} depuis le slot {slotIndex}");
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
        
        Debug.Log($"Fin du drag depuis le slot {slotIndex}");
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // Récupère le slot d'origine du drag
        InventorySlot draggedSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        
        if (draggedSlot != null && draggedSlot != this)
        {
            Debug.Log($"Drop: du slot {draggedSlot.slotIndex} vers le slot {this.slotIndex}");
            
            // Vérifier qu'il y a bien un objet à déplacer
            var itemToMove = inventoryManager.GetItemAtSlot(draggedSlot.slotIndex);
            if (itemToMove != null)
            {
                inventoryManager.SwapItems(draggedSlot.slotIndex, this.slotIndex);
            }
            else
            {
                Debug.Log(" Aucun objet à déplacer");
            }
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