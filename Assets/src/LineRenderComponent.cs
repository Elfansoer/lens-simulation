using System;
using UnityEngine;

public class LineRenderComponent : MonoBehaviour {
    LineRender l;
    public float length;
    // Start is called before the first frame update
    void Start() {
        l = new LineRender();
        l.Create( gameObject );
        Length = length;
    }

    // Update is called once per frame
    void Update() {
        var time = Time.time;
        var hue = (float) (time - Math.Floor(time));
        var color = Color.HSVToRGB(hue,0.5f,0.5f);
        l.SetColor( color );
        l.Render();
    }

    public float Length {
        get { return l.length; }
        set { l.length = value; }
    }
}
