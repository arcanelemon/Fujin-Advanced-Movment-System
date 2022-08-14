using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FujinRope : MonoBehaviour
{
    #region Variables

    public enum Type 
    {
        Horizontal = 0,
        Vertical = 1
    }

    public bool forward;
    public bool backward;
    public float speed;
    public float wobble;
    public int quality;
    public Type type;
    public Material mat;
    public Transform[] anchors;
    FujinCurve curve = new FujinCurve();

    List<Rigidbody> rigidbodies = new List<Rigidbody>();

    // Editor
    public bool edit;
    public bool show;
    public bool anchorFoldout;
    public int toolbarSelection;

    #endregion Variables

    #region Engine

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Rigidbody rb in rigidbodies) 
        {
            Vector3 direction = GetPoint(1) - GetPoint(0);
            rb.velocity = direction * speed;
        }
    }

    #endregion Engine

    #region Private Methods

    void UpdateFromCurve()
    {
        // stretch mesh

        // set anchors
        if (anchors[0] != null)
            anchors[0].position = GetPoint(0);
        if (anchors[1] != null)
            anchors[1].position = GetPoint(quality - 1);
    }

    void StretchMesh() 
    {
        
    }

    #endregion Private Methods

    #region Public Methods

    public void Init() 
    {
        if (anchors.Length == 0) 
        {
            anchors = new Transform[2];
        }
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

        if (type == Type.Vertical)
        {
            points[2].x = points[0].x;
            points[2].z = points[0].z;
            curve.Straighten();

            anchors[0].rotation = Quaternion.Euler(0, 0, 90);
            anchors[1].rotation = Quaternion.Euler(0, 0, 90);
        }

        curve.points = points;
        UpdateFromCurve();
    }

    public void FlipOrientation() 
    {
        Vector3[] points = curve.points;
        if (type == Type.Vertical)
        {
            points[2].x = points[0].x;
            points[2].z = points[0].z;
            Straighten();

            anchors[0].rotation = Quaternion.Euler(0, 0 , 90);
            anchors[1].rotation = Quaternion.Euler(0, 0, 90);
        } else 
        {
            anchors[0].rotation = Quaternion.identity;
            anchors[1].rotation = Quaternion.identity;
        }
    }

    public void Revert()
    {
        curve.Reset();
        UpdateFromCurve();
    }

    public void Straighten() 
    {
        curve.Straighten();
        UpdateFromCurve();
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
