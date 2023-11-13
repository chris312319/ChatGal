using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawSystem : MonoBehaviour
{
    // Start is called before the first frame update
    public FurnitureSystem furniture;
    public StableDiffusion stable;
    public GameObject DrawPanel;
    public GameObject[] Buttons;
    public GameObject currentObject;
    public Image image;
    public InputField Prompt, Negative;
    public bool isDrawing;
    public int currentIndex;
    public string[] Base64;
    string tempBase64;
    void Start()
    {
        stable = GameObject.Find("StableDiffusion").GetComponent<StableDiffusion>();
        furniture = GameObject.Find("FurnitureSystem").GetComponent<FurnitureSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SaveBase64(string base64)
    {
        tempBase64 = base64;
    }
    public void Open(GameObject obj,int index)
    {
        tempBase64 = "";
        currentObject = obj;
        currentIndex = index;
        if(Base64[currentIndex] != null && Base64[currentIndex].Length > 100)
        {
            Texture2D tex = StableDiffusion.CreateTexture(Base64[currentIndex]);
            StableDiffusion.SetSprite(image, tex,tex.width,tex.height);
        }
        DrawPanel.SetActive(true);
    }
    public void Close()
    {
        currentObject = null;
        currentIndex = -1;
        image.sprite = null;
        DrawPanel.SetActive(false);
    }
    public void Draw()
    {
        SetButton(false);
        stable.StartCoroutine(stable.SendRequest(Prompt.text, Negative.text, 1));
    }
    public void Clear()
    {
        Prompt.text = "";
        Negative.text = "";
        image.sprite = null;
        tempBase64 = "";  
    }
    public void Save()
    {
        Base64[currentIndex] = tempBase64;
        for(int i = 0; i < furniture.Amount; i++)
        {
            furniture.Furnitures[i].Base64 = Base64[i];
        }
        if (Base64[currentIndex] != null && Base64[currentIndex].Length > 100)
        {
            Texture2D tex = StableDiffusion.CreateTexture(Base64[currentIndex]);
            StableDiffusion.SetMaterial(currentObject, StableDiffusion.CreateMaterial(tex));
        }
        else
        {
            currentObject.GetComponent<DrawObject>().ResetMaterial();
        }
        Close();
    }
    public void SetButton(bool flag)
    {
        for(int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].GetComponent<Button>().enabled = flag;
            if (flag) Buttons[i].GetComponent<Button>().image.color = Color.green;
            else Buttons[i].GetComponent<Button>().image.color = Color.gray;
        }
    }
}
