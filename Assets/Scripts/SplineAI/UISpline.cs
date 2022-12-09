using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineAI
{
    public class UISpline : MonoBehaviour
    {
        public Vector3[] points;

        public void Start()
        {
            raycastPointsToGround();
        }

        private void raycastPointsToGround()
        {
            for (int i = 0; i < points.Length && points.Length > 0; i++)
            {
                RaycastHit hit;

                if (Physics.Raycast(points[i], Vector3.down, out hit))
                    points[i].y -= hit.distance;
            }
        }

        private Vector3 offsetPos(Vector3 p)
        {
            float x = p.x;
            float y = p.y;
            float z = p.z;

            // radius
            float r = Mathf.Sqrt((x * x) + (z * z));

            // angle
            float a = Mathf.Atan2(z, x) - ((transform.parent.transform.eulerAngles.y) * (Mathf.PI / 180));

            x = r * Mathf.Cos(a);
            z = r * Mathf.Sin(a);

            return new Vector3(x, y, z);
        }

        private void OnDrawGizmos()
        {
            // loop to draw line from point to point in list
            Gizmos.color = Color.red;
            for (int i = 0; i < points.Length - 1; i++)
            {
                int iNext = (i + 1) % points.Length;
                Gizmos.DrawLine(transform.parent.transform.position + offsetPos(points[i]) + Vector3.up * 0.1f, transform.parent.transform.position + offsetPos(points[iNext]) + Vector3.up * 0.1f);
            }

            // loop to draw spheres from points in list
            Gizmos.color = Color.yellow;

            float nodeEditorSize = 1.0f;
            if (transform.parent != null && transform.parent.parent != null)
            {
                GameObject splineObject = transform.parent.parent.gameObject;
                if (splineObject.GetComponent<SplineManager>() != null)
                {
                    SplineManager splineManager = splineObject.GetComponent<SplineManager>();
                    nodeEditorSize = splineManager.nodeEditorSize;
                }
            }

            //float radius = 0.25f;
            for (int i = 0; i < points.Length; i++)
                Gizmos.DrawSphere(transform.parent.transform.position + offsetPos(points[i]), nodeEditorSize);
        }
    }
}
