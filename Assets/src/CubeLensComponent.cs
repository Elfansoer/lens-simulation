using UnityEngine;
using System.Collections.Generic;

class CubeLensComponent : MonoBehaviour {
    public CubeContainerLens lens;
    public int lensOptions = 0;

    List<GRINDistribution> distributions = new List<GRINDistribution>();

    void Awake() {
        CreateDistributions();
        int selectedDistribution = lensOptions%distributions.Count;

        lens = new CubeContainerLens( gameObject, distributions[selectedDistribution] );
    }

    void Update() {
        // Switch lens
        if (Input.GetKeyDown(KeyCode.Alpha1)) lensOptions = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) lensOptions = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) lensOptions = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) lensOptions = 3;
        
        lens.DistributionFunction = distributions[lensOptions];
    }

    void CreateDistributions() {
        var uniform = new GRINDistribution{
            ValueFunc = (Vector3 relativePos)=>{
                return 2f;
            },
            NormalFunc = (Vector3 relativePos)=>{
                return Vector3.zero;
            }
        };

        var cylindrical = new GRINDistribution{
            ValueFunc = (Vector3 relativePos)=>{
                var radius = 1.5f;
                var axisGradient = new Vector3(1,0,0);
                var distanceFromAxis = Vector3.ProjectOnPlane( relativePos, axisGradient ).magnitude;
                return Mathf.Lerp( 2f, 1f, distanceFromAxis/radius );
            },
            NormalFunc = (Vector3 relativePos)=>{
                var axisGradient = new Vector3(1,0,0);
                return Vector3.Normalize( Vector3.ProjectOnPlane( relativePos, axisGradient ) );
            }
        };

        var sphericalUniform = new GRINDistribution{
            ValueFunc = (Vector3 relativePos)=>{
                var radius = 1.5f;
                if (relativePos.magnitude<radius) {
                    return 2f;
                } else {
                    return 1f;
                }
            },
            NormalFunc = (Vector3 relativePos)=>{
                var radius = 1.5f;
                if (relativePos.magnitude>radius) {
                    return Vector3.zero;
                } else {
                    return Vector3.Normalize(relativePos);
                }
            }
        };

        var sphericalRadial = new GRINDistribution{
            ValueFunc = (Vector3 relativePos)=>{
                var radius = 1.5f;
                return Mathf.Lerp( 2f, 1f, relativePos.magnitude/radius );
            },
            NormalFunc = (Vector3 relativePos)=>{
                return Vector3.Normalize(relativePos);
            }
        };

        distributions.Add( uniform );
        distributions.Add( cylindrical );
        distributions.Add( sphericalUniform );
        distributions.Add( sphericalRadial );
    }
}