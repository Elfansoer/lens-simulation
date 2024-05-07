using UnityEngine;

class MainScene : MonoBehaviour {
    Camera cam;
    void Awake() {
        cam = Camera.main;
    }

    void FixedUpdate() {
        // simple camera movement
        var camSpeed = 0.3f;
        var direction = new Vector3();
        if (Input.GetKey(KeyCode.W)) direction += new Vector3(0,1,0);
        if (Input.GetKey(KeyCode.S)) direction += new Vector3(0,-1,0);
        if (Input.GetKey(KeyCode.D)) direction += new Vector3(0,0,1);
        if (Input.GetKey(KeyCode.A)) direction += new Vector3(0,0,-1);
        if (Input.GetKey(KeyCode.Q)) direction += new Vector3(1,0,0);
        if (Input.GetKey(KeyCode.E)) direction += new Vector3(-1,0,0);
        direction *= camSpeed;

        cam.transform.position += cam.transform.up * direction.y;
        cam.transform.position += cam.transform.right * direction.z;
        cam.transform.RotateAround( cam.transform.position, Vector3.up, -direction.x*2 );
    }
}