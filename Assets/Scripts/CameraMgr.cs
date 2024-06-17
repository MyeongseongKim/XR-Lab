using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraMgr : MonoBehaviour
{
    // public Camera _mainCamera;
    public Transform _mainCameraRig;

    [Range(0.1f, 100f)]
    public float _rotationSensitivity;
    [Range(0.1f, 100f)]
    public float _translationSensitivity;

    public bool _inertia;
    [Range(0f, 100f)]
    public float _drag;
    [Range(0f, 100f)]
    public float _angularDrag;
    public Vector3 _velocity;
    public Quaternion _angularVelocity;


    // Start is called before the first frame update
    void Start()
    {
        _velocity = Vector3.zero;
        _angularVelocity = Quaternion.identity;
    }

    // LatedUpdate is called once per frame after Update
    void LateUpdate()
    {
        _mainCameraRig.position += _velocity * Time.deltaTime;
        _mainCameraRig.rotation = 
            Quaternion.SlerpUnclamped(Quaternion.identity, _angularVelocity, Time.deltaTime) * _mainCameraRig.rotation;

        if (_inertia) {
            // Decreasing velocity
            _velocity *= Mathf.Clamp(1f - _drag * Time.deltaTime, 0f, 1f);
            if (_velocity.magnitude < 0.01f) {
                _velocity = Vector3.zero;
            }
            // Decresing angular velocity
            _angularVelocity = Quaternion.SlerpUnclamped(Quaternion.identity, _angularVelocity, Mathf.Clamp(1f - _angularDrag * Time.deltaTime, 0f, 1f));
            if (_angularVelocity.w < 0.01f) {
                _angularVelocity = Quaternion.identity;
            }
        }
        else {
            _velocity = Vector3.zero;
            _angularVelocity = Quaternion.identity;
        }
    }


    public void SetCameraTransfrom(Pose pose) {
        _mainCameraRig.position = pose.position;
        _mainCameraRig.rotation = pose.rotation;
    }

    public void CameraPanTilt(float pitch, float yaw, float roll) {
        Quaternion qRot = Quaternion.Euler(pitch, yaw, roll);
        if (_inertia) {
            _angularVelocity = 
                Quaternion.SlerpUnclamped(Quaternion.identity, qRot, _rotationSensitivity) * _angularVelocity;
        }
        else {
            _mainCameraRig.rotation = 
                Quaternion.SlerpUnclamped(Quaternion.identity, qRot, _rotationSensitivity) * _mainCameraRig.rotation;
        }
    }

    public void CameraDollyTrack(float truck, float jib, float dolly) {
        Vector3 heading = _mainCameraRig.transform.rotation * new Vector3(truck, jib, dolly);
        if (_inertia) {
            _velocity += _translationSensitivity * heading;
        }
        else {
            _mainCameraRig.position += _translationSensitivity * heading;
        }
    }

}