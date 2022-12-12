using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineAI
{
    public class Ladder : MonoBehaviour
    {
        UISpline spline;
        public float angle;
        new BoxCollider collider;
        GameObject colliderObject;
        
        void Start()
        {
            
            spline = gameObject.GetComponent<UISpline>();
            if (spline.points.Length != 2)
                enabled = false;
            colliderObject = new GameObject("LadderCollider");
            colliderObject.transform.parent = gameObject.transform;
            colliderObject.tag = "Ladder";
            colliderObject.transform.position = new Vector3(spline.points[0].x, spline.points[0].y + ((spline.points[1].y - spline.points[0].y) / 2.0f), spline.points[0].z);
            colliderObject.transform.rotation = Quaternion.Euler(0, angle, 0);
            collider = colliderObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(3.0f, spline.points[1].y - spline.points[0].y, 0.01f);
            //collider.center = new Vector3(spline.points[0].x, spline.points[0].y + ((spline.points[1].y - spline.points[0].y) / 2.0f), spline.points[0].z);
            //collider.
        }
    }
}