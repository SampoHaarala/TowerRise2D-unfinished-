using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

public class EventSystem : MonoBehaviour
{
    public static Save currentSave;
    public static EventSystem current = null;
    public static int currentDaveInteractionCount = 0;
    public Sprite inventoryBaseImage;
    public static Image baseInventoryImage;
    public Event dodgeAction;

    public static Dictionary<int, float> statusEffectChances = new Dictionary<int, float>()
    {
        {0, 0f }, //slow
        {1, 0f }
    };
    public static Dictionary<int, bool> statusEffectsBools = new Dictionary<int, bool>()
    {
        {0, false },
        {1,false }
    };
    public static Dictionary<int, float> statusEffectDurations = new Dictionary<int, float>
    {
        {0, 0f},
        {1, 0f}
    };

    // Start is called before the first frame update
    private void Awake()
    {
        baseInventoryImage = gameObject.AddComponent<Image>();

        if (inventoryBaseImage)
            baseInventoryImage.sprite = inventoryBaseImage;

        if (current)
        {
            Destroy(this);
        }
        else
        {
            current = this;
            DontDestroyOnLoad(this.gameObject);
        }
        if (currentSave == null)
        {
            Debug.Log("Game is missing current save so a new save has been created.");
            currentSave = new Save();
        }
        SceneManager.sceneUnloaded += SaveOnSceneUnload;
    }
    public static Dictionary<int, float> GetStatusEffectChances()
    {
        return statusEffectChances;
    }
    public static Dictionary<int, float> GetStatusEffectDurations()
    {
        return statusEffectDurations;
    }
    public static Dictionary<int, bool> GetStatusEffectBools()
    {
        return statusEffectsBools;
    }
    public int GetCurrentDaveActionCount()
    {
        return currentDaveInteractionCount;
    }
    public int AddCurrentDaveInteractionCount()
    {
        return currentDaveInteractionCount += 1;
    }
    public event Action attackReset;
    public void AttackReset()
    {
        if (attackReset != null)
        {
            attackReset();
        }
    }

    public event Action onDaveEvent00;
    public void DaveEvent00()
    {
        if (onDaveEvent00 != null)
        {
            onDaveEvent00();
        }
    }
    public event Action onDaveEvent01;
    public void DaveEvent01()
    {
        if (onDaveEvent01 != null)
        {
            onDaveEvent01();
        }
    }
    public event Action onPlayerAttack;
    public void PlayerAttack()
    {
        if (onPlayerAttack != null)
        {
            Debug.Log("PlayerAttacking.");
            onPlayerAttack();
        }
    }
    public event Action onPlayerAttackEnd;
    public void PlayerAttackEnd()
    {
        if (onPlayerAttackEnd != null)
        {
            onPlayerAttackEnd();
        }
    }
    public event Action onPlayerHit;
    public void PlayerHit()
    {
        if (onPlayerHit != null)
        {
            onPlayerHit();
        }
    }

    public event Action onPlayerHitted;
    public void PlayerHitted()
    {
        if (onPlayerHitted != null)
        {
            onPlayerHitted();
        }
    }

    public event Action onAllRoomsSpawned;
    public void AllRoomsSpawned()
    {
        if (onAllRoomsSpawned != null)
        {
            onAllRoomsSpawned();
        }
    }

    public void SaveInteractionCounts()
    {
        currentSave.savedDaveInteractionCount = currentDaveInteractionCount;
    }
    void SaveOnSceneUnload(Scene lastScene)
    {
        if (lastScene.IsValid())
        {
            SaveInteractionCounts();
            int id = currentSave.saveId;
            if (File.Exists(Application.persistentDataPath + "/gamesave" + id.ToString() + ".save"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(Application.persistentDataPath + "/gamesave" + id.ToString() + ".save");
                bf.Serialize(file, currentSave);
                file.Close();
                Debug.Log("Interaction counts saved to /gamesave" + id.ToString() + ".save");
            }
        }
    }
    void LoadOnSceneLoad(Scene currentScene, LoadSceneMode loadSceneMode)
    {
        int id = currentSave.saveId;
        if (File.Exists(Application.persistentDataPath + "/gamesave" + id.ToString() + ".save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave" + id.ToString() + ".save", FileMode.Open);
            Save save = (Save)bf.Deserialize(file);
            file.Close();
            currentDaveInteractionCount = save.savedDaveInteractionCount;
        }
    }
}
