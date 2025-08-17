using UnityEngine;
using UnityEngine.UI;

public class ContextMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button useButton;
    public Button DropButton;
    public Button cancelButton;


    private InventoryManager inventoryManager;
    private int currentSlotIndex = -1;

    void Start()
    {
        inventoryManager = FindAnyObjectByType<InventoryManager>();


        useButton.onClick.AddListener(UseItem);
        DropButton.onClick.AddListener(DropItem);
        cancelButton.onClick.AddListener(CloseMenu);


        gameObject.SetActive(false);
    }


    /// Affiche le menu à la position de la souris pour un slot d'inventaire

    public void ShowMenu(int slotIndex, Vector3 mousePosition)
    {
        SetupMenu(slotIndex, mousePosition);
        UpdateButtonText("Équiper", "Déposer");
    }


    /// Utilise l'objet (l'équipe dans la main)


    public void UseItem()
    {
        if (currentSlotIndex == -2)  // Slot de main
        {
            UnequipItem();
        }
        else if (currentSlotIndex >= 0 && inventoryManager != null)
        {
            inventoryManager.UseItem(currentSlotIndex);
        }
        
        CloseMenu();
    }

    public void DropItem()
    {
        if (currentSlotIndex == -2)  // Slot de main
        {
            ThrowItem();
        }
        else if (currentSlotIndex >= 0 && inventoryManager != null)
        {
            inventoryManager.DropItem(currentSlotIndex);
        }
        
        CloseMenu();
    }


    public void CloseMenu()
    {
        gameObject.SetActive(false);
        currentSlotIndex = -1;
    }

    /// Configure le menu avec les paramètres donnés
    private void SetupMenu(int slotIndex, Vector3 mousePosition)
    {
        currentSlotIndex = slotIndex;
        GetComponent<RectTransform>().position = mousePosition;
        gameObject.SetActive(true);
    }

    /// Met à jour le texte des boutons
    private void UpdateButtonText(string useText, string dropText)
    {
        SetButtonText(useButton, useText);
        SetButtonText(DropButton, dropText);
    }

    /// Change le texte d'un bouton
    private void SetButtonText(Button button, string text)
    {
        var textComponent = button?.GetComponentInChildren<Text>();
        if (textComponent != null)
            textComponent.text = text;
    }


    public void ShowMenuForHandSlot(Vector3 mousePosition)
    {
        SetupMenu(-2, mousePosition);  // -2 pour identifier que c'est le slot de main
        UpdateButtonText("Déséquiper", "Jeter");
    }

    public void UnequipItem()
    {
        var handManager = FindAnyObjectByType<HandManager>();
        var inventoryManager = FindAnyObjectByType<InventoryManager>();

        if (handManager != null && handManager.HasItemInHand())
        {
            var currentItem = handManager.GetCurrentInventoryItem();

            // Essayer de remettre dans l'inventaire
            if (inventoryManager.AddItem(currentItem.sprite, currentItem.prefab, currentItem.size))
            {
                handManager.ClearHandItem();
            }
            else
            {
                Debug.Log("Inventaire plein - impossible de déséquiper !");
            }
        }

        CloseMenu();
    }
    
    public void ThrowItem()
{
    var handManager = FindAnyObjectByType<HandManager>();
    
    if (handManager != null && handManager.HasItemInHand())
    {
        handManager.DropItemInWorld();
    }
    
    CloseMenu();
}
}