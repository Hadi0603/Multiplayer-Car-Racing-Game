using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject child;
    [SerializeField] float speed;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        child = player.transform.Find("CameraConstraint").gameObject;
    }
    private void FixedUpdate()
    {
        follow();
    }
    private void follow()
    {
        gameObject.transform.position = Vector3.Lerp(transform.position, child.transform.position, Time.deltaTime * speed);
        gameObject.transform.LookAt(player.gameObject.transform.position);

    }
}
