using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class InventoryManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventoryPanel;
    public Image[] inventorySlots; 
    public ContextMenu contextMenu;
    
    [Header("Inventory")]
    public Sprite[] items = new Sprite[8];
    public KeyCode inventoryKey = KeyCode.I; 
    
    [Header("Camera Reference")]
    public CinemachineCamera cinemachineCamera;
    
    private bool inventoryOpen = false;
    
    void Start()
    {
        inventoryPanel.SetActive(false);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }
    }
    
    void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;
        inventoryPanel.SetActive(inventoryOpen);
        
        if (inventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            if (cinemachineCamera != null)
                cinemachineCamera.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            if (cinemachineCamera != null)
                cinemachineCamera.enabled = true;
            
            
            if (contextMenu.gameObject.activeSelf)
                contextMenu.CloseMenu();
        }
        
        UpdateUI();
    }
    
    public bool AddItem(Sprite itemSprite)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = itemSprite;
                UpdateUI();
                return true;
            }
        }
        Debug.Log("Inventaire plein !");
        return false;
    }
    
    //Pour afficher le menu contextuel
    public void ShowContextMenu(int slotIndex, Vector3 mousePosition)
    {
        contextMenu.ShowMenu(slotIndex, mousePosition);
    }

    // Pour utiliser un objet (objet à coder)
    public void UseItem(int slotIndex)
    {
        Debug.Log($"Utilisation de l'objet dans le slot {slotIndex + 1}");
    }
    
    void UpdateUI()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (items[i] != null)
            {
                inventorySlots[i].sprite = items[i];
                inventorySlots[i].color = Color.white;
            }
            else
            {
                inventorySlots[i].sprite = null;
                inventorySlots[i].color = Color.gray;
            }
        }
    }
}