using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject child;
    [SerializeField] float speed;
    [SerializeField] float defaultFOV;
    [SerializeField] float desiredFOV;
    [SerializeField]
    [Range(0f, 5f)] float smoothTime;
    private Controller RR;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        child = player.transform.Find("CameraConstraint").gameObject;
        RR = player.GetComponent<Controller>();
        defaultFOV = Camera.main.fieldOfView;
    }
    private void FixedUpdate()
    {
        follow();
        BoostFOV();
    }
    private void follow()
    {
        if (speed < 25)
        {
            speed = Mathf.Lerp(speed, RR.KPH / 5, Time.deltaTime);
        }
        else
        {
            speed = 25;
        }
        
        gameObject.transform.position = Vector3.Lerp(transform.position, child.transform.position, Time.deltaTime * speed);
        gameObject.transform.LookAt(player.gameObject.transform.position);
    }
    void BoostFOV()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, desiredFOV, Time.deltaTime * smoothTime);
        }
        else
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, defaultFOV, Time.deltaTime * smoothTime);
        }
    }
}
