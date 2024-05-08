using System;
using UnityEngine;

public class SimpleComponent : MonoBehaviour {
    LineRender l;
    // Start is called before the first frame update
    void Start() {
        l = new LineRender();
        l.Create( gameObject );
    }

    // Update is called once per frame
    void Update() {

        var time = Time.time;
        var hue = (float) (time - Math.Floor(time));
        var color = Color.HSVToRGB(hue,0.5f,0.5f);
        l.SetColor( color );
        l.Render();
    }

    void FixedUpdate() {
        SimpleMoveLogic();
    }

    void SimpleMoveLogic() {
        var direction = new Vector3();
        if (Input.GetKey(KeyCode.UpArrow)) direction += new Vector3(0,1,0);
        if (Input.GetKey(KeyCode.DownArrow)) direction += new Vector3(0,-1,0);
        if (Input.GetKey(KeyCode.LeftArrow)) direction += new Vector3(0,0,1);
        if (Input.GetKey(KeyCode.RightArrow)) direction += new Vector3(0,0,-1);

        transform.position += direction * 2f * Time.deltaTime;

        var rotationSpeed = 0.3f;
        var NSRotation = 0.0f;
        var WERotation = 0.0f;
        if (Input.GetKey(KeyCode.I)) NSRotation += -1;
        if (Input.GetKey(KeyCode.K)) NSRotation += 1;
        if (Input.GetKey(KeyCode.J)) WERotation += -1;
        if (Input.GetKey(KeyCode.L)) WERotation += 1;
        NSRotation *= rotationSpeed;
        WERotation *= rotationSpeed;

        transform.Rotate(NSRotation,WERotation,0);
    }
}
