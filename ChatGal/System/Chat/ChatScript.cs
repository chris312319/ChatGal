using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using InterviewSystem.APIs;

public class ChatScript : MonoBehaviour
{
    //API key
    public ModelLoader model;
    public ChatDemo interview;
    public bool isMemory;
	[SerializeField]public string m_OpenAI_Key="填写你的Key";
	// 定义Chat API的URL
	private string m_ApiUrl = "https://api.openai.com/v1/completions";
    //配置参数
    [SerializeField]private GetOpenAI.PostData m_PostDataSetting;
    //聊天UI层
    [SerializeField]private GameObject m_ChatPanel;
    //输入的信息
    [SerializeField]public InputField m_InputWord;
    //返回的信息
    [SerializeField]private Text m_TextBack;
    //播放设置
    [SerializeField]public Toggle m_PlayToggle;
    //微软Azure语音
    [SerializeField] private Moegoe moegoe;
    //gpt-3.5-turbo
    [SerializeField] public GptTurboScript m_GptTurboScript;
    public ActionSystem action;
    public int currentIndex;
    public int currentType;
    public Text type_Text;
    public string InnerResponse;

   //发送信息
   public void SetType()
    {
        switch (currentType)
        {
            case 0:
                interview.Init();
                currentType = 1;
                type_Text.text = "Other";
                break;
            case 1:
                currentType = 0;
                type_Text.text = "GPT";
                break;
        }
    }
    public void SendData()
    {
        if(m_InputWord.text.Equals(""))
            return;
        //记录聊天
        moegoe.SetSend(false);
        string _msg=  m_InputWord.text;
        //string _msg =m_lan + " " + m_InputWord.text;
        //发送数据
        //StartCoroutine (GetPostData (_msg,CallBack));
        if (isMemory)
        {
            switch (currentType)
            {
                case 0:
                    StartCoroutine(m_GptTurboScript.GetPostData(_msg, m_OpenAI_Key, CallBack));
                    break;
                case 1:
                    interview.OnSubmit(_msg);
                    break;          
            }
        }
        m_TextBack.text="...";
        m_InputWord.text = "";
    }
    public void SendDataVoice(string voicetext)
    {
        moegoe.SetSend(false);
        //记录聊天
        string _msg = voicetext;
        //string _msg =m_lan + " " + m_InputWord.text;
        //发送数据
        //StartCoroutine (GetPostData (_msg,CallBack));
        if (isMemory)
        {
            switch (currentType)
            {
                case 0:
                    StartCoroutine(m_GptTurboScript.GetPostData(_msg, m_OpenAI_Key, CallBack));
                    break;
                case 1:
                    interview.OnSubmit(_msg);
                    break;
            }
        }
        m_TextBack.text = "...";
        m_InputWord.text = "";
    }

    //AI回复的信息
    public void CallBack(ResponseContent callback){       
        m_GptTurboScript.isWait = false;
        m_GptTurboScript.waitTime = 0;
        string _callback = callback.voiceresponse;
        string text = callback.textresponse;
        _callback=_callback.Trim();
        m_TextBack.text="";
        //开始逐个显示返回的文本
        m_WriteState=true;
        if(_callback.Length > 0) StartCoroutine(SetTextPerWordSplit(text,callback));
         //记录聊天
        if(m_PlayToggle.isOn){
            StartCoroutine(Speek(_callback));
        }
        else
        {
            moegoe.Complete();
        }
    }


    private IEnumerator Speek(string _msg){
        yield return new WaitForEndOfFrame();
        //播放合成并播放音频
        moegoe.Speak(_msg);
    }

