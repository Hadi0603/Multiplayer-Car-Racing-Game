using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    internal enum driveType
    {
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }
    [Header("Specs")]
    [SerializeField] driveType drive;
    internal enum gearBox
    {
        automatic,
        manual
    }
    [SerializeField] private gearBox gearChange;
    public GameManager manager;
    [SerializeField] float handBrakeFrictionMultiplier;
    [SerializeField] public float totalPower;
    [SerializeField] float maxRPM;
    [SerializeField] float minRPM;
    [SerializeField] float engineRpm;
    [SerializeField] float smoothTime;
    [SerializeField] float[] gears;
    [SerializeField] public int gearNum;
    [SerializeField] float wheelsRPM;
    [SerializeField] public bool reverse;
    [SerializeField] AnimationCurve enginePower;
    private InputManager IM;
    private GameObject wheelMeshes;
    private GameObject wheelColliders;
    private WheelCollider[] wheels = new WheelCollider[4];
    private GameObject[] wheelMesh = new GameObject[4];
    [SerializeField] Transform centerOfMass;
    [SerializeField] float steeringMax;
    [SerializeField] float radius;
    [SerializeField] float downForceValue;
    [SerializeField] float brakePower;
    [SerializeField] float thrust;
    public float KPH;
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    [Header("Debug")]
    [SerializeField] float[] slip = new float[4];
    private Rigidbody rigidbody;
    void Start()
    {
        GetObjects();
    }

    private void FixedUpdate()
    {
        AddDownForce();
        AnimateWheels();
        MoveVehicle();
        SteerVehicle();
        GetFriction();
        CalculateEnginePower();
        Shifter();
        AdjustTraction();
    }
    void CalculateEnginePower()
    {
        WheelRPM();
        totalPower = enginePower.Evaluate(engineRpm) * (gears[gearNum]) * IM.vertical;
        float velocity = 0.0f;
        engineRpm = Mathf.SmoothDamp(engineRpm, 1000 + (Mathf.Abs(wheelsRPM) * 3.6f * (gears[gearNum])), ref velocity, smoothTime);
    }
    void WheelRPM()
    {
        float sum = 0;
        int R = 0;
        for(int i = 0; i < 4; i++)
        {
            sum += wheels[i].rpm;
            R++;
        }
        wheelsRPM = (R != 0) ? sum / R : 0;
        if (wheelsRPM < 0 && !reverse)
        {
            reverse = true;
            manager.ChangeGear();
        }
        else if(wheelsRPM > 0 && reverse)
        {
            reverse = false;
            manager.ChangeGear();
        }
    }
    void Shifter()
    {
        if (!IsGrounded()) return;
        if (gearChange == gearBox.automatic)
        {
            if (engineRpm > maxRPM && gearNum < gears.Length - 1 && !reverse)
            {
                gearNum++;
                manager.ChangeGear();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E) && gearNum < gears.Length - 1)
            {
                gearNum++;
                manager.ChangeGear();
            }
        }
        if (engineRpm < minRPM && gearNum > 0)
        {
            gearNum--;
            manager.ChangeGear();
        }
    }
    bool IsGrounded()
    {
        if (wheels[0].isGrounded && wheels[1].isGrounded && wheels[2].isGrounded && wheels[3].isGrounded)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    void MoveVehicle()
    {
        if (IM.boosting)
        {
            totalPower += 2000f;
        }
        if (drive == driveType.allWheelDrive)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = totalPower / 4;
            }
        }
        else if (drive == driveType.rearWheelDrive)
        {
            for (int i = 2; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = totalPower / 2;
            }
        }
        else
        {
            for (int i = 0; i < wheels.Length - 2; i++)
            {
                wheels[i].motorTorque = totalPower / 2;
            }
        }
        if (IM.handbrake)
        {
            for (int i = 2; i < wheels.Length; i++)  // Rear wheels only
            {
                wheels[i].brakeTorque = Mathf.Infinity;  // Lock the rear wheels
            }
        }
        else
        {
            for (int i = 2; i < wheels.Length; i++)
            {
                wheels[i].brakeTorque = 0;  // Release the brakes when handbrake is not pressed
            }
        }
        KPH = rigidbody.velocity.magnitude * 3.6f;
        
    }
    void SteerVehicle()
    {
        if (IM.horizontal > 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * IM.horizontal;
        }
        else if(IM.horizontal < 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * IM.horizontal;
        }
        else
        {
            wheels[0].steerAngle = 0;
            wheels[1].steerAngle = 0;
        }
    }
    void AnimateWheels()
    {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;
        for(int i = 0; i < 4; i++)
        {
            wheels[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelMesh[i].transform.position = wheelPosition;
            wheelMesh[i].transform.rotation = wheelRotation;
        }
    }
    void GetObjects()
    {
        IM = GetComponent<InputManager>();
        rigidbody = GetComponent<Rigidbody>();
        centerOfMass = transform.Find("CenterOfMass");
        rigidbody.centerOfMass = centerOfMass.transform.localPosition;
        wheelColliders = GameObject.Find("WheelColliders");
        wheelMeshes = GameObject.Find("WheelMeshes");
        wheels[0] = wheelColliders.transform.Find("0").gameObject.GetComponent<WheelCollider>();
        wheels[1] = wheelColliders.transform.Find("1").gameObject.GetComponent<WheelCollider>();
        wheels[2] = wheelColliders.transform.Find("2").gameObject.GetComponent<WheelCollider>();
        wheels[3] = wheelColliders.transform.Find("3").gameObject.GetComponent<WheelCollider>();
        wheelMesh[0] = wheelMeshes.transform.Find("0").gameObject;
        wheelMesh[1] = wheelMeshes.transform.Find("1").gameObject;
        wheelMesh[2] = wheelMeshes.transform.Find("2").gameObject;
        wheelMesh[3] = wheelMeshes.transform.Find("3").gameObject;
    }
    void AddDownForce()
    {
        rigidbody.AddForce(-transform.up * downForceValue * rigidbody.velocity.magnitude);
    }
    void GetFriction()
    {
        for(int i = 0; i < wheels.Length; i++)
        {
            WheelHit wheelHit;
            wheels[i].GetGroundHit(out wheelHit);
            slip[i] = wheelHit.forwardSlip;
        }
    }
    private float driftFactor;

    private void AdjustTraction()
    {
        //tine it takes to go from normal drive to drift 
        float driftSmothFactor = .7f * Time.deltaTime;

        if (IM.handbrake)
        {
            sidewaysFriction = wheels[0].sidewaysFriction;
            forwardFriction = wheels[0].forwardFriction;

            float velocity = 0;
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =
                Mathf.SmoothDamp(forwardFriction.asymptoteValue, driftFactor * handBrakeFrictionMultiplier, ref velocity, driftSmothFactor);

            for (int i = 0; i < 4; i++)
            {
                wheels[i].sidewaysFriction = sidewaysFriction;
                wheels[i].forwardFriction = forwardFriction;
            }

            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue = 1.1f;
            //extra grip for the front wheels
            for (int i = 0; i < 2; i++)
            {
                wheels[i].sidewaysFriction = sidewaysFriction;
                wheels[i].forwardFriction = forwardFriction;
            }
        }
        //executed when handbrake is being held
        else
        {

            forwardFriction = wheels[0].forwardFriction;
            sidewaysFriction = wheels[0].sidewaysFriction;

            forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue =
                ((KPH * handBrakeFrictionMultiplier) / 300) + 1;

            for (int i = 0; i < 4; i++)
            {
                wheels[i].forwardFriction = forwardFriction;
                wheels[i].sidewaysFriction = sidewaysFriction;

            }
        }

        //checks the amount of slip to control the drift
        for (int i = 2; i < 4; i++)
        {

            WheelHit wheelHit;

            wheels[i].GetGroundHit(out wheelHit);

            if (wheelHit.sidewaysSlip < 0) driftFactor = (1 + -IM.horizontal) * Mathf.Abs(wheelHit.sidewaysSlip);

            if (wheelHit.sidewaysSlip > 0) driftFactor = (1 + IM.horizontal) * Mathf.Abs(wheelHit.sidewaysSlip);
        }

    }

    private IEnumerator timedLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(.7f);
            radius = 6 + KPH / 20;

        }
    }

}
