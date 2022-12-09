using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineAI
{
    public class Ladder : MonoBehaviour
    {
        UISpline spline;
        public float angle;

        // Start is called before the first frame update
        void Start()
        {
            spline = gameObject.GetComponent<UISpline>();
            if (spline.points.Length != 2)
                enabled = false;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}