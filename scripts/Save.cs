using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Save
{
    public int savedDaveInteractionCount = -1;
    public int saveId = 0;
    public string parryingKey = "left alt";
    public string cancelingKey = "tab";
    public string interactKey = "r";
    public string dodgeKey = "space";

    public string playerName = "name";
    public string race = "race";
    public string gender = "gender";
    public List<string> personality;

    public int vigor = 10;
    public int endurance = 10;
    public int strenght = 10;
    public int dexterity = 10;
    public int intelligence = 10;
    public int magic = 10;
    public int spirit = 10;

    // inventory
    public List<int> inventory;
}
