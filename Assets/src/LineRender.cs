using UnityEngine;
using System.Collections.Generic;

class LineRender {

    float length;
    LineRenderer renderer;
    Transform transform;
    Ray ray;

    public void Create( GameObject o ) {
        // get LineRenderer
        renderer = o.GetComponent<LineRenderer>();
        if (!renderer) {
            renderer = o.AddComponent<LineRenderer>();
        }
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        transform = renderer.transform;
        ray = new Ray(transform.position,transform.forward);

        // Init Ray
        length = 10;

        // Set renderer
        renderer.startWidth = 0.1f;
        renderer.endWidth = 0.1f;

        Render();
    }

    public void SetColor( Color color ) {
        renderer.startColor = color;
        renderer.endColor = color;
    }

    public void Render() {
        // set up ray
        ray.origin = transform.position;
        ray.direction = transform.forward;

        // set up points
        Vector3[] linePositions;

        // test colliding with any object
        if (Physics.Raycast(ray, out var hitInfo, length)) {
            var pos = new List<Vector3>();
            pos.Add(transform.position);

            var lens = hitInfo.collider.gameObject.GetComponent<CubeLensComponent>();
            if (lens!=null) {
                // get ray traces inside lens
                var rays = lens.lens.Interact( ray, hitInfo, 1, 0.1f );

                // add them to positions
                foreach( var innerRay in rays ) {
                    pos.Add( innerRay.origin );
                }

                // add exit ray
                var lastRay = rays[rays.Count-1]; 
                pos.Add( lastRay.GetPoint( length ) );
            }

            linePositions = pos.ToArray();
        } else {
            // hits nothing, just 2 pos
            linePositions = new Vector3[2];
            linePositions[0] = transform.position;
            linePositions[1] = transform.position + transform.forward * length;
        }

        renderer.positionCount = linePositions.Length;
        renderer.SetPositions(linePositions);
    }
}