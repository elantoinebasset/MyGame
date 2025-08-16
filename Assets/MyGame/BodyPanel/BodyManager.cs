using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BodyManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject BodyPanel;
    public Image[] BodySlots;

    [Header("Inventory Settings")]
    public int inventorySize = 4;

    public enum SlotType
    {
        Hand = 0,      //  Hand
        Head = 1,      // Head
        Body = 2,      // Chest
        Feet = 3       // Legs
    }

    [Header("References")]
    public InventoryManager inventoryManager;
    public HandManager handManager;
    public ContextMenu contextMenu;



    // Structure pour stocker les items équipés
    private InventoryManager.InventoryItem[] equippedItems;


    // Initialiser les références et trouver l'inventaire
    void Start()
    {
        equippedItems = new InventoryManager.InventoryItem[inventorySize];

        if (BodyPanel != null)
            BodyPanel.SetActive(false);

        UpdateBodyUI();

        Debug.Log($"BodyManager initialisé avec {inventorySize} slots");
    }

    public void ToggleBodyPanel()
    {
        UpdateBodyUI();
    }



    // Pour équiper un item dans un slot spécifique du corps
    public bool EquipItemToSlot(InventoryManager.InventoryItem item, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
        {
            Debug.LogWarning($"Index de slot invalide: {slotIndex}");
            return false;
        }


        // Si le slot est occupé, remet l'item dans l'inventaire
        if (equippedItems[slotIndex] != null)
        {
            ReturnItemToInventory(slotIndex);
        }


        // Équiper le nouvel item
        equippedItems[slotIndex] = item;


        // Si c'est le slot main, équipe aussi dans HandManager
        if (slotIndex == (int)SlotType.Hand && handManager != null)
        {
            handManager.EquipItem(item);
        }


        UpdateBodyUI();
        Debug.Log($"Item équipé dans le slot {slotIndex} ({(SlotType)slotIndex})");
        return true;
    }





    // Retirer un item d'un slot et le remettre dans l'inventaire
    public bool ReturnItemToInventory(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize || equippedItems[slotIndex] == null)
        {
            return false;
        }

        InventoryManager.InventoryItem itemToReturn = equippedItems[slotIndex];

        // Essayer de remettre l'item dans l'inventaire
        if (inventoryManager != null && inventoryManager.AddItem(itemToReturn.sprite, itemToReturn.prefab, itemToReturn.size))
        {
            // Si c'est le slot main, vider aussi HandManager
            if (slotIndex == (int)SlotType.Hand && handManager != null)
            {
                handManager.ClearHandItem();
            }

            equippedItems[slotIndex] = null;
            UpdateBodyUI();
            Debug.Log($"Item retiré du slot {slotIndex} et remis dans l'inventaire");
            return true;
        }
        else
        {
            Debug.LogWarning("Inventaire plein, impossible de retirer l'item");
            return false;
        }
    }



    // Échanger les items entre deux slots du corps
    public void SwapBodySlots(int fromSlot, int toSlot)
    {
        if (fromSlot < 0 || fromSlot >= inventorySize || toSlot < 0 || toSlot >= inventorySize)
        {
            Debug.LogWarning("Indices de slots invalides pour l'échange");
            return;
        }

        InventoryManager.InventoryItem tempItem = equippedItems[fromSlot];
        equippedItems[fromSlot] = equippedItems[toSlot];
        equippedItems[toSlot] = tempItem;

        // Mettre à jour HandManager si le slot main est impliqué
        if (fromSlot == (int)SlotType.Hand || toSlot == (int)SlotType.Hand)
        {
            UpdateHandManagerItem();
        }

        UpdateBodyUI();
        Debug.Log($"Items échangés entre slots {fromSlot} et {toSlot}");
    }




    // Transférer un item de l'inventaire vers un slot du corps
    public bool TransferFromInventoryToBody(int inventorySlot, int bodySlot)
    {
        if (inventoryManager == null || bodySlot < 0 || bodySlot >= inventorySize)
            return false;

        InventoryManager.InventoryItem item = inventoryManager.GetItemAtSlot(inventorySlot);
        if (item == null)
            return false;



        // Vérifier si le slot du corps est libre ou échanger si occupé
        if (equippedItems[bodySlot] != null)
        {
            // Échanger les items
            InventoryManager.InventoryItem bodyItem = equippedItems[bodySlot];

            // Retirer l'item de l'inventaire
            inventoryManager.RemoveItem(item);

            // Placer l'item du corps dans l'inventaire à la place
            if (inventoryManager.AddItem(bodyItem.sprite, bodyItem.prefab, bodyItem.size))
            {
                equippedItems[bodySlot] = item;

                if (bodySlot == (int)SlotType.Hand && handManager != null)
                {
                    handManager.EquipItem(item);
                }

                UpdateBodyUI();
                return true;
            }
            else
            {
                // Si on ne peut pas placer l'item du corps dans l'inventaire, annuler
                inventoryManager.AddItem(item.sprite, item.prefab, item.size);
                return false;
            }
        }
        else
        {
            // Slot libre, simplement transférer
            inventoryManager.RemoveItem(item);
            return EquipItemToSlot(item, bodySlot);
        }
    }



    // Mettre à jour l'item dans HandManager
    private void UpdateHandManagerItem()
    {
        if (handManager == null) return;

        if (equippedItems[(int)SlotType.Hand] != null)
        {
            handManager.EquipItem(equippedItems[(int)SlotType.Hand]);
        }
        else
        {
            handManager.ClearHandItem();
        }
    }



    // Obtenir l'item équipé dans un slot spécifique
    public InventoryManager.InventoryItem GetEquippedItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySize)
        {
            return equippedItems[slotIndex];
        }
        return null;
    }



    // Vérifier si un slot est libre
    public bool IsSlotFree(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < inventorySize && equippedItems[slotIndex] == null;
    }



    // Obtenir le type de slot
    public SlotType GetSlotType(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySize)
        {
            return (SlotType)slotIndex;
        }
        return SlotType.Hand; // Par défaut
    }



    // Mettre à jour l'interface utilisateur du corps
    private void UpdateBodyUI()
    {
        if (BodySlots == null) return;

        for (int i = 0; i < BodySlots.Length && i < inventorySize; i++)
        {
            if (BodySlots[i] != null)
            {
                if (equippedItems[i] != null)
                {
                    BodySlots[i].sprite = equippedItems[i].sprite;
                    BodySlots[i].color = Color.white;
                }
                else
                {
                    BodySlots[i].sprite = null;
                    BodySlots[i].color = Color.gray;
                }
            }
        }
    }



    // Méthode pour déposer un item du corps dans le monde
    public void DropBodyItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize || equippedItems[slotIndex] == null)
            return;

        InventoryManager.InventoryItem itemToDrop = equippedItems[slotIndex];

        if (itemToDrop.prefab != null)
        {
            Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            Quaternion spawnRot = Camera.main.transform.rotation;

            GameObject droppedObject = Instantiate(itemToDrop.prefab, spawnPos, spawnRot);
            droppedObject.SetActive(true);

            IDropable dropableItem = droppedObject.GetComponent<IDropable>();
            if (dropableItem != null)
            {
                dropableItem.DropItem();
            }

            // Si c'est le slot main, vider HandManager
            if (slotIndex == (int)SlotType.Hand && handManager != null)
            {
                handManager.ClearHandItem();
            }

            equippedItems[slotIndex] = null;
            UpdateBodyUI();

            Debug.Log($"Item déposé depuis le slot {slotIndex}");
        }
    }



    // Vider un slot du corps sans remettre dans l'inventaire
    public void ClearBodySlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySize && equippedItems[slotIndex] != null)
        {

            if (slotIndex == (int)SlotType.Hand && handManager != null)
            {
                handManager.ClearHandItem();
            }

            equippedItems[slotIndex] = null;
            UpdateBodyUI();
            Debug.Log($"Slot corps {slotIndex} vidé");
        }
    }


    // Afficher le menu contextuel pour un slot du corps
    public void ShowContextMenu(int slotIndex, Vector3 mousePosition)
    {
        if (slotIndex >= 0 && slotIndex < inventorySize && equippedItems[slotIndex] != null && contextMenu != null)
        {
            contextMenu.ShowMenuForBodySlot(slotIndex, mousePosition);
        }
    }
}