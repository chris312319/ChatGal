using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using UnityEngine.Networking;
using System;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public enum Language
{
    中文,
    日文
}
public enum ModelSource
{
    本地,
    自訂,
    API
}
public class Config
{
    public string train;
    public string data;
    public string model;
    public string[] speakers;
    public string symbols;
}
public class Moegoe : MonoBehaviour
{
    // Start is called before the first frame update
    public ModelLoader model;
    public GameObject InitPanel,SourcePanel,APISourcePanel;
    public GameObject Resetbtn;
    public ChatScript chat;
    public UnityEngine.UI.Button sendbtn,voicebtn;
    public AudioSource audios;
    public Process moegoe;
    public string ModelPath, ConfigPath, VoiceIndex;
    public string customModelPath, customConfigPath;
    public Language language;
    public ModelSource modelsource;
    public bool isStart,isSpeak;
    public bool SpeakEnabled;
    public InputField modelsourcePath, configsourcePath, APIUrl;
    public Dropdown languageDropdown,speakersDropdown,sourceDropdown;
    public Text Name;
    public Config config;
    public string[] Speakers;
    public bool moegoeActive;
    public float interval;
    public AudioClip[] clips;
    void Start()
    {
        //System.Console.InputEncoding = System.Text.Encoding.UTF8;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitVits() 
    {
        LoadSpeakers();
        moegoe = new Process();
        moegoe.StartInfo.FileName = UnityEngine.Application.streamingAssetsPath + "\\MoeGoe\\MoeGoe.exe";  //确定程序名
        moegoe.StartInfo.Arguments = ModelPath;
        moegoe.StartInfo.CreateNoWindow = true;
        moegoe.StartInfo.RedirectStandardInput = true;
        moegoe.StartInfo.RedirectStandardOutput = false;
        moegoe.StartInfo.UseShellExecute = false;
        moegoe.Start();
        moegoeActive = true;
        Speak("OK");
        SetSend(false);
    }
    public void Speak(string text)
    {
        text = text.Replace("\n", " ");
        text = text.Replace("\r", " ");
        UnityEngine.Debug.Log(text);
        if (modelsource != ModelSource.API) StartCoroutine(CreateVoice(text));
        else StartCoroutine(CreateVoiceFromAPI(text));
    }
    public void SpeakSet()
    {
        SpeakEnabled = !SpeakEnabled;
        if (SpeakEnabled)
        {
            voicebtn.image.color = Color.green;
            voicebtn.GetComponentInChildren<Text>().text = "Voice On";
        }
        else if (!SpeakEnabled)
        {
            voicebtn.image.color = Color.gray;
            voicebtn.GetComponentInChildren<Text>().text = "Voice Off";
        }
    }
    public IEnumerator CreateVoice(string text)
    {
        string[] texts = text.Split(new char[] {'？','！','。','!','?','.'});
        if (!isStart)
        {
            if(modelsource == ModelSource.本地)
            {
                UnityEngine.Debug.Log(UnityEngine.Application.streamingAssetsPath + ModelPath + " " + UnityEngine.Application.streamingAssetsPath + ConfigPath);
                moegoe.StandardInput.WriteLine(UnityEngine.Application.streamingAssetsPath + ModelPath);
                moegoe.StandardInput.WriteLine(UnityEngine.Application.streamingAssetsPath + ConfigPath);
            }
            else
            {
                UnityEngine.Debug.Log(customModelPath + " " + customConfigPath);
                moegoe.StandardInput.WriteLine(customModelPath);
                moegoe.StandardInput.WriteLine(customConfigPath);
            }
            UnityEngine.Debug.Log("初始化完成");
        }
        for(int i = 0; i < texts.Length; i++)
        {
            UnityEngine.Debug.Log("生成開始");
            moegoe.StandardInput.WriteLine("t");
            if (language == Language.中文) moegoe.StandardInput.WriteLine("[ZH]" + texts[i] + "[ZH]");
            if (language == Language.日文) moegoe.StandardInput.WriteLine("[JA]" + texts[i] + "[JA]");
            moegoe.StandardInput.WriteLine(VoiceIndex);
            moegoe.StandardInput.WriteLine(UnityEngine.Application.streamingAssetsPath + "\\t" + i + ".wav");
            moegoe.StandardInput.WriteLine("y");
        }
        chat.currentIndex = 0;
        for (int i = 0; i < texts.Length; i++)
        {
            yield return new WaitUntil(() => File.Exists(UnityEngine.Application.streamingAssetsPath + "\\t" + i + ".wav"));
            UnityEngine.Debug.Log("生成完成");
            //chat.StartCoroutine(chat.SetText(texts[i]));           
            yield return StartCoroutine(PlayAudio(UnityEngine.Application.streamingAssetsPath + "\\t" + i + ".wav", AudioType.WAV,i));
            chat.currentIndex++;
        }
        model.Play((int)BehaviourType.Idle);
        Complete();
        yield return new WaitForSeconds(2f);
        chat.StartCoroutine(chat.SetText(" "));
        chat.currentIndex = 0;
    }
    public IEnumerator CreateVoiceFromAPI(string text)
    {
        string[] texts = text.Split(new char[] { '？', '！', '。', '!', '?', '.' });
        clips = new AudioClip[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            UnityEngine.Debug.Log("生成開始 " + i);
            if(texts[i].Length > 0) StartCoroutine(CreateVoiceClipFromAPI(texts[i], i));
        }
        chat.currentIndex = 0;
        for (int i = 0; i < texts.Length; i++)
        {
            yield return new WaitUntil(() => clips[i] != null || texts[i].Length == 0);
            model.SetBool("isTalk", true);
            UnityEngine.Debug.Log("生成完成");
            audios.clip = clips[i];
            audios.Play();
            if(texts[i].Length > 0) yield return new WaitForSeconds(audios.clip.length);
            chat.currentIndex++;
        }
        model.Play((int)BehaviourType.Idle);
        Complete();
        yield return new WaitForSeconds(2f);
        chat.StartCoroutine(chat.SetText(" "));
        chat.currentIndex = 0;
    }
    public IEnumerator CreateVoiceClipFromAPI(string text,int index)
    {
        string url = APIUrl.text + "/voice/bert-vits2";
        text = text.Replace(" ","");
        //string url = APIUrl.text + "/voice/vits";
        url += "?text=" + text + "&id=" + VoiceIndex + "&lang=" + (language == Language.中文? "zh" : "ja") + "&format=wav";
        using (var request = UnityWebRequestMultimedia.GetAudioClip(url,AudioType.WAV))
        {
            yield return request.SendWebRequest();
            if (request.responseCode == 200)
            {
                AudioClip audioclip = DownloadHandlerAudioClip.GetContent(request);
                UnityEngine.Debug.Log("獲取音頻");
                clips[index] = audioclip;
            }
            else
            {
                UnityEngine.Debug.Log(request.error);
            }
        }
    }
    private IEnumerator PlayAudio(string _url, AudioType _audioType,int i)
    {
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(_url, _audioType);
        yield return request.SendWebRequest();        
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            UnityEngine.Debug.Log(request.error.ToString());
        }
        else
        {
            AudioClip _audioClip = DownloadHandlerAudioClip.GetContent(request);
            audios.clip = _audioClip;
            UnityEngine.Debug.Log("播放");
            audios.Play();
            model.SetBool("isTalk", true);
            yield return new WaitForSeconds(_audioClip.length);
            if (_audioClip.length > 1f) yield return new WaitForSeconds(interval);
            File.Delete(UnityEngine.Application.streamingAssetsPath + "\\t" + i + ".wav");
            audios.clip = null;
            UnityEngine.Debug.Log("刪除");
        }
    }
    public void Complete()
    {
        chat.model.SetBool("isTalk", false);
        chat.model.SetBool("ToIdle", true);
        chat.action.Action();        
        chat.interview.OnAnimationFinishPlaying();
        isStart = true;
        SetSend(true);
        InitPanel.SetActive(false);
        SourcePanel.SetActive(false);
        APISourcePanel.SetActive(false);
        Resetbtn.SetActive(true);
    }
    public void OnDisable()
    {
        if (moegoe != null) 
        {
            moegoe.Close();
            moegoe.Dispose();
        }
        if(File.Exists(UnityEngine.Application.streamingAssetsPath + "\\t.wav"))  File.Delete(UnityEngine.Application.streamingAssetsPath + "\\t.wav");
    }
    public void OnApplicationQuit()
    {
        if (File.Exists(UnityEngine.Application.streamingAssetsPath + "\\t.wav")) File.Delete(UnityEngine.Application.streamingAssetsPath + "\\t.wav");
    }
    public void SetSend(bool flag)
    {
        sendbtn.enabled = flag;
        isSpeak = !flag;
        if (flag)
        {
            sendbtn.image.color = Color.green;
        }
        else
        {
            sendbtn.image.color = Color.gray;
        }
    }
    public void SetLanguage()
    {
        language = (Language)languageDropdown.value;
    }
    public void SetSpeakers()
    {
        VoiceIndex = speakersDropdown.value + "";
    }
    public void SetSource()
    {
        modelsource = (ModelSource)sourceDropdown.value;
        if(modelsource == ModelSource.本地)
        {
            SourcePanel.SetActive(false);
            APISourcePanel.SetActive(false);
            ReInitVits();
        }
        else if(modelsource == ModelSource.自訂)
        {
            SourcePanel.SetActive(true);
            APISourcePanel.SetActive(false);
        }
        else
        {
            SourcePanel.SetActive(false);
            APISourcePanel.SetActive(true);
        }
        Resetbtn.SetActive(false);
    }
    public void SetModelPath()
    {
        customModelPath = modelsourcePath.text;
    }
    public void SetConfigPath()
    {
        customConfigPath = configsourcePath.text;
    }
    public void ChooseModelPath()
    {         
           OpenFileName openFileName = new OpenFileName();
           openFileName.structSize = Marshal.SizeOf(openFileName);
           openFileName.filter = "文件(*." + "pth" + ")\0*." + "pth" + "";
           openFileName.file = new string(new char[256]);
           openFileName.maxFile = openFileName.file.Length;
           openFileName.fileTitle = new string(new char[64]);
           openFileName.maxFileTitle = openFileName.fileTitle.Length;
           openFileName.initialDir = UnityEngine.Application.streamingAssetsPath.Replace('/', '\\');//默认路径
           openFileName.title = "選擇VITS模型";
           openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
           if (LocalDialogue.GetSaveFileName(openFileName))//点击系统对话框框保存按钮
           {
            modelsourcePath.text = openFileName.file;
            customModelPath = openFileName.file;
           }
    }
    public void ChooseConfigPath()
    {
        OpenFileName openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = "文件(*." + "json" + ")\0*." + "json" + "";
        openFileName.file = new string(new char[256]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = UnityEngine.Application.streamingAssetsPath.Replace('/', '\\');//默认路径
        openFileName.title = "選擇配置文件";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
        if (LocalDialogue.GetSaveFileName(openFileName))//点击系统对话框框保存按钮
        {
            configsourcePath.text = openFileName.file;
            customConfigPath = openFileName.file;
        }
    }
    public void ReInitVits()
    {
        if(modelsource == ModelSource.自訂)
        {
            if (!File.Exists((customModelPath)) || !File.Exists((customConfigPath))) return;
            UnityEngine.Debug.Log("檔案存在");
            if (Path.GetExtension(customModelPath) != ".pth") return;
            UnityEngine.Debug.Log("模型副檔名正確");
            if (Path.GetExtension(customConfigPath) != ".json" || Path.GetFileNameWithoutExtension(customConfigPath) != "config") return;
            UnityEngine.Debug.Log("配置文件副檔名正確");
        }
        VoiceIndex = "0";
        language = Language.中文;
        languageDropdown.value = 0;
        languageDropdown.RefreshShownValue();
        speakersDropdown.value = 0;
        chat.ResetSpeak(false);
        isStart = false;
        isSpeak = true;
        if (moegoeActive) 
        {
            moegoe.Kill();
            moegoe.Dispose();
            moegoeActive = false;
        }
        InitPanel.SetActive(true);
        InitVits();
    }
    public void InitAPIVits()
    {
        VoiceIndex = "0";
        language = Language.中文;
        languageDropdown.value = 0;
        languageDropdown.RefreshShownValue();
        speakersDropdown.value = 0;
        chat.ResetSpeak(false);
        isStart = false;
        isSpeak = true;
        if (moegoeActive)
        {
            moegoe.Kill();
            moegoe.Dispose();
            moegoeActive = false;
        }
        InitPanel.SetActive(true);
        StartCoroutine(LoadSpeakersFromAPI());
    }
    public void LoadSpeakers()
    {
        if(File.Exists(UnityEngine.Application.streamingAssetsPath + ConfigPath))
        {
            string input = "";
            if(modelsource == ModelSource.本地) input = File.ReadAllText(UnityEngine.Application.streamingAssetsPath + ConfigPath);
            else input = File.ReadAllText(customConfigPath);
            config = JsonUtility.FromJson<Config>(input);
            Speakers = config.speakers;
            speakersDropdown.options.Clear();
            for(int i = 0; i < Speakers.Length; i++)
            {
                Dropdown.OptionData data = new Dropdown.OptionData();
                data.text = Speakers[i];
                speakersDropdown.options.Add(data);
            }
            speakersDropdown.value = 0;
            speakersDropdown.RefreshShownValue();
            //Name.text = speakersDropdown.options[speakersDropdown.value].text;
        }
    }
    public IEnumerator LoadSpeakersFromAPI()
    {
        string url = APIUrl.text + "/voice/speakers";
        UnityEngine.Debug.Log(url);
        string _jsonText = JsonConvert.SerializeObject("");
        byte[] b = System.Text.Encoding.UTF8.GetBytes(_jsonText);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(b);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            UnityEngine.Debug.Log("Request Speakers");
            yield return request.SendWebRequest();
            if (request.responseCode == 200)
            {
                UnityEngine.Debug.Log("Get Speakers");
                string res = request.downloadHandler.text;
                VitsContent content = JsonConvert.DeserializeObject<VitsContent>(res);
                Character[] speakers = content.BertVits2;
                //Character[] speakers = content.VITS;
                speakersDropdown.options.Clear();
                for (int i = 0; i < speakers.Length; i++)
                {
                    Dropdown.OptionData data = new Dropdown.OptionData();
                    data.text = speakers[i].name;
                    speakersDropdown.options.Add(data);
                }
                speakersDropdown.value = 0;
                speakersDropdown.RefreshShownValue();
                Complete();
            }
            else
            {
                UnityEngine.Debug.Log(request.error);
            }
        }
    }
}
[Serializable]
public class VitsPostData
{
    public string text;
    public string id;
    public string format;
    public string lang;
    public string length;
    public string noise;
    public string noisew;
    public string max;
}
[Serializable]
public class VitsContent
{
    [Newtonsoft.Json.JsonProperty("BERT-VITS2")]
    public Character[] BertVits2;
    [Newtonsoft.Json.JsonProperty("HUBERT-VITS")]
    public Character[] HubertVits;
    [Newtonsoft.Json.JsonProperty("VITS")]
    public Character[] VITS;
    [Newtonsoft.Json.JsonProperty("W2V2-VITS")]
    public Character[] W2V2VITS;
}
[Serializable]
public class Character
{
    public int id;
    public string[] lang;
    public string name;
}
