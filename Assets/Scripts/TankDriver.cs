using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TankDriver : MonoBehaviour {

    public int[] torques = { 0, 0, 0, 0 };
    public bool brake = false;
    private bool braked = false;

    /**********Folowing FPSController************/
    public bool followFPS = false;
    public GameObject fps;    
    /********************************************/

    public Vector2 target = new Vector2(512, 512);

    // Use this for initialization
    private Transform gun, turret;

    private List<WheelCollider> wheels = new List<WheelCollider>();

    private float GUN_MAXROTATE = 65, GUN_MINROTATE = 350, STEERING_DEADZONE = 15;
    private float m = -1;

    private int state = 0;

    //private int torqueCommand = 0;
    private static int MAX_SPEED = 20;

    private Vector2 pos, oldPos;

    /************* INITIALIZATION *************/

	void Start () {
        registerParts();
        registerWheels();
        this.transform.GetComponent<Rigidbody>().centerOfMass = (this.transform.Find("CenterOfMass").localPosition);

        oldPos.x = pos.x;
        oldPos.y = pos.y;
        pos.x = this.transform.position.x;
        pos.y = this.transform.position.z;
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
        if(followFPS)
        {
            target.x = fps.transform.position.x;
            target.y = fps.transform.position.z;
        }

        oldPos.x = pos.x;
        oldPos.y = pos.y;
        pos.x = this.transform.position.x;
        pos.y = this.transform.position.z;

        //stateMachine();
        reachTarget();

        if (brake) brakeWheels();
        else freeWheels();
    }
	
	// Update is called once per frame
	void Update ()
    {

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

        float speed = MyMaths.getDistance(oldPos.x, oldPos.y, pos.x, pos.y) / Time.deltaTime;

        if (distanceToTarget < 5)
        {
            brakeWheels();
            //print("Target Reached");
            return;
        }
        else { freeWheels(); }

        /**Calculating steering command**/

        float localAngle = MyMaths.rescaleAngle(this.transform.localEulerAngles.y);
        float angleToTarget = MyMaths.getAngle((int)pos.x, (int)pos.y, (int)target.x, (int)target.y);
        float targetedAngle = angleToTarget - 90;
        float angleCommand = MyMaths.rescaleAngle(targetedAngle - localAngle);

        //print("AngleCommand " + angleCommand + " .targetedAngle " + targetedAngle + " .localAngle " + localAngle);


        float MAX_STEERING_ANGLE = 45;

        wheels[0].steerAngle = Mathf.Clamp(angleCommand, -MAX_STEERING_ANGLE, MAX_STEERING_ANGLE);                      //frontLeft
        wheels[1].steerAngle = Mathf.Clamp(angleCommand, -MAX_STEERING_ANGLE, MAX_STEERING_ANGLE);                      //frontRight
        wheels[2].steerAngle = Mathf.Clamp(-angleCommand, -MAX_STEERING_ANGLE, MAX_STEERING_ANGLE);                     //backLeft
        wheels[3].steerAngle = Mathf.Clamp(-angleCommand, -MAX_STEERING_ANGLE, MAX_STEERING_ANGLE);                     //backRight

        /** Calculating torque command **/

        float distCmd = distanceToTarget > MAX_SPEED ? MAX_SPEED : distanceToTarget-5;
        float torqueCommand = (distCmd - speed) * 80;

        //print("Torque Command " + torqueCommand + " .speed " + speed + " .distCmd " + distCmd);

        goForward(-torqueCommand);

    }

    private void goForward(float command)
    {
        foreach(WheelCollider w in wheels)
        {
            w.motorTorque = command;
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


