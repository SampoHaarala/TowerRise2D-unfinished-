using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuStatusWindow : MonoBehaviour
{
    public Text playerName;
    public Text race;
    public Text gender;

    public Text vigor;
    public Text endurance;
    public Text strenght;
    public Text dexterity;
    public Text intelligence;
    public Text magic;
    public Text spirit;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Save save = EventSystem.currentSave;

        playerName.text = save.playerName;
        race.text = save.race;
        gender.text = save.gender;

        vigor.text = save.vigor.ToString();
        endurance.text = save.endurance.ToString();
        strenght.text = save.strenght.ToString();
        dexterity.text = save.dexterity.ToString();
        intelligence.text = save.intelligence.ToString();
        magic.text = save.magic.ToString();
        spirit.text = save.spirit.ToString();
    }
}
