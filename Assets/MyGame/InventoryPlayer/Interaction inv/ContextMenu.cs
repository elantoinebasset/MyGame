// Me permet d'interagir avec les slots d'inventaire ET du corps (configurer les clics et l'apparition du menu contextuel)
using UnityEngine;
using UnityEngine.UI;

public class ContextMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button useButton;
    public Button DropButton;
    public Button cancelButton;
    
    private InventoryManager inventoryManager;
    private BodyManager bodyManager; // AJOUTÉ : Référence au BodyManager
    private int currentSlotIndex = -1;
    private bool isBodySlot = false; // AJOUTÉ : Pour savoir si on travaille avec un slot du corps
    
    void Start()
    {
        inventoryManager = FindAnyObjectByType<InventoryManager>();
        bodyManager = FindAnyObjectByType<BodyManager>(); // AJOUTÉ
        
        useButton.onClick.AddListener(UseItem);
        DropButton.onClick.AddListener(DropItem);
        cancelButton.onClick.AddListener(CloseMenu);
        
        gameObject.SetActive(false);
    }
    
    // Méthode existante pour les slots d'inventaire
    public void ShowMenu(int slotIndex, Vector3 mousePosition)
    {
        currentSlotIndex = slotIndex;
        isBodySlot = false; // C'est un slot d'inventaire
        
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = mousePosition;
        
        // Personnaliser les boutons pour l'inventaire
        UpdateButtonsForInventory();
        
        gameObject.SetActive(true);
    }
    
    // AJOUTÉ : Nouvelle méthode pour les slots du corps
    public void ShowMenuForBodySlot(int bodySlotIndex, Vector3 mousePosition)
    {
        currentSlotIndex = bodySlotIndex;
        isBodySlot = true; // C'est un slot du corps
        
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = mousePosition;
        
        // Personnaliser les boutons pour le corps
        UpdateButtonsForBody(bodySlotIndex);
        
        gameObject.SetActive(true);
    }
    
    // AJOUTÉ : Personnaliser les boutons pour l'inventaire
    private void UpdateButtonsForInventory()
    {
        if (useButton != null)
        {
            Text buttonText = useButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = "Équiper"; // Ou "Utiliser"
        }
        
        if (DropButton != null)
        {
            Text buttonText = DropButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = "Déposer";
        }
    }
    
    // AJOUTÉ : Personnaliser les boutons pour le corps
    private void UpdateButtonsForBody(int bodySlotIndex)
    {
        if (bodyManager == null) return;
        
        BodyManager.SlotType slotType = bodyManager.GetSlotType(bodySlotIndex);
        
        if (useButton != null)
        {
            Text buttonText = useButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                // Personnaliser selon le type de slot
                switch (slotType)
                {
                    case BodyManager.SlotType.Hand:
                        buttonText.text = "Utiliser";
                        break;
                    case BodyManager.SlotType.Head:
                    case BodyManager.SlotType.Body:
                    case BodyManager.SlotType.Feet:
                        buttonText.text = "Retirer";
                        break;
                    default:
                        buttonText.text = "Retirer";
                        break;
                }
            }
        }
        
        if (DropButton != null)
        {
            Text buttonText = DropButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = "Déposer";
        }
    }
    
    public void UseItem()
    {
        if (currentSlotIndex < 0) return;
        
        if (isBodySlot)
        {
            // Logique pour les slots du corps
            if (bodyManager != null)
            {
                BodyManager.SlotType slotType = bodyManager.GetSlotType(currentSlotIndex);
                
                if (slotType == BodyManager.SlotType.Hand)
                {
                    // Pour la main, utiliser l'item
                    UseHandItem();
                }
                else
                {
                    // Pour les autres slots, retirer et remettre dans l'inventaire
                    bodyManager.ReturnItemToInventory(currentSlotIndex);
                }
            }
        }
        else
        {
            // Logique existante pour l'inventaire
            if (inventoryManager != null)
            {
                inventoryManager.UseItem(currentSlotIndex);
            }
        }
        CloseMenu();
    }
    
    // AJOUTÉ : Méthode pour utiliser l'item en main
    private void UseHandItem()
    {
        if (bodyManager != null && bodyManager.handManager != null)
        {
            if (bodyManager.handManager.HasItemInHand())
            {
                var currentItem = bodyManager.handManager.GetCurrentItem();
                if (currentItem != null)
                {
                    IUsable usableItem = currentItem.GetComponent<IUsable>();
                    if (usableItem != null)
                    {
                        usableItem.UseItem();
                        Debug.Log("Item utilisé depuis le menu contextuel");
                        bodyManager.handManager.ForceRemoveHandItem();
                    }
                    else
                    {
                        Debug.Log("Cet item ne peut pas être utilisé");
                    }
                }
            }
        }
    }

    public void DropItem()
    {
        if (currentSlotIndex < 0) return;
        
        if (isBodySlot)
        {
            if (bodyManager != null)
            {
                bodyManager.DropBodyItem(currentSlotIndex);
            }
        }
        else
        {
            if (inventoryManager != null)
            {
                inventoryManager.DropItem(currentSlotIndex);
            }
        }
        CloseMenu();
    }
    
    public void CloseMenu()
    {
        Debug.Log("Menu contextuel fermé (Annuler)");
        gameObject.SetActive(false);
        
        currentSlotIndex = -1;
        isBodySlot = false;
    }
}