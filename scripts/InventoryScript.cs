using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    public static InventoryScript current;

    private InventorySlot[] equipmentItemSlots;
    public InventorySlot[] inventoryItemSlots;
    public GameObject inventory;
    public Dictionary<int, Dictionary<int, InventorySlot>> slotPosition = new Dictionary<int, Dictionary<int, InventorySlot>>();
    private InventorySlot hoveredOverSlot;
    public InventorySlot selectedEquipmentSlot;
    public InventorySlot selectedInventorySlot;
    public bool bUseGrid = false;
    private bool openInventoryOnce = true;

    // Start is called before the first frame update
    void Awake()
    {
        if (current)
            Destroy(this);
        else
            current = this;
    }

    void Start()
    {
        int i = 1;
        equipmentItemSlots = gameObject.GetComponentsInChildren<InventorySlot>();
        foreach (InventorySlot slot in equipmentItemSlots)
        {
            Debug.Log("Slot " + i + ": " + slot.gameObject.name);

            slot.isEquiped = true;
            slot.inventoryScript = this;
            i++;
        }

        inventoryItemSlots = inventory.gameObject.GetComponentsInChildren<InventorySlot>();
        foreach (InventorySlot slot in inventoryItemSlots)
        {
            slot.inventoryScript = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedEquipmentSlot != null && selectedInventorySlot != null)
        {
            Debug.Log(selectedInventorySlot.item + " has been assigned to " + selectedEquipmentSlot.name);

            selectedEquipmentSlot.AssignToSlot(selectedInventorySlot.item);
            selectedInventorySlot.UnAssignSlot();


            selectedEquipmentSlot.UnSelect();
            selectedInventorySlot.UnSelect();


            PlayerController.current.UpdateEquipment();
        }

        if (selectedEquipmentSlot)
        {
            if (openInventoryOnce)
                OpenInventory();
            openInventoryOnce = false;
        }
        else
        {
            CloseInventory();
            openInventoryOnce = true;
        }
    }

    public void Select(InventorySlot slot)
    {
        if (slot.isEquiped)
        {
            if (selectedEquipmentSlot != null)
                selectedEquipmentSlot.UnSelect();

            selectedEquipmentSlot = slot;
        }
        else
        {
            if (selectedInventorySlot != null)
                selectedInventorySlot.UnSelect();

            selectedInventorySlot = slot;
        }
    }

    public void UnSelect(InventorySlot slot)
    {
        if (selectedInventorySlot == slot)
            selectedInventorySlot = null;
        else if (selectedEquipmentSlot == slot)
            selectedEquipmentSlot = null;
    }

    private void HoverOverFromGrid(int x, int y)
    {
        if (hoveredOverSlot)
        {
            hoveredOverSlot.UnHover();
            hoveredOverSlot = slotPosition[x][y];
            hoveredOverSlot.HoverOver();
        }
    }

    public void OpenInventory()
    {
        int i = 0;
        inventory.gameObject.SetActive(true);
        List<int> items = EventSystem.currentSave.inventory;
        Debug.Log(items.Count);
        foreach (int itemNumber in items)
        {
            ItemBase item = ItemBase.GetItem(itemNumber, inventoryItemSlots[i].gameObject);
            Debug.Log("Slot " + i + ": " + item.name);
            if (item.type != selectedEquipmentSlot.type)
            {
                Destroy(item);
            }
            else
            {
                inventoryItemSlots[i].item = item;
                i++;
            }
        }
    }

    public void CloseInventory()
    {
        foreach (InventorySlot slot in inventoryItemSlots)
        {
            slot.item = null;
            slot.ResetItemData();
            Destroy(slot.gameObject.GetComponent<ItemBase>());
            slot.UnHover();
        }
        inventory.gameObject.SetActive(false);
    }
}
