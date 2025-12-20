using Kamgam.BikeAndCharacter25D.Helpers;
using System;
using UnityEngine;

[Serializable]
public class BC_CameraFollow
{
    public Vector3 Offset = new Vector3(0f, 1f, -3.5f);

    /// <summary>
    /// X = max offset due to zoom on the Y-axis.\nY = max offset due to zoom on the Z-axis.
    /// </summary>
    public Vector2 SpeedZoomOffset = new Vector2(1f, 2f);

    /// <summary>
    /// X = min (at which the zooming starts), Y = max (at which the zooming is capped)
    /// </summary>
    public Vector2 SpeedZoomMinMaxVelocity = new Vector2(0, 13f);

    /// <summary>
    /// Used to verage the velocity of the objectToTrack. Using the velocity directly would be too "jittery".
    /// </summary>
    protected Vector2AverageQueue velocityAverage = new Vector2AverageQueue(30);

    protected Transform cameraToMove;
    protected Rigidbody2D objectToTrack;

    /// <summary>
    /// Sets the transform which will be moved (usually a camera).
    /// </summary>
    /// <param name="obj"></param>
    public void SetCameraToMove(Transform obj)
    {
        this.cameraToMove = obj;
    }

    /// <summary>
    /// Sets the object to track (usually a character).
    /// </summary>
    /// <param name="obj"></param>
    public void SetObjectToTrack(Rigidbody2D obj)
    {
        this.objectToTrack = obj;
    }

    public bool HasValidTargets()
    {
        return cameraToMove != null && cameraToMove.gameObject != null && objectToTrack != null && objectToTrack.gameObject != null;
    }

    public Vector3 offset;
    public void LateUpdate()
    {
        if (!HasValidTargets())
            return;

        // update average velocity
        velocityAverage.Enqueue(objectToTrack.velocity);
        velocityAverage.UpdateAverage();


        offset = Offset;
        float delta = SpeedZoomMinMaxVelocity.y - SpeedZoomMinMaxVelocity.x;
        float zoomFactor = (velocityAverage.Average().magnitude - SpeedZoomMinMaxVelocity.x) / delta;
        offset.y += zoomFactor * SpeedZoomOffset.x;
        offset.z -= zoomFactor * SpeedZoomOffset.y;

        // move camera
        if (objectToTrack != null && cameraToMove != null)
        {
            // use LateUpdate for tracking if the rigidbody is interpolating
            if (objectToTrack.interpolation != RigidbodyInterpolation2D.None)
                cameraToMove.position = objectToTrack.transform.position + offset;
        }
    }

    // Use this if you are not using interpolation on your physics objects.
    // Don't know what I am talking about? Then read this:
    // https://forum.unity.com/threads/camera-following-rigidbody.171343/#post-2491001
    /*
    Vector3 tmpPos = Vector3.zero;
    private void FixedUpdate()
    {
        if (objectToTrack != null && cameraToMove != null)
        {
            // use FixedUpdate for tracking if the rigidbody is not interpolating
            if (objectToTrack.interpolation == RigidbodyInterpolation2D.None)
            {
                tmpPos.x = objectToTrack.position.x;
                tmpPos.y = objectToTrack.position.y;
                tmpPos.z = objectToTrack.transform.position.z;

                cameraToMove.position = tmpPos + Offset;
            }
        }
    }*/
}
