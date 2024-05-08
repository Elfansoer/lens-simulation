using System;
using System.Collections.Generic;
using UnityEngine;

abstract class Lens {
    static public Vector3 Refraction( Vector3 incidentDir, Vector3 normal, float refIndex1, float refIndex2 ) {
        // same index, nothing to refract
        if (refIndex1==refIndex2) return incidentDir;

        // incident angle and normal angle must be more than 90 degrees
        if (Vector3.Dot( incidentDir, normal )>0) {
            normal = -normal;
        }

        var crossVector = Vector3.Cross( normal, -incidentDir );

        // get incident angle against normal
        var incidentSinAngle = crossVector.magnitude;

        // calculate refracted angle
        var refractedSinAgle = (refIndex1/refIndex2) * incidentSinAngle;

        // check for Total Internal Reflection
        if (refractedSinAgle>1) {
            // calculate reflected direction
            var reflectRotation = Quaternion.AngleAxis(-Mathf.Rad2Deg*Mathf.Asin(incidentSinAngle),Vector3.Normalize(crossVector));
            var reflectDir = reflectRotation * normal;
            return Vector3.Normalize(reflectDir);

        } else {
            // calculate refracted direction
            var refractRotation = Quaternion.AngleAxis(Mathf.Rad2Deg*Mathf.Asin(refractedSinAgle),Vector3.Normalize(crossVector));
            var refractDir = refractRotation * -normal;
            return Vector3.Normalize(refractDir);
        }
    }
}

abstract class GradientLens : Lens {
    protected bool isUniform;

    // Get gradient normal at a given position within the lens
    // Gradient normal is defined as the direction which reflective value difference is greatest
    // For example, a linear gradient would have its normal be the gradient direction
    // A radial gradient has normals equal to direction from center
    abstract public Vector3 GetGradientNormal( Vector3 position );

    // Get reflective index on a given position. Constant if uniform
    abstract public float GetGradientValue( Vector3 position );

    // Test whether this raycasthit event is a proper event of the light entering the lens
    // A lens may have many surface colliders, and not all collisions means entering the lens
    abstract public bool TestEnter( Ray light, RaycastHit info );

    // Test whether this ray segment actually exits the lens
    abstract public bool TestExit( Ray light, float ds, out RaycastHit info );

    public List<Ray> Interact( Ray incident, RaycastHit enterInfo, float outsideRefIndex, float ds ) {
        var rays = new List<Ray>();

        // add entrance point ray
        var enterRay = RefractFromRaycastHit( incident, enterInfo, outsideRefIndex, GetGradientValue( enterInfo.point ) );
        rays.Add( enterRay );

        // traverse in small steps
        var maxSegments = 100;
        var ctr = 0;
        while(ctr<maxSegments) {
            var lastRay = rays[rays.Count-1];

            // check if this step moves out of lens
            // TODO check if Raycast being inclusive at the surface
            if ( TestExit( lastRay, ds, out var info ) ) {
                var maybeExitRay = RefractFromRaycastHit( lastRay, info, GetGradientValue( info.point ), outsideRefIndex );

                rays.Add( maybeExitRay );

                // if this is NOT a reflection, break
                if (Vector3.Dot( maybeExitRay.direction, info.normal )<0) {
                    break;
                }

            // still inside lens
            } else {

                // advance previous ray by ds to get next ray
                var nextRay = new Ray( lastRay.GetPoint( ds ), lastRay.direction );

                // calculate refracted ray
                var refractRay = RefractFromGradient( nextRay, ds );
                rays.Add(refractRay);
            }

            ctr++;
        }

        return rays;
    }

