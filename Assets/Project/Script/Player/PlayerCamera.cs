using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] InputReader inputReader;
    [SerializeField] bool lockCameraPosition = false;
    [SerializeField] bool isCurrentDeviceMouse = true;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float bottomClamp = -30.0f;
    [SerializeField] float topClamp = 70.0f;
    [SerializeField] GameObject cinemachineCameraTarget;
    [SerializeField] float cameraAngleOverride = 0.0f;
    [SerializeField] float mouseSensitivity = 1.0f;

    private const float _threshold = 0.01f;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private Vector3 _mouseInput;

    private void Update()
    {
        _mouseInput = inputReader.lookDirection;
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (_mouseInput.sqrMagnitude >= _threshold && !lockCameraPosition)
        {
            float deltaTimeMultiplier = isCurrentDeviceMouse ? mouseSensitivity : rotationSpeed * Time.deltaTime;

            _cinemachineTargetYaw += _mouseInput.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += -_mouseInput.y * deltaTimeMultiplier;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
