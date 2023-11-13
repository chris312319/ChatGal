using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractPos : MonoBehaviour
{
    // Start is called before the first frame update
    public Interactable interactable;
    bool isMouse = false;
    public int index;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isMouse && Input.GetKeyDown(KeyCode.E) && interactable.PlayerisOn == -1 && Vector3.Distance(this.gameObject.transform.position, interactable.player.transform.position) < interactable.InteractDis)
        {
            if (interactable.isOn[index] == 0)
            {
                interactable.Interact(index);
            }
        }
        if (isMouse &&　Input.GetKeyDown(KeyCode.R))
        {
            if (this.GetComponentInParent<DrawObject>())
            {
                this.GetComponentInParent<DrawObject>().Active(true);
            }
        }
    }
    public void OnMouseEnter()
    {
        isMouse = true;
    }
    public void OnMouseExit()
    {
        isMouse = false;
    }
}
