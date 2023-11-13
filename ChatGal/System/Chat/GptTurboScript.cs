
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class GptTurboScript : MonoBehaviour
{
    /// <summary>
    /// api��ַ
    /// </summary>
    /// 
    [SerializeField] public PostData m_PostDataSetting;
    public BehaviourSystem behaviour;
    public FaceRecognizer face;
    public BackpackSystem backpack;
    public FurnitureSystem furniture;
    public ActionSystem action;
    public Setting setting;
    public ChatScript chat;
    public Toggle draw;
    public MemorySystem memory;
    public bool isSimplify;
    public bool isDraw;
    public bool isWait;
    public float waitTime, maxwaitTime;
    public StableDiffusion sd;
    public Moegoe moegoe;
    public int maxLength;
    public string m_ApiUrl = "https://api.openai.com/v1/chat/completions";
    /// <summary>
    /// gpt-3.5-turbo
    /// </summary>
    /// <summary>
    /// ����Ի�
    /// </summary>
    [SerializeField]public List<SendData> m_DataList = new List<SendData>();
    /// <summary>
    /// AI����
    /// </summary>
    /// 
    public string[] Facts;
    [TextArea]
    public string Character;
    [TextArea]
    public string Prompt;
    [TextArea]
    public string SimplifyPrompts;
    [TextArea]
    public string Target;
    [TextArea]
    public string SimplifyPrompt;
    public float timer;
    public GameObject SavePanel;
    public GameObject[] btns;
    public float thinkTime;
    public bool Simplify;

    public void SaveData(int i)
    {
        furniture.SavePos();
        Dialogue dialogue = new Dialogue();
        dialogue.timer = timer;
        dialogue.data = m_DataList;
        dialogue.favorability = memory.Favorability;
        dialogue.furnitures = furniture.Furnitures;
        dialogue.itemlist = backpack.itemlist;
        dialogue.memorylist = memory.MemoryList;
        FileManager.SaveData(i,dialogue);
        setting.Set();
        RefreshSaveData();
    }
    public void RefreshSaveData()
    {
        for (int j = 0; j < btns.Length; j++)
        {
            float timer = FileManager.datalist.list[j].timer;
            btns[j].GetComponentInChildren<Text>().text = (((int)timer) / 60).ToString("00") + ":" + (((int)timer) % 60).ToString("00");
        }
    }
    public void SavePanelActive(bool flag)
    {
        RefreshSaveData();
        SavePanel.SetActive(flag);
    }
    public void DrawSet()
    {
        isDraw = draw.isOn;
    }
    private void Start()
    {
        Dialogue dialogue = FileManager.LoadDialogue();
        m_DataList = dialogue.data;
        for(int i = 0; i < m_DataList.Count; i++)
        {
            if (m_DataList[i].role != "system") chat.m_ChatHistory.Add(m_DataList[i].content);
        }
        timer = dialogue.timer;
        furniture.Furnitures = dialogue.furnitures;
        backpack.itemlist = dialogue.itemlist;
        memory.MemoryList = dialogue.memorylist;
        furniture.Action();
        backpack.Create();
        backpack.Refresh();
        if(m_DataList.Count == 0 && !Simplify) m_DataList.Add(new SendData("system", string.Join("\n",Facts) + "\n"));
        if (dialogue.favorability != -1) memory.SetFavorability(dialogue.favorability);
    }
    public void Update()
    {
        timer += Time.deltaTime;
        if (isWait)
        {
            waitTime += Time.deltaTime;
        }
        if (waitTime > maxwaitTime)
        {
            Debug.Log("超時");
            chat.ResetSpeak(true);
            waitTime = 0;
            isWait = false;
            behaviour.isSend = false;
        }
    }
    /// <summary>
    /// </summary>
    /// <param name="_postWord"></param>
    /// <param name="_openAI_Key"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    public IEnumerator GetPostData(string _postWord,string _openAI_Key, System.Action<ResponseContent> _callback)
    {
        setting.model.SetBool("ToIdle", false);
        setting.model.Play((int)BehaviourType.Think);
        isWait = true;
        waitTime = 0;
        Debug.Log(_postWord);
        m_DataList.Add(new SendData("user", _postWord));
        chat.m_ChatHistory.Add(_postWord);       
        using (UnityWebRequest request = new UnityWebRequest(m_ApiUrl, "POST"))
        {
            PostData _postData = new PostData
            {
                model = m_PostDataSetting.model,
                messages = m_DataList
            };
            Debug.Log(JsonUtility.ToJson(_postData).Length + " " + maxLength);
            //Debug.Log(JsonUtility.ToJson(_postData));
            if (JsonUtility.ToJson(_postData).Length > maxLength) isSimplify = true;
            string temp = m_DataList[m_DataList.Count - 1].content;
           
            if (Simplify)
            {
                string actionlist = "";
                for (int i = 0; i < (int)BehaviourType.Count; i++)
                {
                    if (i != 0) actionlist += ",";
                    actionlist += action.ActionList[i].name;
                }
                string locationlist = "";
                for (int i = 0; i < (int)LocationType.Count; i++)
                {
                    if (i != 0) locationlist += ",";
                    locationlist += Enum.GetName(typeof(LocationType), i);
                }
                m_DataList[m_DataList.Count - 1].content = Character + "," + SimplifyPrompts + JsonConvert.SerializeObject(action.EmotionList) + string.Format(", behaviours you can choose are:{0},", actionlist) + ".Following is the user's message: " + m_DataList[m_DataList.Count - 1].content;
                m_DataList[m_DataList.Count - 1].content += string.Format(",target you can move are:{0}",locationlist);
            }
            else
            {
                m_DataList[m_DataList.Count - 1].content = Character + Prompt + string.Format("The following are important matters，target、emotion、action、item reply in english,response reply translate to {0}.And only shows the json format,don't show any other content.", moegoe.language, (TextLanguage)setting.textLanguage) + "Emotion be chosen from following choices:" + JsonConvert.SerializeObject(action.EmotionList) + Target + (isSimplify ? SimplifyPrompt : "simplify is null string") + String.Format(",[favorability]:{0}", memory.Favorability) + String.Format("The following are items name you can give and its favorability，if current favorability is higher then the item's favorability,you can make decision you want to give the item to user or not,reject if current favorability is lower than the item's favorability.", JsonConvert.SerializeObject(action.ObjectList)) + "Following is the user's message: " + m_DataList[m_DataList.Count - 1].content + ((setting.camera && face.isFace) ? "user's current emotion is：" + face.emotion : "") + string.Format(",response translate to {0},", moegoe.language, (TextLanguage)setting.textLanguage) + ",time is null";
                m_DataList[m_DataList.Count - 1].content += string.Format("The following are your current state, your current behaviour is {0},your current location is {1},your current emotion is {2}，the date now is {3}", memory.currentBehaviour, memory.currentLocation, memory.currentEmotion, behaviour.Time);
                m_DataList[m_DataList.Count - 1].content += string.Format("The followings are memory list with what you have done,memory include time,behaviour,location,emotion and items you give to user,the meomory list is here:{0}", JsonConvert.SerializeObject(memory.MemoryList));
                string locationlist = "";
                for (int i = 0; i < (int)LocationType.Count; i++)
                {
                    if (i != 0) locationlist += ",";
                    locationlist += Enum.GetName(typeof(LocationType), i);
                }
                string actionlist = "";
                for (int i = 0; i < (int)BehaviourType.Count; i++)
                {
                    if (i != 0) actionlist += ",";
                    actionlist += action.ActionList[i].name;
                }
                string emotionlist = "";
                for (int i = 0; i < (int)EmotionType.Count; i++)
                {
                    if (i != 0) emotionlist += ",";
                    emotionlist += Enum.GetName(typeof(EmotionType), i);
                }
                if (setting.Behaviour.text != "") memory.BehaviourSet = setting.Behaviour.text;
                m_DataList[m_DataList.Count - 1].content += string.Format("The time now is:{0}, locations you can move are:{1},behaviours you can choose are:{2},emotions you can express are:{3}, your lifestyle is:{4}", memory.currentTime, locationlist + ",User", actionlist, emotionlist, memory.BehaviourSet);
            }
            string _jsonText = JsonUtility.ToJson(_postData);
            Debug.Log(_jsonText);
            m_DataList[m_DataList.Count - 1].content = temp;
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", _openAI_Key));
            Debug.Log(m_ApiUrl + "," + _openAI_Key);

            yield return request.SendWebRequest();
            if (request.responseCode == 200)
            {
                string _msg = request.downloadHandler.text;
                MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msg);
                Debug.Log(_textback.choices[0].message.content);
                if (_textback != null && _textback.choices.Count > 0)
                {
                    string res = _textback.choices[0].message.content;
                    if(res.Contains("{") && res.Contains("}"))
                    {
                        string[] reslist = res.Split(new char[2] { '{', '}' });
                        res = "{" + reslist[1] + "}";
                        try
                        {
                            var obj = JsonConvert.DeserializeObject(res);
                        }
                        catch (JsonReaderException)
                        {
                            Debug.Log("回覆格式錯誤");
                            UnityEvent resend = new UnityEvent();
                            chat.ResetSpeak(true, resend);
                            resend.AddListener(()=> { StartCoroutine(GetPostData(_postWord, _openAI_Key, _callback)); });
                        }
                        Content response = JsonUtility.FromJson<Content>(res);
                        if (Simplify)
                        {                        
                            setting.model.SetInteger("Action", -1);
                            string _backMsg = response.response;
                            action.Parser(response,0);
                            action.EmotionAction();
                            _callback(new ResponseContent(_backMsg,_backMsg));
                            chat.m_ChatHistory.Add(_backMsg);
                            m_DataList.Add(new SendData("assistant", _backMsg));
                        }
                        else
                        {
                            action.Parser(response);
                            action.EmotionAction();
                            StartCoroutine(GetReponse(response.response, _openAI_Key, _callback));
                        }
                        memory.SetFavorability(response.favorability);
                        
                        //if (isDraw) sd.StartCoroutine(sd.SendRequest(response.prompt,"",0));
                        if (isSimplify)
                        {
                            isSimplify = false;
                            SendData tempdata = m_DataList[0];
                            m_DataList.Clear();
                            m_DataList.Add(tempdata);
                            m_DataList.Add(new SendData("user", "這是之前對話的摘要:" + response.simplify));
                        }
                        //FileManager.SaveDialogue(m_DataList);
                    }
                    else
                    {
                        Debug.Log("回覆格式錯誤");
                        UnityEvent resend = new UnityEvent();
                        chat.ResetSpeak(true, resend);
                        resend.AddListener(() => { StartCoroutine(GetPostData(_postWord, _openAI_Key, _callback)); });
                    }
                }
            }
        }
    }
    public IEnumerator GetPostDataAuto(string _openAI_Key)
    {
        setting.model.Play((int)BehaviourType.Think);
        isWait = true;
        waitTime = 0;
        moegoe.SetSend(false);
        m_DataList.Add(new SendData("user",""));
        using (UnityWebRequest request = new UnityWebRequest(m_ApiUrl, "POST"))
        {
            PostData _postData = new PostData
            {
                model = m_PostDataSetting.model,
                messages = m_DataList
            };
            Debug.Log(JsonUtility.ToJson(_postData).Length + " " + maxLength);
            if (JsonUtility.ToJson(_postData).Length > maxLength) isSimplify = true;
            string temp = m_DataList[m_DataList.Count - 1].content;
            //m_DataList[m_DataList.Count - 1].content = Character + Prompt + string.Format("以下為重要事項，target、emotion、action、item內容使用英文。並且只顯示json格式內容，不要顯示其他的回答。") + "Emotion從以下選項中選擇:" + string.Join(",", action.EmotionList) + Target + (isSimplify ? SimplifyPrompt : "simplify欄位留空") + String.Format(",[favorability]:{0}", memory.Favorability) + string.Format(",response內容使用{0},target內容使用英文。", moegoe.language);
            //m_DataList[m_DataList.Count - 1].content += string.Format("以下為你當前的狀態，你上一個執行的行為是{0}，你當前的地點是{1}，你當前的情緒是{2}，現在是{3}", memory.currentBehaviour, memory.currentLocation, memory.currentEmotion,behaviour.Time);
            //m_DataList[m_DataList.Count - 1].content += string.Format("以下為你之前時間、行為、地點、情緒以及給予物品的紀錄表，Time代表紀錄時間，Behaviour為記錄時的行為，Location為記錄時的地點，Emotion為記錄時的情緒，Item為記錄時給予玩家的物品，資料如下:{0}", JsonConvert.SerializeObject(memory.MemoryList));

            m_DataList[m_DataList.Count - 1].content = Character + Prompt + string.Format("The following are important matters，target、emotion、action、item reply in english.And only shows the json format,don't show any other content.") + "Emotion be chosen from following choices:" + string.Join(",", action.EmotionList) + Target + (isSimplify ? SimplifyPrompt : "simplify is null string") + String.Format(",[favorability]:{0}", memory.Favorability) + string.Format(",voiceresponse reply in {0},textresponse translate response's content to {1},textresponse cant be null,", moegoe.language, (TextLanguage)setting.textLanguage);
            m_DataList[m_DataList.Count - 1].content += string.Format("The following are your current state, your current behaviour is {0},your current location is {1},your current emotion is {2}，the date now is {3}", memory.currentBehaviour, memory.currentLocation, memory.currentEmotion, behaviour.Time);
            m_DataList[m_DataList.Count - 1].content += string.Format("The followings are memory list with what you have done,memory include time,behaviour,location,emotion and items you give to user,the meomory list is here:{0}", JsonConvert.SerializeObject(memory.MemoryList));
            string locationlist = "";
            for(int i = 0; i < (int)LocationType.Count; i++)
            {
                if (i != 0) locationlist += ",";
                locationlist += Enum.GetName(typeof(LocationType), i);
            }
            string actionlist = "";
            for (int i = 0; i < (int)BehaviourType.Count; i++)
            {
                if (i != 0) actionlist += ",";
                actionlist += Enum.GetName(typeof(LocationType), i);
            }
            string emotionlist = "";
            for (int i = 0; i < (int)EmotionType.Count; i++)
            {
                if (i != 0) emotionlist += ",";
                emotionlist += Enum.GetName(typeof(EmotionType), i);
            }
            if(setting.Behaviour.text!="") memory.BehaviourSet = setting.Behaviour.text;
            //m_DataList[m_DataList.Count - 1].content += string.Format("現在時間是:{0}，可移動的地點如下:{1}，可執行的行為如下:{2}，可選擇的情緒如下:{3}，你的角色設定的生活作息如下:{4}", memory.currentTime, locationlist + ",User", actionlist, emotionlist, memory.BehaviourSet);
            m_DataList[m_DataList.Count - 1].content += string.Format("The time now is:{0}, locations you can move are:{1},behaviours you can choose are:{2},emotions you can express are:{3}, your lifestyle is:{4}", memory.currentTime, locationlist + ",User", actionlist, emotionlist, memory.BehaviourSet);
            //m_DataList[m_DataList.Count - 1].content += "response以及textresponse欄位留空，依據時間、地點、當前情緒以及角色的生活作息以及上面的描述，從可選擇的行為中選擇接下來要執行的行為填入action，並且從可選擇的地點中決定要前往的地點填入target，time欄位填入接下來這個行動的持續時間，單位為秒，範圍為60至600";
            m_DataList[m_DataList.Count - 1].content += "response and textresponse field is null string,acording to the current time,location,emotion and your lifestule,choose the next behaviour from behaviours that can be chosen and put into action field,then choose the location you want to go into target fieldm, time field is the next behaviour's duraton time,unit is second,range is 60 to 600";
            string _jsonText = JsonUtility.ToJson(_postData);
            Debug.Log(_jsonText.Length);
            m_DataList.RemoveAt(m_DataList.Count-1);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", _openAI_Key));

            yield return request.SendWebRequest();
            if (request.responseCode == 200)
            {
                setting.model.SetBool("ToIdle", true);
                setting.model.Play((int)BehaviourType.Idle);
                isWait = false;
                yield return new WaitForSeconds(thinkTime);
                moegoe.SetSend(true);
                string _msg = request.downloadHandler.text;
                MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msg);
                Debug.Log(_textback.choices[0].message.content);
                if (_textback != null && _textback.choices.Count > 0)
                {
                    string res = _textback.choices[0].message.content;
                    if (res.Contains("{") && res.Contains("}"))
                    {
                        string[] reslist = res.Split(new char[2] { '{', '}' });
                        res = "{" + reslist[1] + "}";
                        try
                        {
                            var obj = JsonConvert.DeserializeObject(res);
                        }
                        catch (JsonReaderException)
                        {
                            Debug.Log("語音格式錯誤");
                            behaviour.isSend = false;
                        }
                        Content response = JsonUtility.FromJson<Content>(res);
                        action.Parser(response);
                        action.EmotionAction();
                        behaviour.timer = response.time;
                        behaviour.isSend = false;
                        behaviour.Play();
                    }
                    else
                    {
                        Debug.Log("語音格式錯誤");
                        behaviour.isSend = false;
                    }
                }
            }
        }
    }
    public IEnumerator GetReponse(string _postWord, string _openAI_Key, System.Action<ResponseContent> _callback)
    {
        waitTime = 0;
        using (UnityWebRequest request = new UnityWebRequest(m_ApiUrl, "POST"))
        {
            List<SendData> voice = new List<SendData>();
            SendData data = new SendData();
            data.role = "user";
            data.content = "Output in json format,the format is as follow:{public string voiceresponse;public string textresponse}," + String.Format("voiceresponse translate the input content to {0},textresponse translate the input content to {1}.", moegoe.language, (TextLanguage)setting.textLanguage) + "Here is the input content: " + _postWord;
            voice.Add(data);
            PostData _postData = new PostData
            {
                model = m_PostDataSetting.model,
                messages = voice
            };
            string _jsonText = JsonUtility.ToJson(_postData);
            Debug.Log(_jsonText);
            if(m_DataList.Count > 0) m_DataList.RemoveAt(m_DataList.Count - 1);
            byte[] databytes = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(databytes);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", _openAI_Key));

            yield return request.SendWebRequest();
            if (request.responseCode == 200)
            {
                setting.model.SetBool("ToIdle", true);
                setting.model.Play((int)BehaviourType.Idle);
                isWait = false;
                yield return new WaitForSeconds(thinkTime);
                string _msg = request.downloadHandler.text;
                MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msg);
                Debug.Log(_textback.choices[0].message.content);
                if (_textback != null && _textback.choices.Count > 0)
                {
                    string res = _textback.choices[0].message.content;
                    if (res.Contains("{") && res.Contains("}"))
                    {
                        string[] reslist = res.Split(new char[2] { '{', '}' });
                        res = "{" + reslist[1] + "}";
                        try
                        {
                            var obj = JsonConvert.DeserializeObject(res);
                        }
                        catch (JsonReaderException)
                        {
                            Debug.Log("翻譯格式錯誤");                          
                            UnityEvent resend = new UnityEvent();
                            chat.ResetSpeak(true, resend);
                            resend.AddListener(() => { StartCoroutine(GetReponse(_postWord, _openAI_Key, _callback)); });
                        }
                        ResponseContent response = JsonUtility.FromJson<ResponseContent>(res);
                        string _backMsg = response.textresponse;
                        _callback(response);
                        m_DataList.Add(new SendData("assistant", _backMsg));
                        //FileManager.SaveDialogue(m_DataList);
                    }
                    else
                    {
                        Debug.Log("翻譯格式錯誤");
                        UnityEvent resend = new UnityEvent();
                        chat.ResetSpeak(true, resend);
                        resend.AddListener(() => { StartCoroutine(GetReponse(_postWord, _openAI_Key, _callback)); });
                    }
                }
            }
        }
    }
}
#region 

