using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public ItemBase item;
    public bool isEquiped;
    public string type;

    public InventoryScript inventoryScript;
    public Image image;

    public Dictionary<int, List<int>> slotPosition = new Dictionary<int, List<int>>();

    void Start()
    {
        Physics2D.queriesHitTriggers = true;
        image = gameObject.GetComponent<Image>();
    }

    public void Select()
    {
        if (isEquiped)
            image.color = Color.gray;
        inventoryScript.Select(this);
    }

    public void UnSelect()
    {
        image.color = Color.white;
        inventoryScript.UnSelect(this);
    }

    public void HoverOver()
    {
        image.color = Color.gray;
        // ShowInfo();
    }

    public void UnHover()
    {
        image.color = Color.white;
    }

    public void AssignToSlot(ItemBase item)
    {
        if (item != null)
        {
            this.item = item;

            item.image = image;
        }
    }

    public void UnAssignSlot()
    {
        item = null;
        if (EventSystem.baseInventoryImage)
            image = EventSystem.baseInventoryImage;

        int i = 0;
        InventorySlot[] slots = inventoryScript.inventoryItemSlots;
        foreach (InventorySlot slot in slots)
        {
            Debug.Log("Checking slot: " + i);
            if (slots[i] == this && i != slots.Length - 1)
            {
                Debug.Log("Found match.");
                if (slots[i + 1].item != null)
                {
                    InventorySlot nextSlot = slots[i + 1];

                    item = nextSlot.item;
                    nextSlot.UnAssignSlot();
                }
                return;
            }
            i++;
        }
    }

    public void ResetItemData()
    {
        item = null;
        type = null;
        image = EventSystem.baseInventoryImage;
    }
}
