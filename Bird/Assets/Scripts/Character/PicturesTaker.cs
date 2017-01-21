using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(DepthOfField))]
public class PicturesTaker : MonoBehaviour
{
    #region Variables
    [Header("UI"), SerializeField]
    private Canvas m_CameraCanvas;

    [Header("Zoom"), SerializeField]
    private float m_ZoomIncrement;
    [SerializeField]
    private float m_ZoomSpeed;
    [SerializeField]
    private float m_MinimumFieldOfView;
    [SerializeField]
    private float m_MaximumFieldOfView;

    [Header("Other"), SerializeField]
    private Transform m_CameraRoot;

    private float m_DefaultFieldOfView;
    private float m_TargetFieldOfView;
    private float m_LastFieldOfView;

    private Vector3 m_CameraOffset;

    private bool m_IsCameraInUse;
    
    private Camera m_Camera;
    private DepthOfField m_DepthOfField;
    #endregion

    protected void Awake()
    {
        m_Camera = GetComponent<Camera>();
        m_DepthOfField = GetComponent<DepthOfField>();

        InitialiseFieldOfViewVariables();
    }

    private void InitialiseFieldOfViewVariables()
    {
        m_DefaultFieldOfView = m_Camera.fieldOfView;
        m_TargetFieldOfView = m_Camera.fieldOfView;
        m_LastFieldOfView = m_Camera.fieldOfView;
    }

    protected void Update()
    {
        CameraInputs();
        UpdateCamera();
    }

    private void CameraInputs()
    {
        if (Input.GetMouseButton(1))
        {
            m_IsCameraInUse = true;
        }
        else
        {
            m_IsCameraInUse = false;
        }
    }

    private void UpdateCamera()
    {
        if (m_IsCameraInUse)
        {
            CameraZoom();
            ActivateCameraUI();
            SetFieldOfViewToValue(m_LastFieldOfView);
        }
        else
        {
            DeactivateCameraUI();
            SetFieldOfViewToValue(m_DefaultFieldOfView);
        }
    }

    private void ActivateCameraUI()
    {
        m_CameraCanvas.enabled = true;
    }

    private void DeactivateCameraUI()
    {
        m_CameraCanvas.enabled = false;
    }

    #region FieldOfView
    private void CameraZoom()
    {
        if (m_IsCameraInUse)
        {
            if (Input.GetKey(KeyCode.E))
            {
                UpdateTargetFieldOfView(-m_ZoomIncrement);
            }
            else if (Input.GetKey(KeyCode.R))
            {
                UpdateTargetFieldOfView(m_ZoomIncrement);
            }

            UpdateCameraLastFieldOfView();
            UpdateCameraFieldOfView();
        }
    }

    private void UpdateCameraFieldOfView()
    {
        m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, m_TargetFieldOfView, Time.deltaTime * m_ZoomSpeed);

        m_Camera.fieldOfView = Mathf.Clamp(m_Camera.fieldOfView, m_MinimumFieldOfView, m_MaximumFieldOfView);
        m_TargetFieldOfView = Mathf.Clamp(m_TargetFieldOfView, m_MinimumFieldOfView, m_MaximumFieldOfView);
    }

    private void UpdateCameraLastFieldOfView()
    {
        m_LastFieldOfView = m_Camera.fieldOfView;
    }

    private void UpdateTargetFieldOfView(float a_Increment)
    {
        m_TargetFieldOfView += a_Increment;
    }

    private void SetFieldOfViewToValue(float a_Value)
    {
        m_Camera.fieldOfView = a_Value;
    }
    #endregion
}