[Serializable]
public class PostData
{
    public string model;
    public List<SendData> messages;
    public ResponseFormat format = new ResponseFormat();
    //public int max_tokens;
    //public float temperature;
    //public int top_p;
    //public float frequency_penalty;
    //public float presence_penalty;
}
[Serializable]
public class ResponseFormat
{
    public string type = "json_object";
}
[Serializable]
public class SendData
{
    public string role;
    public string content;
    public SendData() { }
    public SendData(string _role, string _content)
    {
        role = _role;
        content = _content;
    }

}
[Serializable]
public class MessageBack
{
    public string id;
    public string created;
    public string model;
    public List<MessageBody> choices;
}
[Serializable]
public class MessageBody
{
    public Message message;
    public string finish_reason;
    public string index;
}
[Serializable]
public class Message
{
    public string role;
    public string content;
}
[Serializable]
public class Content
{
    public string response = "";
    public string target = "";
    public string simplify = "";
    public int favorability = -1;
    public string emotion = "";
    public string action = "";
    public string item = "";
    public bool choice = false;
    public float time = 0;
    public Content(string r, string e, string a)
    {
        response = r;
        target = string.Empty;
        simplify = string.Empty;
        favorability = -1;
        emotion = e;
        action = a;
        item = string.Empty;
        choice = false;
        time = 0;
    }
}
public class ResponseContent
{
    public string voiceresponse;
    public string textresponse;
    public ResponseContent(string v,string t)
    {
        voiceresponse = v;
        textresponse = t;
    }
}
public enum TextLanguage
{
    中文,
    Japanese
}
#endregion

