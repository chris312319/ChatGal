using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;

public class FileManager : MonoBehaviour
{
    // Start is called before the first frame update
    static public string userSetting = "userSetting.json";
    static public string dialoguedata = "dialogue.json";
    static public string data = "data.json";
    static public string furniture = "furniture.json";
    static public string action = "action.json";
    static public DialogueList datalist = new DialogueList();
    static public int Amount = 20;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    static public void SaveUserSetting(UserSetting setting,bool flag)
    {
        UserSetting origin = LoadUserSetting();
        if (flag)
        {
            origin.ApiKey = setting.ApiKey;
            origin.CharacterSet = setting.CharacterSet;
        }
        else
        {
            origin = setting;
        }
        using (StreamWriter writer = new StreamWriter(Application.streamingAssetsPath + "/" + userSetting))
        {
           string json = JsonUtility.ToJson(origin);
           writer.WriteLine(json);
        }
    }
    static public UserSetting LoadUserSetting()
    {
        UserSetting set = new UserSetting();
        if (!File.Exists(Application.streamingAssetsPath + "/" + userSetting))
        {
            using (StreamWriter writer = new StreamWriter(Application.streamingAssetsPath + "/" + userSetting))
            {
                string json = JsonUtility.ToJson(set);
                writer.WriteLine(json);
            }
        }
        using (StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/" + userSetting))
        {
            string json = reader.ReadToEnd();
            set = JsonUtility.FromJson<UserSetting>(json);
        }
        return set;
    }
    static public void SaveDialogue(Dialogue dialogue)
    {
        Dialogue data = new Dialogue();
        data.data = dialogue.data;
        data.timer = dialogue.timer;
        data.favorability = dialogue.favorability;
        data.furnitures = dialogue.furnitures;
        data.itemlist = dialogue.itemlist;
        data.memorylist = dialogue.memorylist;
        using (StreamWriter writer = new StreamWriter(Application.streamingAssetsPath + "/" + dialoguedata))
        {
            string json = JsonConvert.SerializeObject(data, new VectorConverter());
            writer.WriteLine(json);
        }
    }
    static public Dialogue LoadDialogue()
    {
        Dialogue data = new Dialogue();
        for (int j = 0; j < 20; j++)
        {
            data.furnitures.Add(new Furniture());
        }
        if (File.Exists(Application.streamingAssetsPath + "/" + dialoguedata))
        {
            using (StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/" + dialoguedata))
            {
                string json = reader.ReadToEnd();
                data = JsonConvert.DeserializeObject<Dialogue>(json);
            }
        }
        return data;
    }
    static public void SaveData()
    {
        using (StreamWriter writer = new StreamWriter(Application.streamingAssetsPath + "/" + data))
        {
            string json = JsonConvert.SerializeObject(datalist,new VectorConverter());
            Debug.Log(json);
            writer.WriteLine(json);
        }
    }
    static public void SaveData(int i,Dialogue dialogue)
    {
        datalist.list[i] = dialogue;
        SaveData();
    }
    static public void LoadData()
    {
        if (!File.Exists(Application.streamingAssetsPath + "/" + data)) 
        {
            for(int i = 0; i < 6; i++)
            {
                Dialogue dialogue = new Dialogue();
                dialogue.favorability = -1;
                for(int j = 0; j < Amount; j++)
                {
                    dialogue.furnitures.Add(new Furniture());
                }
                datalist.list.Add(dialogue);
            }
            SaveData();
        }
        using (StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/" + data))
        {
            string json = reader.ReadToEnd();
            datalist = JsonConvert.DeserializeObject<DialogueList>(json);
        }
    }
    static public Dialogue LoadData(int i)
    {
        LoadData();
        Dialogue dialogue = new Dialogue();
        dialogue = datalist.list[i];
        return dialogue;
    }
    static public void SaveAction(List<ActionP> actionlist)
    {
        ActionList list = new ActionList();
        list.list = actionlist;
        using (StreamWriter writer = new StreamWriter(Application.streamingAssetsPath + "/" + action))
        {
            string json = JsonConvert.SerializeObject(list);
            writer.WriteLine(json);
        }
    }
    static public ActionList LoadAction()
    {
        ActionList list = new ActionList();
        if (!File.Exists(Application.streamingAssetsPath + "/" + action))
        {
            using (StreamWriter writer = new StreamWriter(Application.streamingAssetsPath + "/" + action))
            {
                string json = JsonConvert.SerializeObject(list);
                writer.WriteLine(json);
            }
        }
        using (StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/" + action))
        {
            string json = reader.ReadToEnd();
            list = JsonConvert.DeserializeObject<ActionList>(json);
        }
        return list;
    }
}
public class UserSetting
{
    public string GirlName = "結月ゆかり";
    public string ModelType = "gpt-3.5-turbo";
    public string StableUrl;
    public string ApiKey;
    public string CharacterSet;
    public string BehaviourSet;
    public bool isCamera = true;
    public bool isVoice = true;
    public bool isDraw;
    public bool isBehaviour = true;
    public int ModelSource;
    public int PmxSource;
    public string ModelPath = "";
    public string ConfigPath;
    public string ApiUrl;
    public string PmxPath;
    public int Language;
    public int TextLanguage;
    public string VoiceCharacter = "0";
    public float SpeakVolume = 0.9f;
    public float MusicVolume = 0.5f;
}
[SerializeField]
public class Dialogue
{
    public float timer;
    public int favorability;
    public List<SendData> data = new List<SendData>();
    public List<Furniture> furnitures = new List<Furniture>();
    public List<Item> itemlist = new List<Item>();
    public List<Memory> memorylist = new List<Memory>();
}
public class DialogueList
{
    public List<Dialogue> list = new List<Dialogue>();
}
[Serializable]
public class Furniture
{
    //public Vector3 Position;
    //public Vector3 Rotation;
    public float[] Position = new float[3];
    public float[] Rotation = new float[3];
    public bool isActive;
    public string Base64 = "";
}
[Serializable]
public class ActionP
{
    public string prompt;
    public float time;
    public string path;
}
public class ActionList
{
    public List<ActionP> list = new List<ActionP>();
}


