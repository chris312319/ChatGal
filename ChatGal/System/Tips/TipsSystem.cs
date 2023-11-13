using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipsSystem : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject tip;
    public Text info;
    bool isActive;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isActive && Input.GetMouseButtonDown(0))
        {
            Close();
        }
    }
    public void Open(string text)
    {
        info.text = text;
        tip.SetActive(true);
        isActive = true;
    }
    public void Close()
    {
        info.text = "";
        tip.SetActive(false) ;
        isActive = false;
    }
}
