using UnityEngine;
using System.Collections;

public class TankDriver : MonoBehaviour{

    // Use this for initialization
    private Transform gun, turret;

	void Start () {
        turret = this.gameObject.transform.Find("Turret");
        gun = turret.Find("Gun");
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}


