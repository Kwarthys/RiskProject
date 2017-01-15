using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TankDriver : MonoBehaviour {

    public int[] torques = { 0, 0, 0, 0 };
    public bool brake = false;
    private bool braked = false;

    public Vector2 target = new Vector2(512, 512);

    // Use this for initialization
    private Transform gun, turret;

    private List<WheelCollider> wheels = new List<WheelCollider>();

    private float GUN_MAXROTATE = 65, GUN_MINROTATE = 350, STEERING_DEADZONE = 15;
    private float m = -1;

    private int state = 0;

    private Vector2 pos;

    /************* INITIALIZATION *************/

	void Start () {
        registerParts();
        registerWheels();
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

    /**************** UPDATES *************/

    void FixedUpdate()
    {
        //stateMachine();
        reachTarget();

        if (brake) brakeWheels();
        else freeWheels();
    }
	
	// Update is called once per frame
	void Update ()
    {
        pos.x = this.transform.position.x;
        pos.y = this.transform.position.z;

        turret.Rotate(new Vector3(0,0,0.2f));

        float gr = gun.localEulerAngles.x;

        if ((int)gr == 65) m = -1;
        if ((int)gr == 350) m = 1;

        gun.Rotate(new Vector3(m*0.2f, 0, 0));

    }

    /*************** MOVEMENT AI ***************/

    private void reachTarget()
    {
        float distanceToTarget = MyMaths.getDistance((int)pos.x, (int)pos.y, (int)target.x, (int)target.y);

        if (distanceToTarget < 5)
        {
            brakeWheels();
            print("Target Reached");
            return;
        }
        else { freeWheels(); }

        /**Calculating steering command**/

        float localAngle = this.transform.localEulerAngles.y > 180 ? this.transform.localEulerAngles.y - 360 : this.transform.localEulerAngles.y;
        float angleToTarget = MyMaths.getAngle((int)pos.x, (int)pos.y, (int)target.x, (int)target.y);
        float targetedAngle = angleToTarget - 90;
        float command = targetedAngle - localAngle; //VALIDATED COMMAND

        float MAX_STEERING_ANGLE = 60;

        goForward(distanceToTarget);
        wheels[0].steerAngle = Mathf.Clamp(command, -MAX_STEERING_ANGLE, MAX_STEERING_ANGLE);                        //frontLeft
        wheels[1].steerAngle = Mathf.Clamp(command, -MAX_STEERING_ANGLE, MAX_STEERING_ANGLE);                       //frontRight
        wheels[2].steerAngle = Mathf.Clamp(-command, -MAX_STEERING_ANGLE, MAX_STEERING_ANGLE);                      //backLeft
        wheels[3].steerAngle = Mathf.Clamp(-command, -MAX_STEERING_ANGLE, MAX_STEERING_ANGLE);                     //backRight

        /*
        wheels[0].motorTorque = 100 + 10 * sideToTurn; //frontLeft
        wheels[1].motorTorque = 100 + 10 * sideToTurn; //frontRight
        wheels[2].motorTorque = 100 + 10 * sideToTurn; //backLeft
        wheels[3].motorTorque = 100 + 10 * sideToTurn; //backRight
        */
    }
    private void stateMachine()
    {

        for(int i = 0; i < 4; i++)
        {
            wheels[i].motorTorque = torques[i];
        }
    }

    private void goForward(float command)
    {
        print("command : " + (-Mathf.Clamp(command * 10, 50, 1000)));
        foreach(WheelCollider w in wheels)
        {
            w.motorTorque = - Mathf.Clamp(command*10, 50, 1000);
        }
    }

    /************** WHEELS MANAGEMENT ***************/

    private void brakeWheels()
    {
        if (braked) return;
        foreach (WheelCollider w in wheels)
        {
            w.motorTorque = 0;
            w.brakeTorque = 1000;
        }
        braked = true;
    }
    
    private void freeWheels()
    {
        if (!braked) return;
        foreach (WheelCollider w in wheels)
        {
            w.brakeTorque = 0;
        }
        braked = false;
    }
}


