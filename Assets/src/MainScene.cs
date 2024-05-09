using UnityEngine;
using UnityEngine.SceneManagement;

class MainScene : MonoBehaviour {
    Camera cam;
    public GameObject lineGenerator;
    void Awake() {
        cam = Camera.main;
    }

    void FixedUpdate() {
        CameraMovement();
        LineRendererMovement();
    }

    void CameraMovement() {
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

    void LineRendererMovement() {
        if (lineGenerator==null) return;

        var direction = new Vector3();
        if (Input.GetKey(KeyCode.UpArrow)) direction += new Vector3(0,1,0);
        if (Input.GetKey(KeyCode.DownArrow)) direction += new Vector3(0,-1,0);
        if (Input.GetKey(KeyCode.LeftArrow)) direction += new Vector3(0,0,1);
        if (Input.GetKey(KeyCode.RightArrow)) direction += new Vector3(0,0,-1);

        lineGenerator.transform.position += direction * 2f * Time.deltaTime;

        var rotationSpeed = 0.3f;
        var NSRotation = 0.0f;
        var WERotation = 0.0f;
        if (Input.GetKey(KeyCode.I)) NSRotation += -1;
        if (Input.GetKey(KeyCode.K)) NSRotation += 1;
        if (Input.GetKey(KeyCode.J)) WERotation += -1;
        if (Input.GetKey(KeyCode.L)) WERotation += 1;
        NSRotation *= rotationSpeed;
        WERotation *= rotationSpeed;

        lineGenerator.transform.Rotate(NSRotation,WERotation,0);
    }

}