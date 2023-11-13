using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    public FaceRecognizer face;
    public Setting setting;
    CharacterController controller;
    public float rotateSpeed, rotateheadSpeed, gravity;
    public float speed;
    public float jumpheight;
    public float minOffset;
    public GameObject Camera, DirObj;
    Vector3 mousePos;
    Vector3 Dir;
    public Vector3 CurPos,HeadPos;
    void Start()
    {
        controller = this.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Rotate();
        CameraRotate2();
        DirObj.transform.eulerAngles = new Vector3(0, Camera.transform.eulerAngles.y, 0);
        Vector3 dir = DirObj.transform.TransformDirection(Dir);
        dir.y = 0;
        controller.Move(dir * speed * Time.deltaTime);
    }
    public void Move()
    {
        if (Input.GetAxis("Horizontal") != 0)
        {
            Dir.x = Input.GetAxis("Horizontal");
        }
        else
        {
            Dir.x = 0;
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            Dir.z = Input.GetAxis("Vertical");
        }
        else
        {
            Dir.z = 0;
        }
        if (!controller.isGrounded)
        {
            Dir.y = gravity;
        }
        else
        {
            Dir.y = 0;
        }
    }
    public void Rotate()
    {
        if (!Input.GetKey(KeyCode.LeftControl)) mousePos = Input.mousePosition;
        if (mousePos == Vector3.zero) mousePos = Input.mousePosition;
        Vector3 offset = Input.mousePosition - mousePos;
        this.transform.localEulerAngles += new Vector3(0, offset.x, 0) * rotateSpeed * Time.deltaTime;
        mousePos = Input.mousePosition;
    }
    public void CameraRotate()
    {
        if(face.isFace && setting.camera)
        {
            Vector3 current = face.pos;
            if(HeadPos == Vector3.zero) HeadPos = current;
            Vector3 offset = current - HeadPos;
            if (offset.magnitude < minOffset)
            {
                HeadPos = face.pos;
                return;
            }
            if(Mathf.Abs(offset.y) > Mathf.Abs(offset.x))
            {
                if (offset.y > 0) Camera.transform.localEulerAngles += new Vector3(1, 0, 0) * rotateheadSpeed * Time.deltaTime;
                else
                {
                    Camera.transform.localEulerAngles -= new Vector3(1, 0, 0) * rotateheadSpeed * Time.deltaTime;
                }
            }
            else if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
            {
                if (offset.x > 0) Camera.transform.localEulerAngles += new Vector3(0,1, 0) * rotateheadSpeed * Time.deltaTime;
                else
                {
                    Camera.transform.localEulerAngles -= new Vector3(0,1, 0) * rotateheadSpeed * Time.deltaTime;
                }
            }
            //Camera.transform.localEulerAngles += new Vector3(Mathf.Abs(offset.y)> Mathf.Abs(offset.x) ?offset.y:0, Mathf.Abs(offset.x) > Mathf.Abs(offset.y) ? offset.x : 0, 0).normalized * rotateheadSpeed * Time.deltaTime;
            HeadPos = current;
        }
        else
        {
            HeadPos = face.pos;
            if(!setting.camera) Camera.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }
    public void CameraRotate2()
    {
        if(face.isFace && setting.camera)
        {
            HeadPos = face.pos;
            if (CurPos == Vector3.zero) CurPos = HeadPos;
            Vector3 offset = CurPos - HeadPos;
            if(Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
            {
                if (CurPos.x - HeadPos.x > minOffset)
                {
                    CurPos -= new Vector3(1, 0, 0) * rotateheadSpeed * 2 * Time.deltaTime;
                    Camera.transform.localEulerAngles -= new Vector3(0, 1, 0) * rotateheadSpeed * Time.deltaTime;
                }
                else if (CurPos.x - HeadPos.x < -minOffset)
                {
                    CurPos += new Vector3(1, 0, 0) * rotateheadSpeed * 2 * Time.deltaTime;
                    Camera.transform.localEulerAngles += new Vector3(0, 1, 0) * rotateheadSpeed * Time.deltaTime;
                }
                else
                {
                    CurPos = new Vector3(HeadPos.x, CurPos.y, 0);
                }
                CurPos = new Vector3(CurPos.x, HeadPos.y, 0);
            }
            else
            {
                if (CurPos.y - HeadPos.y > minOffset/2f)
                {
                    CurPos -= new Vector3(0, 1, 0) * rotateheadSpeed * Time.deltaTime;
                    Camera.transform.localEulerAngles -= new Vector3(1, 0, 0) * rotateheadSpeed * Time.deltaTime;
                }
                else if (CurPos.y - HeadPos.y < -minOffset/2f)
                {
                    CurPos += new Vector3(0, 1, 0) * rotateheadSpeed * Time.deltaTime;
                    Camera.transform.localEulerAngles += new Vector3(1, 0, 0) * rotateheadSpeed * Time.deltaTime;
                }
                else
                {
                    CurPos = new Vector3(CurPos.x, HeadPos.y, 0);
                }
                CurPos = new Vector3(HeadPos.x, CurPos.y, 0);
            }
        }
        else
        {
            HeadPos = face.pos;
            CurPos = HeadPos;
            if (!setting.camera) Camera.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }
}
