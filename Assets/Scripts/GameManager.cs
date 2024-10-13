using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Controller RR;
    [SerializeField] Text speedo;
    [SerializeField] Text gearNum;
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
    public void ChangeGear()
    {
        gearNum.text = (!RR.reverse) ? (RR.gearNum + 1).ToString() : "R";
    }
}
