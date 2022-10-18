using System.Collections.Generic;
using UnityEngine;
using Curve = Fujin.Utils.Math.Curve;

public class Platform : MonoBehaviour
{
    #region Variables

    public enum PathType 
    {
        Loop,
        Stop,
        Reverse,
    }

    public enum SmoothType
    {
        None,
        Smooth,
    }

    public float speed;
    public float delay;
    public bool alignRotation;
    public bool detectionFromScale;
    public Vector3 detectionExtents;
    public Vector3 detectionOffset;
    public float quality;
    public PathType pathType;
    public SmoothType smoothType;

    // Editor
    public bool show;
    public bool edit;
    public int toolbarSelection;

    // Instance
    float progress;
    bool goingForward = true;
    [SerializeField] Vector3[] waypoints;
    [SerializeField] Curve curve;
    List<Rigidbody> bodies = new List<Rigidbody>();

    #endregion Variables

    #region Engine

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoveAlongCurve();

        foreach (Rigidbody rb in bodies)
            AddRelativeVelocity(rb);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position + detectionOffset, 2*detectionExtents);
    }

    #endregion Engine

    #region Private Methods

    void MoveAlongCurve() 
    {
        //if (goingForward)
        //{
        //    progress += speed * Time.deltaTime / spline.Length;
        //    if (progress > 1f)
        //    {
        //        if (pathType == PathType.Stop)
        //        {
        //            progress = 1f;
        //        }
        //        else if (pathType == PathType.Loop)
        //        {
        //            progress -= 1f;
        //        }
        //        else
        //        {
        //            progress = 2f - progress;
        //            goingForward = false;
        //        }
        //    }
        //}
        //else
        //{
        //    progress -= speed * Time.deltaTime / spline.Length;
        //    if (progress < 0f)
        //    {
        //        progress = -progress;
        //        goingForward = true;
        //    }
        //}

        //Vector3 position = spline.GetPoint(progress);
        //transform.localPosition = position;
        //if (alignRotation)
        //{
        //    transform.LookAt(position + spline.GetDirection(progress));
        //}
    }

    void AddRelativeVelocity(Rigidbody rb) 
    {
    
    }

    #endregion Private Methods

    #region Public Methods

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

        curve.points = points;
    }

    public void Revert()
    {
        curve.points = new Vector3[] {
        new Vector3(0, 0f, 0f),
        new Vector3(0, 0f, 1f),
        new Vector3(0, 0f, 2f) };
    }

    #endregion Public Methods
}