	private IEnumerator GetPostData(string _postWord,System.Action<string> _callback)
	{
        using(UnityWebRequest request = new UnityWebRequest (m_ApiUrl, "POST")){   
        GetOpenAI.PostData _postData = new GetOpenAI.PostData
		{
			model = m_PostDataSetting.model,
			prompt = _postWord,
			max_tokens = m_PostDataSetting.max_tokens,
            temperature=m_PostDataSetting.temperature,
            top_p=m_PostDataSetting.top_p,
            frequency_penalty=m_PostDataSetting.frequency_penalty,
            presence_penalty=m_PostDataSetting.presence_penalty,
            stop=m_PostDataSetting.stop
		};

		string _jsonText = JsonUtility.ToJson (_postData);
		byte[] data = System.Text.Encoding.UTF8.GetBytes (_jsonText);
		request.uploadHandler = (UploadHandler)new UploadHandlerRaw (data);
		request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer ();

		request.SetRequestHeader ("Content-Type","application/json");
		request.SetRequestHeader("Authorization",string.Format("Bearer {0}",m_OpenAI_Key));

		yield return request.SendWebRequest ();

		if (request.responseCode == 200) {
			string _msg = request.downloadHandler.text;
			GetOpenAI.TextCallback _textback = JsonUtility.FromJson<GetOpenAI.TextCallback> (_msg);
			if (_textback!=null && _textback.choices.Count > 0) {
                    
                string _backMsg=Regex.Replace(_textback.choices [0].text, @"[\r\n]", "").Replace("？","");
                _callback(_backMsg);
			}	
		}
        }
	}
    public IEnumerator GetDataFromInner(System.Action<ResponseContent> _callback,string message,bool Save = true)
    {
        if (Save)
        {
            m_GptTurboScript.m_DataList.Add(new SendData("user", message));
            m_ChatHistory.Add(message);
        }
        m_GptTurboScript.setting.model.SetBool("ToIdle", false);
        m_GptTurboScript.setting.model.Play((int)BehaviourType.Think);
        string _msg = InnerResponse;//From
        yield return new WaitForSeconds(0f);

        Debug.Log(_msg);
        if (_msg != "")
        {
            string res = _msg;           
            Content response = JsonUtility.FromJson<Content>(res);
            if (m_GptTurboScript.Simplify)
            {
                m_GptTurboScript.setting.model.SetInteger("Action", -1);
                string _backMsg = response.response;
                action.Parser(response, 0);
                action.EmotionAction();
                _callback(new ResponseContent(_backMsg, _backMsg));
                if (Save)
                {
                    m_GptTurboScript.m_DataList.Add(new SendData("assistant", _backMsg));
                    m_ChatHistory.Add(_backMsg);
                }
            }
            m_GptTurboScript.memory.SetFavorability(response.favorability);

            //if (isDraw) sd.StartCoroutine(sd.SendRequest(response.prompt,"",0));
            if (m_GptTurboScript.isSimplify)
            {
                m_GptTurboScript.isSimplify = false;
                SendData tempdata = m_GptTurboScript.m_DataList[0];
                m_GptTurboScript.m_DataList.Clear();
                m_GptTurboScript.m_DataList.Add(tempdata);
                m_GptTurboScript.m_DataList.Add(new SendData("user", "這是之前對話的摘要:" + response.simplify));
            }
        }
    }


    #region 文字逐个显示
    //逐字显示的时间间隔
    [SerializeField]private float m_WordWaitTime=0.1f,m_SentenceWaitTime=0.8f;
    //是否显示完成
    [SerializeField]private bool m_WriteState=false;
    private IEnumerator SetTextPerWord(string _msg,ResponseContent response){
        if (_msg != null)
        {
            if (m_PlayToggle.isOn) yield return new WaitUntil(() => moegoe.audios.isPlaying);
            int currentPos = 0;
            while (m_WriteState)
            {
                yield return new WaitForSeconds(m_WordWaitTime);
                currentPos++;
                //更新显示的内容
                m_TextBack.text = _msg.Substring(0, currentPos);
                m_WriteState = currentPos < _msg.Length;
            }
        }
    }
    private IEnumerator SetTextPerWordSplit(string _msg, ResponseContent response)
    {
        if (_msg != null)
        {
            if (m_PlayToggle.isOn) yield return new WaitUntil(() => moegoe.audios.isPlaying);
            string[] sentences = _msg.Split(new char[] { '？', '！', '。', '!', '?', '.' });
            for (int i = 0; i < sentences.Length; i++)
            {
                int currentPos = 0;
                m_WriteState = true;
                while (m_WriteState)
                {
                    yield return new WaitForSeconds(m_WordWaitTime);
                    currentPos++;
                    //更新显示的内容
                    m_TextBack.text = sentences[i].Length >0 ? sentences[i].Substring(0, currentPos) : "";
                    m_WriteState = currentPos < sentences[i].Length;
                }
                if (m_PlayToggle.isOn) yield return new WaitUntil(() => currentIndex == i + 1);
                else yield return new WaitForSeconds(m_SentenceWaitTime);
                m_TextBack.text = "";
            }
        }   
    }
    public IEnumerator SetText(string _msg)
    {
        int currentPos = 0;
        m_WriteState = true;
        while (m_WriteState)
        {
            yield return new WaitForSeconds(m_WordWaitTime);
            currentPos++;
            //更新显示的内容
            m_TextBack.text = _msg.Length > 0 ? _msg.Substring(0, currentPos) : "";
            m_WriteState = currentPos < _msg.Length;
        }
    }
    #endregion


