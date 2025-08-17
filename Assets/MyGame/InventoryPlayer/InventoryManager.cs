using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections.Generic;

[System.Serializable]
public struct ItemSize
{
    public int width, height;
    
    public ItemSize(int w, int h) 
    { 
        width = w; 
        height = h; 
    }
}

public class InventoryManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventoryPanel;
    public Image[] inventorySlots;
    public ContextMenu contextMenu;
    public HandSlot handSlot;

    [Header("Settings")]
    public int gridWidth = 4;
    public int gridHeight = 2;
    public KeyCode inventoryKey = KeyCode.I;

    [Header("References")]
    public HandManager handManager;
    public CinemachineCamera cinemachineCamera;

    [System.Serializable]
    public class InventoryItem
    {
        public Sprite sprite;         
        public GameObject prefab;     
        public ItemSize size;      
        public int startX, startY;     

        public InventoryItem(Sprite s, GameObject go, ItemSize sz, int x, int y)
        {
            sprite = s;
            prefab = go;
            size = sz;
            startX = x;
            startY = y;
        }
    }


    private InventoryItem[,] grid;
    private List<InventoryItem> items = new List<InventoryItem>(); // Liste de tous les objets
    public bool inventoryOpen = false;

    void Start()
    {
        grid = new InventoryItem[gridWidth, gridHeight];
        inventoryPanel.SetActive(false);
    }

    void Update()
    {

        if (Input.GetKeyDown(inventoryKey))
            ToggleInventory();
    }

    // === MÉTHODES PUBLIQUES PRINCIPALES ===



    public void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;
        inventoryPanel.SetActive(inventoryOpen);

        SetCursorState(inventoryOpen);
        UpdateUI();
    }

    /// Ajoute un objet dans l'inventaire à la première place libre

    public bool AddItem(Sprite itemSprite, GameObject itemObject = null, ItemSize itemSize = default)
    {
        if (itemSize.width == 0 || itemSize.height == 0)
            itemSize = new ItemSize(1, 1);

        // Chercher une place libre dans la grille
        for (int y = 0; y <= gridHeight - itemSize.height; y++)
        {
            for (int x = 0; x <= gridWidth - itemSize.width; x++)
            {
                if (CanPlaceAt(x, y, itemSize))
                {
                    PlaceItem(x, y, itemSprite, itemObject, itemSize);
                    UpdateUI();
                    return true;
                }
            }
        }
        
        Debug.Log("Inventaire plein !");
        return false;
    }

    public void RemoveItem(InventoryItem item)
    {
        ClearItemFromGrid(item);
        items.Remove(item);
        
        // Détruire l'objet GameObject s'il existe
        if (item.prefab != null)
        {
            Destroy(item.prefab);
            item.prefab = null;
        }
        
        UpdateUI();
    }


    public void SwapItems(int fromSlot, int toSlot)
    {
        var fromCoords = SlotToCoords(fromSlot);
        var toCoords = SlotToCoords(toSlot);
        
        // Vérifier que les coordonnées sont valides
        if (!IsValidCoords(fromCoords) || !IsValidCoords(toCoords)) 
            return;

        var itemToMove = grid[fromCoords.x, fromCoords.y];
        if (itemToMove == null) return; // Pas d'objet à déplacer

        var targetItem = grid[toCoords.x, toCoords.y];
        
        // Si on clique sur le même objet, ne rien faire
        if (targetItem == itemToMove) return;

        var newPos = FindBestPosition(toCoords.x, toCoords.y, itemToMove.size, itemToMove);
        if (newPos.HasValue)
            MoveItem(itemToMove, newPos.Value.x, newPos.Value.y);
    }


    /// Utilise un objet (l'équipe dans la main du joueur)

    public void UseItem(int slotIndex)
    {
        var item = GetItemAtSlot(slotIndex);
        if (item?.prefab != null && handManager != null)
        {
            handManager.EquipItem(item);
            RemoveItem(item);
            
            if (inventoryOpen) 
                ToggleInventory();
        }
    }


    public void DropItem(int slotIndex)
    {
        var item = GetItemAtSlot(slotIndex);
        if (item?.prefab != null)
        {
            // Position devant le joueur
            var spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            var dropped = Instantiate(item.prefab, spawnPos, Camera.main.transform.rotation);
            dropped.SetActive(true);
            dropped.GetComponent<IDropable>()?.DropItem();
            
            RemoveItem(item);
        }
    }


    public void ShowContextMenu(int slotIndex, Vector3 mousePosition)
    {
        var coords = SlotToCoords(slotIndex);
        if (IsValidCoords(coords) && grid[coords.x, coords.y] != null)
            contextMenu.ShowMenu(slotIndex, mousePosition);
    }


    /// Récupère l'objet à un slot donné

    public InventoryItem GetItemAtSlot(int slotIndex)
    {
        var coords = SlotToCoords(slotIndex);
        return IsValidCoords(coords) ? grid[coords.x, coords.y] : null;
    }

    // === MÉTHODES PRIVÉES - LOGIQUE INTERNE ===


    /// Vérifie si on peut placer un objet à une position donnée

    private bool CanPlaceAt(int startX, int startY, ItemSize size, InventoryItem ignore = null)
    {
        // Vérifier que l'objet ne dépasse pas des limites de la grille
        if (startX + size.width > gridWidth || startY + size.height > gridHeight) 
            return false;

        // Vérifier que tous les emplacements nécessaires sont libres
        for (int y = startY; y < startY + size.height; y++)
        {
            for (int x = startX; x < startX + size.width; x++)
            {
                if (grid[x, y] != null && grid[x, y] != ignore) 
                    return false;
            }
        }

        return true;
    }


    /// Place un objet dans la grille à la position spécifiée

    private void PlaceItem(int startX, int startY, Sprite sprite, GameObject gameObj, ItemSize size)
    {
        var newItem = new InventoryItem(sprite, gameObj, size, startX, startY);
        
        // Marquer tous les emplacements occupés par cet objet dans la grille
        for (int y = startY; y < startY + size.height; y++)
        {
            for (int x = startX; x < startX + size.width; x++)
            {
                grid[x, y] = newItem;
            }
        }

        items.Add(newItem);
    }


    /// Retire un objet de la grille (libère les emplacements)

    private void ClearItemFromGrid(InventoryItem item)
    {
        for (int y = item.startY; y < item.startY + item.size.height; y++)
        {
            for (int x = item.startX; x < item.startX + item.size.width; x++)
            {
                if (x < gridWidth && y < gridHeight) 
                    grid[x, y] = null;
            }
        }
    }

    /// Déplace un objet vers une nouvelle position dans la grille

    private void MoveItem(InventoryItem item, int newStartX, int newStartY)
    {
        // Retirer l'objet de son ancienne position
        ClearItemFromGrid(item);
        
        // Mettre à jour sa position
        item.startX = newStartX;
        item.startY = newStartY;
        
        // Le replacer dans la grille à sa nouvelle position
        for (int y = newStartY; y < newStartY + item.size.height; y++)
        {
            for (int x = newStartX; x < newStartX + item.size.width; x++)
            {
                grid[x, y] = item;
            }
        }

        UpdateUI();
    }


    /// Convertit un index de slot en coordonnées x,y dans la grille

    private (int x, int y) SlotToCoords(int slotIndex)
    {
        int x = slotIndex % gridWidth;
        int y = slotIndex / gridWidth;
        return (x, y);
    }




    private bool IsValidCoords((int x, int y) coords)
    {
        bool validX = coords.x >= 0 && coords.x < gridWidth;
        bool validY = coords.y >= 0 && coords.y < gridHeight;
        return validX && validY;
    }


    /// Trouve la meilleure position pour placer un objet près d'une position cible

    private (int x, int y)? FindBestPosition(int targetX, int targetY, ItemSize size, InventoryItem ignore)
    {
        // Essayer d'abord la position exacte demandée
        if (CanPlaceAt(targetX, targetY, size, ignore))
            return (targetX, targetY);

        // Chercher autour de la position cible avec un rayon croissant
        for (int offset = 1; offset <= 2; offset++)
        {
            for (int dy = 0; dy <= offset; dy++)
            {
                for (int dx = 0; dx <= offset; dx++)
                {
                    int testX = targetX + dx;
                    int testY = targetY + dy;
                    if (CanPlaceAt(testX, testY, size, ignore))
                        return (testX, testY);
                }
            }
        }
        
        return null; // Aucune position trouvée
    }


    /// Gère l'état du curseur et de la caméra selon si l'inventaire est ouvert

    private void SetCursorState(bool inventoryOpen)
    {
        Cursor.lockState = inventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        
        if (cinemachineCamera != null)
            cinemachineCamera.enabled = !inventoryOpen;

        if (!inventoryOpen && contextMenu.gameObject.activeSelf)
            contextMenu.CloseMenu();
    }

    /// /// Met à jour l'affichage visuel de l'inventaire

    private void UpdateUI()
    {
        // Étape 1: Réinitialiser tous les slots (les vider visuellement)
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
            {
                inventorySlots[i].sprite = null;           // Pas d'image
                inventorySlots[i].color = Color.gray;      // Couleur grise (vide)
                inventorySlots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80); // Taille normale
            }
        }

        // Étape 2: Afficher tous les objets dans leurs slots
        foreach (var item in items)
        {
            int slotIndex = item.startY * gridWidth + item.startX; // Calculer l'index du slot principal
            
            if (slotIndex < inventorySlots.Length && inventorySlots[slotIndex] != null)
            {
                var slot = inventorySlots[slotIndex];
                slot.sprite = item.sprite;    // Afficher l'image de l'objet
                slot.color = Color.white;     // Couleur normale (visible)

                // Ajuster la taille pour les objets qui occupent plusieurs cases
                if (item.size.width > 1 || item.size.height > 1)
                {
                    var rect = slot.GetComponent<RectTransform>();
                    float baseSize = 80f;   // Taille de base d'un slot
                    float spacing = 5f;     // Espacement entre les slots
                    
                    rect.sizeDelta = new Vector2(
                        baseSize * item.size.width + spacing * (item.size.width - 1),
                        baseSize * item.size.height + spacing * (item.size.height - 1)
                    );
                }
            }
        }

        // Étape 3: Griser les cases secondaires des objets multi-cases
        foreach (var item in items)
        {
            for (int y = item.startY; y < item.startY + item.size.height; y++)
            {
                for (int x = item.startX; x < item.startX + item.size.width; x++)
                {
                    // Ne pas traiter la case principale (coin supérieur gauche)
                    if (x != item.startX || y != item.startY)
                    {
                        int slotIndex = y * gridWidth + x;
                        if (slotIndex < inventorySlots.Length && inventorySlots[slotIndex] != null)
                        {
                            // Rendre la case semi-transparente pour montrer qu'elle est occupée
                            inventorySlots[slotIndex].color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                        }
                    }
                }
            }
        }
    }
}