    public Ray RefractFromGradient( Ray incidentRay, float ds ) {
        if (isUniform) return incidentRay;

        // get normal
        var normal = GetGradientNormal( incidentRay.origin );

        // if normal is zero vector, it is uniform
        if (normal.sqrMagnitude<0.05) return incidentRay;

        // by convention, normal should be larger than 90deg from incident angle
        if (Vector3.Dot(normal,incidentRay.direction)>0) {
            normal = -normal;
        }

        // get refractive index from both sides of the normal
        var refIndex1 = GetGradientValue( incidentRay.origin + normal * ds/2 );
        var refIndex2 = GetGradientValue( incidentRay.origin - normal * ds/2 );

        // refract
        var refractDir = Lens.Refraction( incidentRay.direction, normal, refIndex1, refIndex2 );
        return new Ray( incidentRay.origin, refractDir );
    }

    public Ray RefractFromRaycastHit( Ray incidentRay, RaycastHit info, float refIndex1, float refIndex2 ) {
        var refractDir = Lens.Refraction( incidentRay.direction, info.normal, refIndex1, refIndex2 );
        var refractRay = new Ray( info.point, refractDir );
        return refractRay;
    }
}

class CubeLens : GradientLens {
    public BoxCollider surfaceCollider;
    float refIndex;

    public CubeLens( GameObject gameObject, float refractionIndex) {
        // create new box collider
        surfaceCollider = gameObject.AddComponent<BoxCollider>();
        refIndex = refractionIndex;
        isUniform = true;
    }

    override public float GetGradientValue( Vector3 position ) {
        return refIndex;
    }

    override public Vector3 GetGradientNormal( Vector3 position ) {
        return Vector3.zero;
    }

    public override bool TestEnter(Ray light, RaycastHit info) {
        return info.collider==surfaceCollider;
    }

    public override bool TestExit(Ray light, float ds, out RaycastHit info) {
        // invert because its from inside
        light.origin = light.GetPoint( ds );
        light.direction = Vector3.Normalize(-light.direction);

        if (surfaceCollider.Raycast( light, out info, ds )) {
            info.normal = -info.normal;
            return true;
        } else {
            return false;
        }
    }
}

class CubeAxialLens : GradientLens {
    public BoxCollider surfaceCollider;
    float maxRefIndex;
    float minRefIndex;
    float radius;
    Vector3 axisGradient;

    public CubeAxialLens( GameObject gameObject, float minRefIndex, float maxRefIndex, float radius) {
        // create new box collider
        surfaceCollider = gameObject.AddComponent<BoxCollider>();
        isUniform = false;

        this.radius = radius;
        this.minRefIndex = minRefIndex;
        this.maxRefIndex = maxRefIndex;
        axisGradient = new Vector3(1,0,0);
    }

    override public float GetGradientValue( Vector3 position ) {
        var relativePos = position - GetCenter();
        var distanceFromAxis = Vector3.ProjectOnPlane( relativePos, axisGradient ).magnitude;

        return Mathf.Lerp( maxRefIndex, minRefIndex, distanceFromAxis/radius );
    }

    override public Vector3 GetGradientNormal( Vector3 position ) {
        var relativePos = position - GetCenter();
        return Vector3.Normalize( Vector3.ProjectOnPlane( relativePos, axisGradient ) );
    }

    Vector3 GetCenter() {
        return surfaceCollider.gameObject.transform.position;
    }

    public override bool TestEnter(Ray light, RaycastHit info) {
        return info.collider==surfaceCollider;
    }

    public override bool TestExit(Ray light, float ds, out RaycastHit info) {
        // invert because its from inside
        light.origin = light.GetPoint( ds );
        light.direction = Vector3.Normalize(-light.direction);

        if (surfaceCollider.Raycast( light, out info, ds )) {
            info.normal = -info.normal;
            return true;
        } else {
            return false;
        }
    }
}

class CubeSphericalLens : GradientLens {
    public BoxCollider surfaceCollider;
    float refIndex;
    float radius;

    public CubeSphericalLens( GameObject gameObject, float refractionIndex, float radius) {
        // create new box collider
        surfaceCollider = gameObject.AddComponent<BoxCollider>();
        isUniform = false;

        refIndex = refractionIndex;
        this.radius = radius;
    }

