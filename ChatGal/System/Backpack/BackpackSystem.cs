using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class BackpackSystem : MonoBehaviour
{
    // Start is called before the first frame update
    public List<ItemInfo> itemInfos;

    public GameObject backpackPanel;
    public GameObject InfoPanel;
    public int Count;
    public BackpackItem[] items;
    public List<Item> itemlist;
    public GameObject root,grid;
    public bool isOpen;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isOpen)
        {
            MouseDetect();
        }
        else
        {
            InfoPanel.SetActive(false);
        }
    }
    public void Open()
    {
        isOpen = true;
        backpackPanel.SetActive(true);
    }
    public void Close()
    {
        isOpen = false;
        backpackPanel.SetActive(false);
    }
    public void Create()
    {
        items = new BackpackItem[Count];
        for (int i = 0; i < Count; i++)
        {
            GameObject temp = Instantiate(grid, root.transform);
            temp.name = "Backpack " + i;
            items[i] = temp.GetComponent<BackpackItem>();
            items[i].backpack = this;
        }
    }
    public void Refresh()
    {
        if(itemlist.Count < Count)
        {
            int offset = Count - itemlist.Count;
            for (int i=0;i<offset; i++)
            {
                itemlist.Add(new Item());
            }
        }
        for (int i = 0; i < Count - 1; i++)
        {
            if(itemlist[i].id == -1)
            {
                Item temp = itemlist[i];
                itemlist[i] = itemlist[i + 1];
                itemlist[i + 1] = temp;
            }
        }
        for (int i = 0; i < Count; i++)
        {
            items[i].id = itemlist[i].id;
            items[i].count = itemlist[i].count;
            items[i].Refresh();
        }
    }
    public bool Add(int id,int count)
    {
        bool flag = false;
        for(int i = 0; i < Count; i++)
        {
            if(itemlist[i].id == id || itemlist[i].id == -1)
            {
                itemlist[i].id = id;
                itemlist[i].count += count;
                flag = true;
                break;
            }
        }
        Refresh();
        return flag;
    }
    public bool Delete(int id,int count)
    {
        bool flag = false;
        for (int i = 0; i < Count; i++)
        {
            if (itemlist[i].id == id && itemlist[i].count > count)
            {
                flag = true;
                itemlist[i].count -= count;
                if (itemlist[i].count == 0)
                {
                    itemlist[i].id = -1;
                }
            }
        }
        Refresh();
        return flag;
    }
    public void MouseDetect()
    {
        //如果碰撞到了
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position =Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        //向点击位置发射一条射线，检测是否点击UI
        EventSystem.current.RaycastAll(eventData, raycastResults);
        if (raycastResults.Count > 0)
        {
            for (int i = 0; i < raycastResults.Count; i++)
            {
                if (raycastResults[i].gameObject.GetComponent<BackpackItem>())
                {
                    BackpackItem item = raycastResults[i].gameObject.GetComponent<BackpackItem>();
                    if (item.id != -1)
                    {
                        InfoPanel.transform.GetChild(0).GetComponent<Text>().text = item.Name;
                        InfoPanel.transform.GetChild(1).GetComponent<Text>().text = item.Info;
                        InfoPanel.transform.position = raycastResults[i].gameObject.transform.position + new Vector3(75, 50, 0);
                        InfoPanel.SetActive(true);
                        break;
                    }
                    else
                    {
                        InfoPanel.SetActive(false);
                    }
                }
                else
                {
                    InfoPanel.SetActive(false);
                }
            }
        }
        else
        {
            InfoPanel.SetActive(false);
        }
    }
}
[Serializable]
public class Item
{
    public int id = -1;
    public int count = 0;
    public int type = -1;
}
[Serializable]
public class ItemInfo
{
    public string prompt;
    public Sprite sprite;
    public string name;
    public string info;
    public int type;
    public int favorability;
}
