using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackpackItem : MonoBehaviour
{
    // Start is called before the first frame update
    public BackpackSystem backpack;
    public Image image;
    public Text countText;
    public string Name;
    public string Info;
    public int Type;
    public int id;
    public int count;
    void Start()
    {

    }
    public void Refresh()
    {
        if (id >= 0) 
        {
            Name = backpack.itemInfos[id].name;
            Info = backpack.itemInfos[id].info;
            Type = backpack.itemInfos[id].type;
            image.sprite = backpack.itemInfos[id].sprite;
        }
        else
        {
            Name = "";
            Info = "";
            Type = -1;
            image.sprite = null;
        }
        countText.text = count + "";
        if (count == 0) countText.text = "";
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
