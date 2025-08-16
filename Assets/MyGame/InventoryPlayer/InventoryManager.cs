using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections.Generic;

[System.Serializable]
public struct ItemSize
{
    public int width;
    public int height;

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

    [Header("Inventory Settings")]
    public int gridWidth = 4;
    public int gridHeight = 2;
    public InventoryItem GetItemAtSlot(int slotIndex)
    {
        int x = slotIndex % gridWidth;
        int y = slotIndex / gridWidth;
        return grid[x, y];
    }



    public KeyCode inventoryKey = KeyCode.I;

    [Header("BodyReferences")]
    public BodyManager bodyManager;

    [Header("Hand Manager")]
    public HandManager handManager;

    [Header("Camera Reference")]
    public CinemachineCamera cinemachineCamera;



    // Structure pour stocker les informations d'un item dans la grille
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

    // LES VARIABLES DE LA GRILLE !!!!!!!
    private InventoryItem[,] grid;
    public List<InventoryItem> items;
    public bool inventoryOpen = false;

    void Start()
    {
        grid = new InventoryItem[gridWidth, gridHeight];
        items = new List<InventoryItem>();

        inventoryPanel.SetActive(false);

        Debug.Log($"Inventaire initialisé avec une grille {gridWidth}x{gridHeight}");
    }




