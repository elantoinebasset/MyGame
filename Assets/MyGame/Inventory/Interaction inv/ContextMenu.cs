// Me permet d'interagir avec les slots d'inventaire (configurer les clics et l'aparition du menu contextuel)
using UnityEngine;
using UnityEngine.UI;

public class ContextMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button useButton;
    public Button cancelButton;
    
    private InventoryManager inventoryManager;
    private int currentSlotIndex = -1;
    
    void Start()
    {
        inventoryManager = FindAnyObjectByType<InventoryManager>();
        

        useButton.onClick.AddListener(UseItem);
        cancelButton.onClick.AddListener(CloseMenu);
        

        gameObject.SetActive(false);
    }
    
    public void ShowMenu(int slotIndex, Vector3 mousePosition)
    {
        currentSlotIndex = slotIndex;
        

        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = mousePosition;
        

        gameObject.SetActive(true);
    }
    
    public void UseItem()
    {
        if (inventoryManager != null && currentSlotIndex >= 0)
        {
            inventoryManager.UseItem(currentSlotIndex);
        }
        CloseMenu();
    }
    
    public void CloseMenu()
    {
        Debug.Log("Menu contextuel fermé (Annuler)");
        gameObject.SetActive(false);
        currentSlotIndex = -1;
    }
}