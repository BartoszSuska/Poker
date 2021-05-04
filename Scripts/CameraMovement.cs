using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Networking;

namespace Com.BoarShroom.RPGtest
{
    public class CameraMovement : NetworkBehaviour
    {
        public float mouseSensitivity;
        float xRotation;
        float yRotation;
        float mouseX;
        float mouseY;
        Animator anim;

        [SerializeField] Transform head;
        [SerializeField] GameObject cam;

        void Awake()
        {
            anim = GetComponent<Animator>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            if(this.isLocalPlayer)
            {
                cam.SetActive(true);

                mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -70f, 70f);

                yRotation += mouseX;
                float maxY = 90f + transform.eulerAngles.y;
                float minY = -90f + transform.eulerAngles.y;

                yRotation = Mathf.Clamp(yRotation, minY, maxY);

                head.rotation = Quaternion.Euler(xRotation, yRotation, transform.rotation.z);

                float move = Input.GetAxis("Horizontal") * Time.deltaTime;
                //transform.Translate(transform.up * move);

                //transform.rotation = Quaternion.Euler(move * 90, transform.rotation.y, transform.rotation.z);

                //head.Rotate(transform.up * move * 90);
            }
            //head.Rotate(transform.up * 100 * Time.deltaTime);
        }
    }
}