    override public float GetGradientValue( Vector3 position ) {
        var relativePos = position - GetCenter();
        if (relativePos.magnitude<radius) {
            return refIndex;
        } else {
            return 1;
        }
    }

    override public Vector3 GetGradientNormal( Vector3 position ) {
        var relativePos = position - GetCenter();
        if (relativePos.magnitude>radius) {
            return Vector3.zero;
        } else {
            return Vector3.Normalize(relativePos);
        }
    }

    public override bool TestEnter(Ray light, RaycastHit info) {
        return info.collider==surfaceCollider;
    }
    Vector3 GetCenter() {
        return surfaceCollider.gameObject.transform.position;
    }

    public override bool TestExit(Ray light, float ds, out RaycastHit info) {
        // invert because its from inside
        light.origin = light.GetPoint( ds );
        light.direction = Vector3.Normalize(-light.direction);

        if (surfaceCollider.Raycast( light, out info, ds )) {
            info.normal = -info.normal;
            return true;
        } else {
            return false;
        }
    }
}

class CubeSphericalRadialLens : GradientLens {
    public BoxCollider surfaceCollider;
    float minRefIndex;
    float maxRefIndex;
    float radius;

    public CubeSphericalRadialLens( GameObject gameObject, float minRefIndex, float maxRefIndex, float radius) {
        // create new box collider
        surfaceCollider = gameObject.AddComponent<BoxCollider>();
        isUniform = false;

        this.minRefIndex = minRefIndex;
        this.maxRefIndex = maxRefIndex;
        this.radius = radius;
    }

    override public float GetGradientValue( Vector3 position ) {
        var relativePos = position - GetCenter();
        return Mathf.Lerp( maxRefIndex, minRefIndex, relativePos.magnitude/radius );
    }

    override public Vector3 GetGradientNormal( Vector3 position ) {
        var relativePos = position - GetCenter();
        return Vector3.Normalize(relativePos);
    }

    public override bool TestEnter(Ray light, RaycastHit info) {
        return info.collider==surfaceCollider;
    }
    Vector3 GetCenter() {
        return surfaceCollider.gameObject.transform.position;
    }

    public override bool TestExit(Ray light, float ds, out RaycastHit info) {
        // invert because its from inside
        light.origin = light.GetPoint( ds );
        light.direction = Vector3.Normalize(-light.direction);

        if (surfaceCollider.Raycast( light, out info, ds )) {
            info.normal = -info.normal;
            return true;
        } else {
            return false;
        }
    }
}

class GRINDistribution {
    public Func<Vector3, float> ValueFunc;
    public Func<Vector3, Vector3> NormalFunc;
}

class CubeCustomDistributionLens : GradientLens {
    public BoxCollider surfaceCollider;
    public GRINDistribution distFunc;

    public CubeCustomDistributionLens( GameObject gameObject, GRINDistribution distFunc) {
        // create new box collider
        surfaceCollider = gameObject.AddComponent<BoxCollider>();
        isUniform = false;

        this.distFunc = distFunc;
    }

    override public float GetGradientValue( Vector3 position ) {
        var relativePos = position - GetCenter();
        return distFunc.ValueFunc( relativePos );
    }

    override public Vector3 GetGradientNormal( Vector3 position ) {
        var relativePos = position - GetCenter();
        return distFunc.NormalFunc( relativePos );
    }

    public override bool TestEnter(Ray light, RaycastHit info) {
        return info.collider==surfaceCollider;
    }
    Vector3 GetCenter() {
        return surfaceCollider.gameObject.transform.position;
    }

    public override bool TestExit(Ray light, float ds, out RaycastHit info) {
        // invert because its from inside
        light.origin = light.GetPoint( ds );
        light.direction = Vector3.Normalize(-light.direction);

        if (surfaceCollider.Raycast( light, out info, ds )) {
            info.normal = -info.normal;
            return true;
        } else {
            return false;
        }
    }
}