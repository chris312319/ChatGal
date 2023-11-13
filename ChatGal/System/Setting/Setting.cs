using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System.IO;
using TMPro;

public class Setting : MonoBehaviour
{
    // Start is called before the first frame update
    public StableDiffusion stable;
    public ModelLoader model;
    public GameObject Panel;
    public InputField Apikey, Character, Behaviour;
    public ChatScript chat;
    public GptTurboScript gptturbo;
    public Moegoe moegoe;
    public ChatScript chatScript;
    public GptTurboScript gpt;
    public Dropdown PmxSource;
    public InputField PmxPath;
    public InputField StableUrl;
    public InputField ModelType;
    public InputField GirlName;
    public Dropdown TextLanguage;
    public string girlName;
    public string modelType;
    public string stableUrl;
    public int pmxSource;
    public string pmxPath;
    public bool isStart;
    public float timer;
    public int textLanguage;
    public GameObject pmxButton,pmxPanel,loadingPanel;
    public Slider SpeakVolume, MusicVolume;
    public float speakVolume, musicVolume;
    public AudioSource Speak, Music;
    public Toggle Camera,AutoBehaviour;
    new public bool camera;
    public bool isOpen,isBehaviour;
    void Start()
    {
        UserSetting set = FileManager.LoadUserSetting();
        GirlName.text = set.GirlName;
        ModelType.text = set.ModelType;
        StableUrl.text = set.StableUrl;
        Apikey.text = set.ApiKey;
        Character.text = set.CharacterSet;
        Behaviour.text = set.BehaviourSet;
        chatScript.m_PlayToggle.isOn = set.isVoice;
        gpt.draw.isOn = set.isDraw;
        TextLanguage.value = set.TextLanguage;
        Camera.isOn = set.isCamera;
        AutoBehaviour.isOn = set.isBehaviour;
        moegoe.sourceDropdown.value = set.ModelSource;
        moegoe.modelsourcePath.text = set.ModelPath;
        moegoe.configsourcePath.text = set.ConfigPath;
        moegoe.APIUrl.text = set.ApiUrl;
        moegoe.SetSource();
        moegoe.SetModelPath();
        moegoe.SetConfigPath();
        if (set.ModelSource == 1) moegoe.ReInitVits();
        else if (set.ModelSource == 2) moegoe.InitAPIVits();
        moegoe.languageDropdown.value = set.Language;
        moegoe.speakersDropdown.value = int.Parse(set.VoiceCharacter);
        moegoe.SetLanguage();
        moegoe.SetSpeakers();
        PmxSource.value = set.PmxSource;
        PmxPath.text = set.PmxPath;
        pmxSourceChanged();
        pmxPathChanged();
        SpeakVolume.value = set.SpeakVolume;
        MusicVolume.value = set.MusicVolume;
        speakVolumeChanged();
        musicVolumeChanged();
        StableUrlChanged();
        ModelTypeChanged();
        GirlNameChanged();
        CameraChanged();
        IsBehaviourChanged();
        if(pmxSource != 0) CreateModel();
        Music.clip = Resources.Load<AudioClip>("Audio/backAudio");
        Music.Play();
        Set();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Set()
    {
        if(true)
        {
            chat.m_OpenAI_Key = Apikey.text;
            if(Character.text != "")
            {
                gptturbo.Character = "以下是你的人物設定:" + Character.text;
            }
            Panel.SetActive(false);
            isOpen = false;
            if (!isStart)
            {
                isStart = true;
            }
            UserSetting setting = new UserSetting();
            setting.GirlName = girlName;
            setting.ModelType = modelType;
            setting.StableUrl = stableUrl;
            setting.ApiKey = Apikey.text;
            setting.CharacterSet = Character.text;
            setting.BehaviourSet = Behaviour.text;
            setting.isCamera = Camera.isOn;
            setting.isVoice = chatScript.m_PlayToggle.isOn;
            setting.isDraw = gpt.draw.isOn;
            setting.isBehaviour = AutoBehaviour.isOn;
            setting.ModelSource = moegoe.sourceDropdown.value;
            setting.ModelPath = moegoe.customModelPath;
            setting.ConfigPath = moegoe.customConfigPath;
            setting.ApiUrl = moegoe.APIUrl.text;
            setting.PmxSource = pmxSource;
            setting.PmxPath = pmxPath;
            setting.Language = moegoe.languageDropdown.value;
            setting.TextLanguage = textLanguage;
            setting.VoiceCharacter = moegoe.VoiceIndex;
            setting.SpeakVolume = speakVolume;
            setting.MusicVolume = musicVolume;
            FileManager.SaveUserSetting(setting,false);
        }

    }
    public void Open()
    {
        UserSetting set = FileManager.LoadUserSetting();
        Apikey.text = set.ApiKey;
        Character.text = set.CharacterSet;
        Panel.SetActive(true);
        isOpen = true;
    }
    public void Back()
    {
        SceneManager.LoadScene(0);
    }
    public void ChoosePmxPath()
    {
        OpenFileName openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = "模型文件(*.pmx;*.pmd;*.vrm)\0*.pmx;*.pmd;*.vrm\0Mmd文件(*.pmx;*.pmd)\0*.pmx;*.pmd\0Vrm文件(*.vrm)\0*.vrm\0\0";
        openFileName.file = new string(new char[256]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = UnityEngine.Application.streamingAssetsPath.Replace('/', '\\');//默认路径
        openFileName.title = "選擇人物模型";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
        if (LocalDialogue.GetSaveFileName(openFileName))//点击系统对话框框保存按钮
        {
            PmxPath.text = openFileName.file;
            pmxPath = openFileName.file;
        }
    }
    public void pmxSourceChanged()
    {
        pmxSource = PmxSource.value;
        if(pmxSource == 0)
        {
            PmxPath.text = "";
            pmxPath = "";
            pmxPanel.SetActive(false);
            CreateModel();
        }
        else
        {
            pmxPanel.SetActive(true);
        }
    }
    public void pmxPathChanged()
    {
        pmxPath = PmxPath.text;
        if (pmxPath.Contains(".pmx") || pmxPath.Contains(".pmd") || pmxPath.Contains(".vrm") || pmxPath.Contains(".bva") || pmxPath.Contains(".glb")|| pmxPath.Contains(".gltf"))
        {
            pmxButton.SetActive(true);
        }
        else
        {
            pmxButton.SetActive(false);
        }
    }
    public void CreateModel()
    {
        StartCoroutine(Load());
    }
    public void speakVolumeChanged()
    {
        speakVolume = SpeakVolume.value;
        Speak.volume = speakVolume;
    }
    public void musicVolumeChanged()
    {
        musicVolume = MusicVolume.value;
        Music.volume = musicVolume;
    }
    public void StableUrlChanged()
    {
        stableUrl = StableUrl.text;
        stable.url = stableUrl;
    }
    public void ModelTypeChanged()
    {
        modelType = ModelType.text;
        gptturbo.m_PostDataSetting.model = modelType;
    }
    public void GirlNameChanged()
    {
        girlName = GirlName.text;
        moegoe.Name.text = girlName;
    }
    public void TextLanguageChanged()
    {
        textLanguage = TextLanguage.value;
    }
    public void CameraChanged()
    {
        camera = Camera.isOn;
    }
    public void IsBehaviourChanged()
    {
        isBehaviour = AutoBehaviour.isOn;
    }
    public IEnumerator Load()
    {
        pmxButton.SetActive(false);
        loadingPanel.SetActive(true);
        model.DeleteModel();
        yield return new WaitUntil(()=>loadingPanel.activeInHierarchy);
        model.LoadModel(pmxPath);
        yield return new WaitUntil(() => GameObject.Find("Girl"));       
        loadingPanel.SetActive(false);
    }
}
