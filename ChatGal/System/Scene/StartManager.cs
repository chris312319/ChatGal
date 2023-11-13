using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class StartManager : MonoBehaviour
{
    // Start is called before the first frame update'p
    public ModelLoader model;
    public GameObject SettingPanel;
    public GameObject AboutPanel;
    public GameObject LoadPanel;
    public GameObject LoadingPanel;
    public GameObject[] btns;
    public InputField key, cha;
    private AsyncOperation async = null;
    void Start()
    {
        FileManager.LoadData();
        UserSetting set = FileManager.LoadUserSetting();
        model.LoadModel(set.PmxPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartGame()
    {
        if (File.Exists(Application.streamingAssetsPath + "/" + FileManager.dialoguedata))
        {
            File.Delete(Application.streamingAssetsPath + "/" + FileManager.dialoguedata);
        }
        StartCoroutine(LoadScene(1));
    }
    public void LoadGame()
    {
        StartCoroutine(LoadScene(1));
    }
    public void Open(int index)
    {
        switch (index)
        {
            case 0:
                UserSetting set = FileManager.LoadUserSetting();
                key.text = set.ApiKey;
                cha.text = set.CharacterSet;
                SettingPanel.SetActive(true);
                break;
            case 1:
                AboutPanel.SetActive(true);
                break;
            case 2:
                FileManager.LoadData();
                RefreshSaveData();
                LoadPanel.SetActive(true);
                break;
        }
    }
    public void Close(int index)
    {
        switch (index)
        {
            case 0:
                UserSetting setting = new UserSetting();
                setting.ApiKey = key.text;
                setting.CharacterSet = cha.text;
                FileManager.SaveUserSetting(setting,true);
                SettingPanel.SetActive(false);
                break;
            case 1:
                AboutPanel.SetActive(false);
                break;
            case 2:
                LoadPanel.SetActive(false);
                break;
        }
    }
    public void LoadData(int i)
    {
        FileManager.SaveDialogue(FileManager.LoadData(i));
        LoadGame();
    }
    public void RefreshSaveData()
    {
        for (int j = 0; j < btns.Length; j++)
        {
            float timer = FileManager.datalist.list[j].timer;
            btns[j].GetComponentInChildren<Text>().text = (((int)timer) / 60).ToString("00") + ":" + (((int)timer) % 60).ToString("00");
            if (timer <= 0) 
            {
                btns[j].GetComponentInChildren<Button>().enabled = false;
                btns[j].GetComponentInChildren<Text>().text = "No Data";
            }
            else btns[j].GetComponentInChildren<Button>().enabled = true;
        }
    }
    public IEnumerator LoadScene(int i)
    {
        LoadingPanel.SetActive(true);
        yield return new WaitUntil(() => LoadingPanel.activeInHierarchy);
        async = SceneManager.LoadSceneAsync(i);
        async.allowSceneActivation = true;
        while (!async.isDone)
        {
            Debug.Log("Loading");
            yield return true;
        }
        async.allowSceneActivation = true;
    }
}
