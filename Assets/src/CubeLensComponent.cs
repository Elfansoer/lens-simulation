using UnityEngine;

class CubeLensComponent : MonoBehaviour {
    public GradientLens lens;
    public int lensOptions = 0;

    void Awake() {
        switch(lensOptions) {
            case 0: lens = new CubeLens( gameObject, 2f ); break;
            case 1: lens = new CubeAxialLens( gameObject, 1f, 2f, 1.5f ); break;
            case 2: lens = new CubeSphericalLens( gameObject, 2f, 1.5f ); break;
            case 3: lens = new CubeSphericalRadialLens( gameObject, 1f, 2f, 1.5f ); break;
            default: lens = new CubeLens( gameObject, 2f ); break;
        }
    }
}