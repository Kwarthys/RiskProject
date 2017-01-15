using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TankDriver : MonoBehaviour{

    public int torque = 0;
    // Use this for initialization
    private Transform gun, turret;

    private List<WheelCollider> wheels = new List<WheelCollider>();

    private float GUN_MAXROTATE = 65, GUN_MINROTATE = 350;
    private float m = -1;

	void Start () {
        registerParts();
        registerWheels();
    }

    void FixedUpdate()
    {
        forward();
    }
	
	// Update is called once per frame
	void Update () {

        
        turret.Rotate(new Vector3(0,0,0.2f));

        float gr = gun.localEulerAngles.x;

        if ((int)gr == 65) m = -1;
        if ((int)gr == 350) m = 1;

        gun.Rotate(new Vector3(m*0.2f, 0, 0));
        
	}

    private void registerWheels()
    {
        wheels.Clear();
        wheels.Add(this.transform.Find("Wheels").Find("frontLeft").GetComponent<WheelCollider>());
        wheels.Add(this.transform.Find("Wheels").Find("frontRight").GetComponent<WheelCollider>());
        wheels.Add(this.transform.Find("Wheels").Find("backLeft").GetComponent<WheelCollider>());
        wheels.Add(this.transform.Find("Wheels").Find("backRight").GetComponent<WheelCollider>());
    }

    private void registerParts()
    {
        Transform Model = this.gameObject.transform.Find("tankModel");
        turret = Model.Find("Turret");
        gun = turret.Find("Gun");
    }

    private void forward()
    {
        foreach(WheelCollider w in wheels)
        {
            w.motorTorque = torque;
        }
    }
}


