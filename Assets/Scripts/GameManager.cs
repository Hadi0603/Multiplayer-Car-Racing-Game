using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Controller RR;
    [SerializeField] Text speedo;
    [SerializeField] float vehicleSpeed;
    // Start is called before the first frame update
    void Start()
    {
        speedo.text = "0";
    }

    private void FixedUpdate()
    {
        vehicleSpeed = RR.KPH;
        UpdateSpeed();
    }
    void UpdateSpeed()
    {
        speedo.text = vehicleSpeed.ToString("0");
    }
}
