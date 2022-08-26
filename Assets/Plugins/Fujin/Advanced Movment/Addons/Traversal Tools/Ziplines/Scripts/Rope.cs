using System.Collections.Generic;
using UnityEngine;
using Fujin.Utils;

namespace Fujin.Addons
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Rope : MonoBehaviour
    {
        #region Variables

        public enum Orientation
        {
            Horizontal = 0,
            Vertical = 1
        }

        public bool straighten;
        public bool meshActive => meshRenderer.enabled;
        public float speed;
        public float wobble;
        public int sides;
        public float thickness;
        public int quality;
        public Orientation orientation;
        public Material mat;
        public Transform[] anchors;

        bool initialized;
        [SerializeField]
        Curve curve;
        Mesh mesh;
        MeshRenderer meshRenderer;
        List<Rigidbody> rigidbodies = new List<Rigidbody>();

        // Editor
        public bool show;
        public bool edit;
        public bool anchorFoldout;
        public int toolbarSelection;

        #endregion Variables

        #region Engine

        // Update is called once per frame
        void Update()
        {

            foreach (Rigidbody rb in rigidbodies)
            {
                Vector3 direction = GetPoint(1) - GetPoint(0);
                rb.velocity = direction * speed;
            }
        }

        #endregion Engine

        #region Private Methods

        void GenerateMesh()
        {
            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i <= quality; i++)
                vertices.Add(curve.GetPoint(i / (float)quality));

            MeshGenerator.GenerateRope(sides, thickness, vertices.ToArray(), mesh, new Vector2(100, 100));
        }

        void SetAnchors()
        {
            Quaternion rotation = orientation == Orientation.Vertical ?
               Quaternion.Euler(0, 0, 90) : Quaternion.identity;

            // set anchors
            if (anchors[0] != null)
            {
                anchors[0].rotation = rotation;
                anchors[0].position = GetPoint(0);
            }
            if (anchors[1] != null)
            {
                anchors[1].rotation = rotation;
                anchors[1].position = GetPoint(quality - 1);
            }
        }

        #endregion Private Methods

        #region Public Methods

        public void Init()
        {
            if (initialized)
                return;

            if (anchors.Length != 2)
            {
                anchors = new Transform[2];
            }

            if (mesh == null)
            {
                mesh = new Mesh();
                GetComponent<MeshFilter>().mesh = mesh;
                meshRenderer = GetComponent<MeshRenderer>();
            }

            UpdateFromCurve();
            initialized = true;
        }

        public void SetMaterial(Material mat)
        {
            this.mat = mat;
            meshRenderer.material = mat;
        }

        public Vector3 GetPoint(float t)
        {
            return transform.TransformPoint(curve.GetPoint(t));
        }

        public Vector3 GetHandle(int index)
        {
            return transform.TransformPoint(curve.points[index]);
        }

        public Vector3[] GetHandles()
        {
            return curve.points;
        }

        public void UpdateFromCurve()
        {
            GenerateMesh();
            SetAnchors();
        }

        public void UpdateCurve(Vector3[] points)
        {
            if (points.Length != curve.points.Length)
            {
                if (points.Length < curve.points.Length)
                    Debug.LogError("[ERROR] Too few curve points. Please check array length.");
                else
                    Debug.LogError("[ERROR] Too many curve points. Please check array length.");
                return;
            }

            if (orientation == Orientation.Vertical)
            {
                curve.points[2].x = curve.points[0].x;
                curve.points[2].z = curve.points[0].z;
                curve.Straighten();
            }

            curve.points = points;
            if (straighten)
                curve.Straighten();
            UpdateFromCurve();
        }

        public void FlipOrientation()
        {
            if (orientation == Orientation.Vertical)
            {
                curve.points[2].x = curve.points[0].x;
                curve.points[2].z = curve.points[0].z;
                curve.Straighten();
            }

            UpdateFromCurve();
        }

        public void Revert()
        {
            if (orientation == Orientation.Vertical)
            {
                curve.points = new Vector3[] {
            new Vector3(0, 2f, 0),
            new Vector3(0, 1f, 0),
            new Vector3(0, 0f, 0) };
            }
            else
            {
                curve.points = new Vector3[] {
            new Vector3(0, 0f, 0f),
            new Vector3(0, 0f, 1f),
            new Vector3(0, 0f, 2f) };
            }

            UpdateFromCurve();
        }

        public void ToggleMesh()
        {
            meshRenderer.enabled = !meshRenderer.enabled;
        }

        public void OnInteract(Rigidbody rigidbody)
        {
            rigidbody.position = GetPoint(0);
            rigidbodies.Add(rigidbody);
        }

        public void OnLeave(Rigidbody rigidbody)
        {
            rigidbodies.Remove(rigidbody);
        }

        #endregion Public Methods
    }
}
