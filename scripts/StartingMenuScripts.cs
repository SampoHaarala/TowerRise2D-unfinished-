using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public class StartingMenuScripts : MonoBehaviour
{
    public Button playButton;
    public Button createANewCharacter;

    public GameObject characterSelectionScreen;
    public GameObject characterCreationScreen;
    private GameObject raceSelection;
    private GameObject characterCustomization;
    private GameObject personalityQuiz;

    private Text raceName;
    private Text raceInfo;
    private Image raceImage;

    private Text questionText;
    private Button positiveAnswerButton;
    private Button negativeAnswerButton;
    private int questionNumber = 0;
    private List<int> questionList = new List<int>(0);

    private GameObject personalityResult;
    private Text characterCustomationText;
    private Button saveButton;

    private string race = "human";
    private int baseMaxHealth = 100;
    private int vigor = 10;
    private int endurance = 10;
    private int strenght = 10;
    private int dexterity = 10;
    private int intelligence = 10;
    private int magic = 10;
    private int spirit = 10;

    // Add race texts and perks here
    Dictionary<string, List<string>> raceDict = new Dictionary<string, List<string>>()
    {
        {"human", new List<string>()
        {
            "Human", "Human text", "humanPerk1;humanPerk2;humanPerk3" // names of the perk classes
        } },
        {"highHuman", new List<string>()
        {
            "High Human", "High Human text", "highHumanPerk1;highHumanPerk2;highHumanPerk3" // names of the perk classes
        } }
    };
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(questionList);
        Button pB = playButton.GetComponent<Button>();
        pB.onClick.AddListener(PlayButtonTask);
        Button createANewCha = createANewCharacter.GetComponent<Button>();
        createANewCha.onClick.AddListener(CreateANewCharacterButtonTask);

        List<Transform> characterCreationChildren = new List<Transform>();
        for (int childIndex = 0; childIndex < characterCreationScreen.transform.childCount; ++childIndex)
        {
            characterCreationChildren.Add(characterCreationScreen.transform.GetChild(childIndex));
        }
        foreach (Transform child in characterCreationChildren)
        {
            if (child.name == "raceSelection") raceSelection = child.gameObject;
            if (child.name == "characterCustomation") characterCustomization = child.gameObject;
            if (child.name == "personalityQuiz") personalityQuiz = child.gameObject;
        }
        // Race selection
        // Race buttons
        Button[] raceSelectionButtonList = raceSelection.GetComponentsInChildren<Button>();
        foreach (Button child in raceSelectionButtonList)
        {
            if (child.name == "Human") child.gameObject.GetComponent<Button>().onClick.AddListener(HighlightHumanButtonTask);
            if (child.name == "High Human") child.GetComponent<Button>().onClick.AddListener(HighlightHighHumanButtonTask);
            if (child.name == "chooseSelectedRace") child.GetComponent<Button>().onClick.AddListener(ChooseSelectedRaceButtonTask);
        }

        // Race Texts
        Text[] raceSelectionTextList = raceSelection.GetComponentsInChildren<Text>();
        foreach (Text child in raceSelectionTextList)
        {
            if (child.name == "raceName") raceName = child;
            if (child.name == "raceInfo") raceInfo = child;
        }

        raceName.text = "Human";
        raceInfo.text = "Human info";
        raceName.enabled = true;
        raceInfo.enabled = true;

        // Personality Quiz
        // Personality Buttons
        Button[] personalityQuizButtonList = personalityQuiz.GetComponentsInChildren<Button>();
        foreach (Button child in personalityQuizButtonList)
        {
            if (child.name == "positiveAnswerButton")
            {
                positiveAnswerButton = child;
                positiveAnswerButton.onClick.AddListener(PositiveAnswerTask);
                positiveAnswerButton.gameObject.GetComponentInChildren<Text>().text = "with friends, better ones and worse ones.";
            }
            if (child.name == "negativeAnswerButton")
            {
                negativeAnswerButton = child;
                negativeAnswerButton.onClick.AddListener(NegativeAnswerTask);
                negativeAnswerButton.gameObject.GetComponentInChildren<Text>().text = "in a safe distance away from people, alone.";
            }
        }
        Text[] personalityQuizTextList = personalityQuiz.GetComponentsInChildren<Text>();
        foreach (Text child in personalityQuizTextList)
        {
            if (child.name == "questionText")
            {
                questionText = child;
                questionText.text = "You enjoy being:";
                break;
            }
        }
        // Customation
        Transform[] customationImageList = characterCustomization.GetComponentsInChildren<Transform>();
        foreach (Transform child in customationImageList)
        {
            if (child.name == "personalityResult") // Personality quiz result screen
            {
                personalityResult = child.gameObject;
                Button personalityResultButton = personalityResult.GetComponentInChildren<Button>();
                personalityResultButton.onClick.AddListener(ConfirmPersonalityResultTask);
            }
            if (child.name == "characterCustomationText") characterCustomationText = child.GetComponent<Text>();
            if (child.name == "saveButton")
            {
                saveButton = child.GetComponent<Button>();
                saveButton.onClick.AddListener(SaveNewCharacter);
            }
        }

        this.enabled = true;
        characterSelectionScreen.SetActive(false);
        characterCreationScreen.SetActive(false);
    }
    void PlayButtonTask()
    {
        gameObject.SetActive(false);
        characterSelectionScreen.SetActive(true);
    }
    void CreateANewCharacterButtonTask()
    {
        characterSelectionScreen.SetActive(false);
        characterCreationScreen.SetActive(true);
        raceSelection.SetActive(true);
    }
    void selectRace(string racename)
    {
        raceName.text = raceDict[racename][0];
        raceInfo.text = raceDict[racename][1];
        raceName.enabled = true;
        raceInfo.enabled = true;
        // raceImage.sprite = humanImage
        race = racename;
    }
    void HighlightHumanButtonTask()
    {
        selectRace("human");
    }
    void HighlightHighHumanButtonTask()
    {
        selectRace("highHuman");
    }
    void ChooseSelectedRaceButtonTask()
    {
        raceSelection.SetActive(false);
        personalityQuiz.SetActive(true);
    }
    string[] questions = { "You pay attention to:", "When you make decisions, you:", "When it comes to new situations, you:", };
    string[] answers = {"Every little detail.", "The bigger picture since I can´t see the details without moving.", "Use logical reasoning.",
                        "Think of its affect to others and do what you feel is right.", "Like to take time to make plans and know what you're getting into.",
                        "Improvise, adapt, overcome."};
    void PositiveAnswerTask()
    {
        if (questionNumber == 0)
        {
            questionList.Add(1);
            questionNumber += 1;
            questionText.text = questions[0];
            positiveAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[0];
            negativeAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[1];
        }
        else if (questionNumber == 1)
        {
            questionList.Add(1);
            questionNumber++;
            questionText.text = questions[1];
            positiveAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[2];
            negativeAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[3];
        }
        else if (questionNumber == 2)
        {
            questionList.Add(1);
            questionNumber++;
            questionText.text = questions[2];
            positiveAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[4];
            negativeAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[5];
        }
        else if (questionNumber == 3)
        {
            questionList.Add(1);
            questionNumber++;
            PersonalityResult();
        }
    }
    void NegativeAnswerTask()
    {
        if (questionNumber == 0)
        {
            questionList.Add(0);
            questionNumber++;
            questionText.text = questions[0];
            positiveAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[0];
            negativeAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[1];
        }
        else if (questionNumber == 1)
        {
            questionList.Add(0);
            questionNumber++;
            questionText.text = questions[1];
            positiveAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[2];
            negativeAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[3];
        }
        else if (questionNumber == 2)
        {
            questionList.Add(0);
            questionNumber++;
            questionText.text = questions[2];
            positiveAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[4];
            negativeAnswerButton.gameObject.GetComponentInChildren<Text>().text = answers[5];
        }
        else if (questionNumber == 3)
        {
            questionList.Add(0);
            questionNumber++;
            PersonalityResult();
        }
    }
    void PersonalityResult()
    {
        personalityQuiz.SetActive(false);
        characterCustomization.SetActive(true);
        personalityResult.SetActive(true);
        characterCustomationText.enabled = true;
        saveButton.gameObject.SetActive(false);

        Text[] personalityTextList = personalityResult.GetComponentsInChildren<Text>();
        List<string> personalityType = DeterminePersonalityType(questionList);
        foreach (Text child in personalityTextList)
        {
            if (child.name == "resultText") child.text = string.Format("Your personality type is:\n {0}", personalityType[0]);
            if (child.name == "personalityInfoText") child.text = GetPersonalityInfo(personalityType);
            if (child.name == "perksText") child.text = GetPersonalityPerks(personalityType);
        }
    }
    void ConfirmPersonalityResultTask()
    {
        personalityResult.SetActive(false);
        characterCustomationText.enabled = true;
        saveButton.gameObject.SetActive(true);
    }
    string GetPersonalityInfo(List<string> personalityType)
    {
        switch (personalityType[1])
        {
            case "ESTJ":
                return "";
            case "ESTP":
                return "";
            case "ESFJ":
                return "";
            case "ESFP":
                return "";
            case "ENTJ":
                return "";
            case "ENTP":
                return "";
            case "ENFJ":
                return "";
            case "ENFP":
                return "";
            case "ISTJ":
                return "";
            case "ISTP":
                return "";
            case "ISFJ":
                return "";
            case "ISFP":
                return "";
            case "INFJ":
                return "";
            case "INFP":
                return "";
            case "INTJ":
                return "";
            case "INTP":
                return "";
            default:
                return string.Format("no personality info available for {0}", personalityType[1]);
        }
    }
    string GetPersonalityPerks(List<string> personalityType)
    {
        switch (personalityType[1])
        {
            case "ESTJ":
                return "";
            case "ESTP":
                return "";
            case "ESFJ":
                return "";
            case "ESFP":
                return "";
            case "ENTJ":
                return "";
            case "ENTP":
                return "";
            case "ENFJ":
                return "";
            case "ENFP":
                return "";
            case "ISTJ":
                return "";
            case "ISTP":
                return "";
            case "ISFJ":
                return "";
            case "ISFP":
                return "";
            case "INFJ":
                return "";
            case "INFP":
                return "";
            case "INTJ":
                return "";
            case "INTP":
                return "";
            default:
                return string.Format("no personality info available for {0}", personalityType[1]);
        }
    }
    void DeterminePersonalityPerks(List<string> personalityType)
    {
        //TO-DO DeterminePersonalityPerks
    }
    List<string> DeterminePersonalityType(List<int> personalityList)
    {
        List<string> personalityType = new List<string>();
        if (personalityList[0] == 1) //is E?
        {
            if (personalityList[1] == 1) //is S?
            {
                if (personalityList[2] == 1) // is T?
                {
                    if (personalityList[3] == 1) // is J?
                    {
                        personalityType.Add("Executive");
                        personalityType.Add("ESTJ");
                        return personalityType;
                    }
                    else
                    {
                        personalityType.Add("Trader");
                        personalityType.Add("ESTP");
                        return personalityType;
                    }
                }
                else
                {
                    if (personalityList[3] == 1) // is J?
                    {
                        personalityType.Add("Consult");
                        personalityType.Add("ESFJ");
                        return personalityType;
                    }
                    else
                    {
                        personalityType.Add("Entertainer");
                        personalityType.Add("ESFP");
                        return personalityType;
                    }
                }
            }
            else
            {
                if (personalityList[2] == 1)
                {
                    if (personalityList[3] == 1)
                    {
                        personalityType.Add("Commander");
                        personalityType.Add("ENTJ");
                        return personalityType;
                    }
                    else
                    {
                        personalityType.Add("Debater");
                        personalityType.Add("ENTP");
                        return personalityType;
                    }
                }
                else
                {
                    if (personalityList[3] == 1)
                    {
                        personalityType.Add("Leader");
                        personalityType.Add("ENFJ");
                        return personalityType;
                    }
                    else
                    {
                        personalityType.Add("Champion");
                        personalityType.Add("ENFP");
                        return personalityType;
                    }
                }
            }
        }
        else
        {
            if (personalityList[1] == 1)
            {
                if (personalityList[2] == 1)
                {
                    if (personalityList[3] == 1)
                    {
                        personalityType.Add("Inspector");
                        personalityType.Add("ISTJ");
                        return personalityType;
                    }
                    else
                    {
                        personalityType.Add("Virtuoso");
                        personalityType.Add("ISTP");
                        return personalityType;
                    }
                }
                else
                {
                    if (personalityList[3] == 1)
                    {
                        personalityType.Add("Defender");
                        personalityType.Add("ISFJ");
                        return personalityType;
                    }
                    else
                    {
                        personalityType.Add("Adventurer");
                        personalityType.Add("ISFP");
                        return personalityType;
                    }
                }
            }
            else
            {
                if (personalityList[2] == 1)
                {
                    if (personalityList[3] == 1)
                    {
                        personalityType.Add("Mastermind");
                        personalityType.Add("INTJ");
                        return personalityType;
                    }
                    else
                    {
                        personalityType.Add("Architect");
                        personalityType.Add("INTP");
                        return personalityType;
                    }
                }
                else
                {
                    if (personalityList[3] == 1)
                    {
                        personalityType.Add("Supporter");
                        personalityType.Add("INFJ");
                        return personalityType;
                    }
                    else
                    {
                        personalityType.Add("Healer");
                        personalityType.Add("INFP");
                        return personalityType;
                    }
                }
            }
        }
    }
    private Save CreateSaveGameObject()
    {
        Save save = new Save();
        List<string> personalityType = DeterminePersonalityType(questionList);
        save.personality = personalityType;
        save.race = race;
        save.vigor = vigor;
        save.endurance = endurance;
        save.strenght = strenght;
        save.dexterity = dexterity;
        save.intelligence = intelligence;
        save.magic = magic;
        save.spirit = spirit;
        return save;
    }
    private void SaveNewCharacter()
    {
        Save save = CreateSaveGameObject();
        BinaryFormatter bf = new BinaryFormatter();
        bool con = true;
        int currentCharacter = 0;
        while (con)
        {
            if (!File.Exists(Application.persistentDataPath + "/gamesave"+currentCharacter.ToString()+".save"))
            {
               FileStream file = File.Create(Application.persistentDataPath + "/gamesave"+currentCharacter.ToString()+".save");
                bf.Serialize(file, save);
                file.Close();
                Debug.Log("Game Saved to file /gamesave" + currentCharacter.ToString() + ".save");
                con = false;
            }
            else
            {
                currentCharacter += 1;
            }
        }
        //TO-DO Tästä pääsee peliin.
        EventSystem.currentSave = save;
        SceneManager.LoadScene(1);
        //TO-Do Saa customationin tabit toimii.
    }
    // Update is called once per frame
    void Update()
    {
    }
}