    #region 聊天记录
    //保存聊天记录
    [SerializeField]public  List<string> m_ChatHistory;
    //缓存已创建的聊天气泡
    [SerializeField]private List<GameObject> m_TempChatBox;
    //聊天记录显示层
    [SerializeField]private GameObject m_HistoryPanel;
    //聊天文本放置的层
    [SerializeField]private RectTransform m_rootTrans;
    //发送聊天气泡
    [SerializeField]private ChatPrefab m_PostChatPrefab;
    //回复的聊天气泡
    [SerializeField]private ChatPrefab m_RobotChatPrefab;
    //滚动条
    [SerializeField]private ScrollRect m_ScroTectObject;
    //获取聊天记录
    public void OpenAndGetHistory(){
        m_ChatPanel.SetActive(false);
        m_HistoryPanel.SetActive(true);

        ClearChatBox();
        StartCoroutine(GetHistoryChatInfo());
    }
    //返回
    public void BackChatMode(){
        m_ChatPanel.SetActive(true);
        m_HistoryPanel.SetActive(false);
    }

    //清空已创建的对话框
    private void ClearChatBox(){
        while(m_TempChatBox.Count!=0){
            if(m_TempChatBox[0]){
                Destroy(m_TempChatBox[0].gameObject);
                m_TempChatBox.RemoveAt(0);
            }
        }
        m_TempChatBox.Clear();
    }

    //获取聊天记录列表
    private IEnumerator GetHistoryChatInfo()
    {

        yield return new WaitForEndOfFrame();

       for(int i=0;i<m_ChatHistory.Count;i++){
        if(i%2==0){
            ChatPrefab _sendChat=Instantiate(m_PostChatPrefab,m_rootTrans.transform);
            _sendChat.SetText(m_ChatHistory[i]);
            m_TempChatBox.Add(_sendChat.gameObject);
            continue;
        }

         ChatPrefab _reChat=Instantiate(m_RobotChatPrefab,m_rootTrans.transform);
        _reChat.SetText(m_ChatHistory[i]);
        m_TempChatBox.Add(_reChat.gameObject);
       }

        //重新计算容器尺寸
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_rootTrans);
        StartCoroutine(TurnToLastLine());
    }

    private IEnumerator TurnToLastLine(){
        yield return new WaitForEndOfFrame();
         //滚动到最近的消息
        m_ScroTectObject.verticalNormalizedPosition=0;
    }


    #endregion


    #region 切换妹子
    //Lo娘
    [SerializeField]private GameObject m_LoGirl;
    [SerializeField]private GameObject m_Girl;

    //
    public void SetLoGirlShowed(GameObject _settingPanel){
        if(!m_LoGirl.activeSelf)
        {
            m_LoGirl.SetActive(true);
            m_Girl.SetActive(false);
        }
        //m_AzurePlayer.SetSound("zh-CN-XiaoyiNeural");

        _settingPanel.SetActive(false);
    }
    //zh-CN-XiaoxiaoNeural
    public void SetGirlShowed(GameObject _settingPanel){
        if(!m_Girl.activeSelf)
        {
            m_LoGirl.SetActive(false);
            m_Girl.SetActive(true);
        }
         //m_AzurePlayer.SetSound("zh-CN-liaoning-XiaobeiNeural");
        _settingPanel.SetActive(false);
    }
    public void ResetSpeak(bool flag,UnityEvent resend = null)
    {
        m_TextBack.text = "";
        if (model.vmdPlayer) model.SetBool("isTalk", false) ;
        model.SetBool("ToIdle", true);
        m_GptTurboScript.setting.model.Play((int)BehaviourType.Idle);
        StopAllCoroutines();
        moegoe.StopAllCoroutines();
        m_GptTurboScript.StopAllCoroutines();
        if (m_GptTurboScript.m_DataList.Count > 1 && m_GptTurboScript.m_DataList[m_GptTurboScript.m_DataList.Count - 1].role == "user") 
        {
            m_GptTurboScript.m_DataList.RemoveAt(m_GptTurboScript.m_DataList.Count - 1);
            m_ChatHistory.RemoveAt(m_ChatHistory.Count - 1);
        }
        if(File.Exists(Application.streamingAssetsPath + "\\t.wav")) File.Delete(Application.streamingAssetsPath + "\\t.wav");
        if(resend != null)
        {
            resend.Invoke();
        }
        else
        {
            if (flag) moegoe.SetSend(true);
        }
    }
    public void ResetSpeak_btn(bool flag)
    {
        m_GptTurboScript.setting.model.Play((int)BehaviourType.Idle);
        StopAllCoroutines();
        moegoe.StopAllCoroutines();
        m_GptTurboScript.StopAllCoroutines();
        if (m_GptTurboScript.m_DataList.Count > 1 && m_GptTurboScript.m_DataList[m_GptTurboScript.m_DataList.Count - 1].role == "user")
        {
            m_GptTurboScript.m_DataList.RemoveAt(m_GptTurboScript.m_DataList.Count - 1);
            m_ChatHistory.RemoveAt(m_ChatHistory.Count - 1);
        }
        if (File.Exists(Application.streamingAssetsPath + "\\t.wav")) File.Delete(Application.streamingAssetsPath + "\\t.wav");
        else
        {
            if (flag) moegoe.SetSend(true);
        }
    }
    #endregion


}
