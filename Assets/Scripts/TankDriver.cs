using UnityEngine;
using System.Collections;

public class TankDriver : MonoBehaviour{

    // Use this for initialization
    private Transform gun, turret;

    private float GUN_MAXROTATE = 65, GUN_MINROTATE = 350;
    private float m = -1;

	void Start () {
        turret = this.gameObject.transform.Find("Turret");
        gun = turret.Find("Gun");
    }
	
	// Update is called once per frame
	void Update () {

        turret.Rotate(new Vector3(0,0,0.2f));

        float gr = gun.localEulerAngles.x;

        if ((int)gr == 65) m = -1;
        if ((int)gr == 350) m = 1;

        print("Gun Rotation on x : " + gr + " going : " + m);

        gun.Rotate(new Vector3(m*0.2f, 0, 0));

	
	}
}


