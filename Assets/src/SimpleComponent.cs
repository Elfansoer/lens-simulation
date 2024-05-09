using System;
using UnityEngine;

public class LineRenderComponent : MonoBehaviour {
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
}
