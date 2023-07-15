using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBase : MonoBehaviour
{
    public string itemName = "name";
    public string type = "type";
    public string info = "info";
    public int itemNumber = 0;

    public Image image;
    public static ItemBase GetItem(int itemNumber, GameObject gameObject)
    {
        switch (itemNumber)
        {
            case 0: // basic one hand iron sword
                ItemBase item = gameObject.AddComponent<ItemBase>();
                item.itemName = "Cheap Iron Sword";
                item.type = "weaponry";
                item.info = "Cheap sword sold near the dungeon entrance. Beginner dungeon dweller often get fooled by their enthusiasm and buy weapons that are sold with golden words.";
                item.itemNumber = 0;
                return item;
        }
        return null;
    }

    public WeaponBase GetWeaponry(GameObject gameObject, bool isPlayer)
    {
        switch (itemNumber)
        {
            case 0:
                WeaponBase cheapIronSword = gameObject.AddComponent<WeaponBase>();
                cheapIronSword.CreateANewWeaponBase(isPlayer, "sharp", false, 10, false, "dexterity", 0.1f, 6, "strenght", 0.1f);
                return cheapIronSword;
        }
        Debug.Log("No Item found.");
        return null;
    }
}
