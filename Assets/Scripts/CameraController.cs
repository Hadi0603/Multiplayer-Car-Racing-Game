using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject child;
    [SerializeField] float speed;
    [SerializeField] float defaultFOV;
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
    }
    private void follow()
    {
        speed = Mathf.Lerp(speed, RR.KPH / 5, Time.deltaTime);
        gameObject.transform.position = Vector3.Lerp(transform.position, child.transform.position, Time.deltaTime * speed);
        gameObject.transform.LookAt(player.gameObject.transform.position);

    }
}
