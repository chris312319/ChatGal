using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VmdPlayer : MonoBehaviour
{
    public UnityVMDPlayer vmdPlayer;
    public Animator animator;

    //VMDControllerのStartを待つため適当に待つ

    // Start is called before the first frame update
    void Start()
    {
        vmdPlayer = GetComponent<UnityVMDPlayer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        
    }
    public void Initial()
    {
        if (!vmdPlayer) vmdPlayer = GetComponent<UnityVMDPlayer>();
        if (!animator) animator = GetComponent<Animator>();
    }
    public void PlayVmd(string path)
    {
        Initial();
        animator.Play("Idle");
        animator.enabled = false;
        vmdPlayer.enabled = true;
        vmdPlayer.Play(path);
    }
    public void PlayAnimator(int index)
    {
        Initial();
        vmdPlayer.Stop();
        vmdPlayer.enabled = false;
        //animator.Play(name);
        SetInteger("Action",index);
    }
    public void SetBool(string name,bool flag)
    {
        Initial();
        if(animator) animator.SetBool(name, flag);
    }
    public void SetInteger(string name,int i)
    {
        Initial();
        if(animator) animator.SetInteger(name, i);
    }
}