    void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }
    }




    public void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;
        inventoryPanel.SetActive(inventoryOpen);
        bodyManager.BodyPanel.SetActive(inventoryOpen);

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





    public bool AddItem(Sprite itemSprite, GameObject itemObject = null, ItemSize itemSize = default)
    {
        if (itemSize.width == 0 || itemSize.height == 0)
        {
            itemSize = new ItemSize(1, 1);
        }

        Debug.Log($"Tentative d'ajout d'un objet {itemSize.width}x{itemSize.height}");

        // Cherche une position libre pour l'objet
        for (int y = 0; y <= gridHeight - itemSize.height; y++)
        {
            for (int x = 0; x <= gridWidth - itemSize.width; x++)
            {
                if (CanPlaceItemAt(x, y, itemSize))
                {
                    PlaceItemAt(x, y, itemSprite, itemObject, itemSize);
                    Debug.Log($"Objet placé à la position ({x}, {y})");
                    UpdateUI();
                    return true;
                }
            }
        }

        Debug.Log($"Pas assez de place pour l'objet {itemSize.width}x{itemSize.height} !");
        return false;
    }




    // Vérifier si on peut placer un objet à une position
    private bool CanPlaceItemAt(int startX, int startY, ItemSize size)
    {
        // Vérifier si toutes les cases nécessaires sont libres
        for (int y = startY; y < startY + size.height; y++)
        {
            for (int x = startX; x < startX + size.width; x++)
            {
                if (x >= gridWidth || y >= gridHeight || grid[x, y] != null)
                {
                    return false;
                }
            }
        }
        return true;
    }




    // Placer un objet à une position
    private void PlaceItemAt(int startX, int startY, Sprite sprite, GameObject gameObj, ItemSize size)
    {
        InventoryItem newItem = new InventoryItem(sprite, gameObj, size, startX, startY);

        // Marquer toutes les cases occupées par cet objet
        for (int y = startY; y < startY + size.height; y++)
        {
            for (int x = startX; x < startX + size.width; x++)
            {
                grid[x, y] = newItem;
            }
        }

        items.Add(newItem);
    }





    public void RemoveItem(InventoryItem item)
    {
        for (int y = item.startY; y < item.startY + item.size.height; y++)
        {
            for (int x = item.startX; x < item.startX + item.size.width; x++)
            {
                if (x < gridWidth && y < gridHeight)
                    grid[x, y] = null;
            }
        }

        items.Remove(item);
        UpdateUI();
    }



    public void ShowContextMenu(int slotIndex, Vector3 mousePosition)
    {
        int x = slotIndex % gridWidth;
        int y = slotIndex / gridWidth;

        if (x < gridWidth && y < gridHeight && grid[x, y] != null)
        {
            contextMenu.ShowMenu(slotIndex, mousePosition);
        }
    }



    public void UseItem(int slotIndex)
    {
        int x = slotIndex % gridWidth;
        int y = slotIndex / gridWidth;

        if (x < gridWidth && y < gridHeight)
        {
            InventoryItem item = grid[x, y];
            if (item != null && item.prefab != null)
            {
                if (handManager != null)
                {
                    handManager.EquipItem(item);
                    RemoveItem(item);
                    Destroy(item.prefab);


                    if (inventoryOpen)
                    {
                        ToggleInventory();
                    }
                }
                else
                    {
                        Debug.LogWarning("HandManager is not assigned!");
                    }
            }
        }
    }

    public void DropItem(int slotIndex)
    {
        int x = slotIndex % gridWidth;
        int y = slotIndex / gridWidth;

        if (x < gridWidth && y < gridHeight)
        {
            InventoryItem item = grid[x, y];
            if (item != null && item.prefab != null)
            {
                Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
                Quaternion spawnRot = Camera.main.transform.rotation;

                GameObject droppedObject = Instantiate(item.prefab, spawnPos, spawnRot);
                droppedObject.SetActive(true);
                IDropable dropableItem = droppedObject.GetComponent<IDropable>();
                if (dropableItem != null)
                {
                    dropableItem.DropItem();
                }
                RemoveItem(item);
                Destroy(item.prefab);
            }
            else
            {
                Debug.LogWarning("Aucun objet à déposer dans ce slot ou prefab manquant.");
            }
        }
    }


    public void SwapItems(int fromSlot, int toSlot)
    {
        Debug.Log($"Tentative de déplacement du slot {fromSlot} vers le slot {toSlot}");

        // Convertir les indices de slots en coordonnées de grille
        int fromX = fromSlot % gridWidth;
        int fromY = fromSlot / gridWidth;
        int toX = toSlot % gridWidth;
        int toY = toSlot / gridWidth;

        // Vérifier si les coordonnées sont valides
        if (fromX >= gridWidth || fromY >= gridHeight || toX >= gridWidth || toY >= gridHeight)
        {
            Debug.Log(" Coordonnées invalides pour le déplacement");
            return;
        }



        // Récupérer l'objet à déplacer
        InventoryItem itemToMove = grid[fromX, fromY];



        // Vérifier s'il y a un objet à déplacer
        if (itemToMove == null)
        {
            Debug.Log(" Aucun objet à déplacer dans le slot source");
            return;
        }



        // Vérifier si on essaie de déplacer vers une case occupée par le même objet
        InventoryItem targetItem = grid[toX, toY];
        if (targetItem == itemToMove)
        {
            Debug.Log(" Impossible de déplacer un objet sur lui-même");
            return;
        }



        // Calculer la nouvelle position de départ pour l'objet
        int newStartX = toX;
        int newStartY = toY;



        // Si on clique sur une case occupée par un autre objet, essayer de placer à côté
        if (targetItem != null && targetItem != itemToMove)
        {
            Debug.Log(" Case occupée, recherche d'une position libre proche...");
            bool foundPosition = false;



            // Chercher une position libre autour de la position cible
            for (int offsetY = 0; offsetY <= 1 && !foundPosition; offsetY++)
            {
                for (int offsetX = 0; offsetX <= 1 && !foundPosition; offsetX++)
                {
                    int testX = toX + offsetX;
                    int testY = toY + offsetY;

                    if (CanPlaceItemAtForMove(testX, testY, itemToMove.size, itemToMove))
                    {
                        newStartX = testX;
                        newStartY = testY;
                        foundPosition = true;
                        Debug.Log($" Position libre trouvée : ({newStartX}, {newStartY})");
                    }
                }
            }



            if (!foundPosition)
            {
                Debug.Log(" Pas assez d'espace pour déplacer cet objet ici !");
                return;
            }
        }
        else
        {
            // Vérifier si l'objet peut être placé à la nouvelle position
            if (!CanPlaceItemAtForMove(newStartX, newStartY, itemToMove.size, itemToMove))
            {
                Debug.Log($" Pas assez d'espace pour placer l'objet {itemToMove.size.width}x{itemToMove.size.height} à la position ({newStartX}, {newStartY}) !");
                return;
            }
        }

        // Déplacer l'objet
        MoveItem(itemToMove, newStartX, newStartY);
        Debug.Log($"Objet {itemToMove.size.width}x{itemToMove.size.height} déplacé avec succès vers ({newStartX}, {newStartY})");
    }




    private bool CanPlaceItemAtForMove(int startX, int startY, ItemSize size, InventoryItem itemToIgnore)
    {
        // Vérifier si la position est dans les limites
        if (startX + size.width > gridWidth || startY + size.height > gridHeight)
        {
            return false;
        }

        // Vérifier si toutes les cases nécessaires sont libres (en ignorant l'objet qu'on déplace)
        for (int y = startY; y < startY + size.height; y++)
        {
            for (int x = startX; x < startX + size.width; x++)
            {
                InventoryItem occup = grid[x, y];
                if (occup != null && occup != itemToIgnore)
                {
                    return false;
                }
            }
        }
        return true;
    }




    private void MoveItem(InventoryItem item, int newStartX, int newStartY)
    {
        // Libére l'ancienne position
        for (int y = item.startY; y < item.startY + item.size.height; y++)
        {
            for (int x = item.startX; x < item.startX + item.size.width; x++)
            {
                if (x < gridWidth && y < gridHeight)
                    grid[x, y] = null;
            }
        }

        // Met à jour les coordonnées de l'objet
        item.startX = newStartX;
        item.startY = newStartY;

        // Occuper la nouvelle position
        for (int y = newStartY; y < newStartY + item.size.height; y++)
        {
            for (int x = newStartX; x < newStartX + item.size.width; x++)
            {
                grid[x, y] = item;
            }
        }

        UpdateUI();
    }



    void UpdateUI()
    {
        // Reset tous les slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
            {
                inventorySlots[i].sprite = null;
                inventorySlots[i].color = Color.gray;

                
                RectTransform rect = inventorySlots[i].GetComponent<RectTransform>();
                if (rect != null)
                {
                    
                    rect.sizeDelta = new Vector2(80, 80);
                }
            }
        }

        // Afficher les objets avec leurs tailles
        foreach (InventoryItem item in items)
        {
            int slotIndex = item.startY * gridWidth + item.startX;
            if (slotIndex < inventorySlots.Length && inventorySlots[slotIndex] != null)
            {
                inventorySlots[slotIndex].sprite = item.sprite;
                inventorySlots[slotIndex].color = Color.white;

                // Ajuster la taille visuelle selon la taille de l'objet
                if (item.size.width > 1 || item.size.height > 1)
                {
                    RectTransform rect = inventorySlots[slotIndex].GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        float slotSize = 80f; // Taille de base d'un slot
                        float spacing = 5f;   // Espacement entre les slots

                        float newWidth = slotSize * item.size.width + spacing * (item.size.width - 1);
                        float newHeight = slotSize * item.size.height + spacing * (item.size.height - 1);

                        rect.sizeDelta = new Vector2(newWidth, newHeight);

                        Debug.Log($"Objet {item.size.width}x{item.size.height} affiché avec taille {newWidth}x{newHeight}");
                    }
                }
            }
        }

        // Marquer les slots occupés par des objets plus grands
        foreach (InventoryItem item in items)
        {
            for (int y = item.startY; y < item.startY + item.size.height; y++)
            {
                for (int x = item.startX; x < item.startX + item.size.width; x++)
                {
                    if (x != item.startX || y != item.startY)
                    {
                        int slotIndex = y * gridWidth + x;
                        if (slotIndex < inventorySlots.Length && inventorySlots[slotIndex] != null)
                        {
                            inventorySlots[slotIndex].color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                        }
                    }
                }
            }
        }
    }
}