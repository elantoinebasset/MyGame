// Pour Verifer si on peut utiliser les clicks sur les slots
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Slot Settings")]
    public int slotIndex;
    
    private InventoryManager inventoryManager;
    
    void Start()
    {
        inventoryManager = FindAnyObjectByType<InventoryManager>();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Vérifie si c'est un clic droit ET si le slot contient un objet
        if (eventData.button == PointerEventData.InputButton.Right || eventData.button == PointerEventData.InputButton.Left)
        {
            if (inventoryManager != null && inventoryManager.items[slotIndex] != null)
            {
                // Affiche le menu contextuel
                inventoryManager.ShowContextMenu(slotIndex, Input.mousePosition);
            }
        }
    }
}