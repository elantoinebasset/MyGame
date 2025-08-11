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
        // Clic droit = menu contextuel (comme avant)
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (inventoryManager != null && inventoryManager.items[slotIndex] != null)
            {
                inventoryManager.ShowContextMenu(slotIndex, Input.mousePosition);
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Démarre le drag seulement s'il y a un objet
        if (inventoryManager.items[slotIndex] != null)
        {
            CreateDragObject();
            canvasGroup.alpha = 0.6f; 
            canvasGroup.blocksRaycasts = false; 
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
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // Récupère le slot d'origine du drag
        InventorySlot draggedSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        
        if (draggedSlot != null && draggedSlot != this)
        {
            // Échange les objets entre les deux slots
            inventoryManager.SwapItems(draggedSlot.slotIndex, this.slotIndex);
        }
    }
    
    private void CreateDragObject()
    {
        
        dragObject = new GameObject("DragObject");
        dragObject.transform.SetParent(canvas.transform, false);
        dragObject.transform.SetAsLastSibling();
        
        dragImage = dragObject.AddComponent<Image>();
        dragImage.sprite = inventoryManager.items[slotIndex];
        dragImage.color = new Color(1, 1, 1, 0.8f);
        dragImage.raycastTarget = false;
        
        
        RectTransform dragRect = dragObject.GetComponent<RectTransform>();
        dragRect.sizeDelta = rectTransform.sizeDelta;
    }
}