using UnityEngine;
using System.Collections.Generic;
using System;

class CubeLensComponent : MonoBehaviour {
    public CubeContainerLens lens;
    public int lensOptions = 0;

    List<GRINDistribution> distributions = new List<GRINDistribution>();

    void Awake() {
        CreateDistributions();
        int selectedDistribution = lensOptions%distributions.Count;
        lens = new CubeContainerLens( gameObject, distributions[selectedDistribution] );
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

        var radialSquared = new GRINDistribution{
            ValueFunc = (Vector3 relativePos)=>{
                var radius = 1.5f;
                var t = Mathf.Clamp(relativePos.magnitude/radius, 0, 1) - 1;
                var tt = t*t;

                return Mathf.Lerp( 1f, 2f, tt );
            },
            NormalFunc = (Vector3 relativePos)=>{
                return Vector3.Normalize(relativePos);
            }
        };

        distributions.Add( uniform );
        distributions.Add( cylindrical );
        distributions.Add( sphericalUniform );
        distributions.Add( sphericalRadial );
        distributions.Add( radialSquared );
    }

    public int LensType {
        get { return lensOptions; }
        set {
            lensOptions = value%distributions.Count;
            lens.DistributionFunction = distributions[lensOptions];
        }
    }
}