using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(Bloom))]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(DepthOfField))]
[RequireComponent(typeof(VignetteAndChromaticAberration))]
public class PicturesTaker : MonoBehaviour
{
    #region Variables
    [Header("UI"), SerializeField]
    private Canvas m_CameraCanvas;

    [Header("Screenshot"), SerializeField]
    private string m_FileName;
    [SerializeField, Range(1, 4)]
    private int m_ScreenshotQuality;

    private int m_CurrentScreenshotIndex = 0;

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
    private VignetteAndChromaticAberration m_VignetteAndChromaticAberration;
    private BloomOptimized m_Bloom;
    #endregion

    protected void Awake()
    {
        m_Camera = GetComponent<Camera>();

        GetImageEffects();
        InitialiseFieldOfViewVariables();
    }

    private void GetImageEffects()
    {
        m_DepthOfField = GetComponent<DepthOfField>();
        m_VignetteAndChromaticAberration = GetComponent<VignetteAndChromaticAberration>();
        m_Bloom = GetComponent<BloomOptimized>();
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

    protected void LateUpdate()
    {
        TakeScreenshot();
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
            
            SetFieldOfViewToValue(m_TargetFieldOfView);

            ActivateBloom();
            ActivateCameraUI();
            ActivateVignetteAndChromaticAberration();
        }
        else
        {
            DeactivateBloom();
            DeactivateCameraUI();
            DeactivateVignetteAndChromaticAberration();

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
        if (Input.GetAxis("Mouse ScrollWheel") < -0.01f)
        {
            UpdateTargetFieldOfView(m_ZoomIncrement);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0.01f)
        {
            UpdateTargetFieldOfView(-m_ZoomIncrement);
        }

        UpdateCameraLastFieldOfView();
        UpdateCameraFieldOfView();
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

    #region Chromatic aberration
    private void ActivateVignetteAndChromaticAberration()
    {
        m_VignetteAndChromaticAberration.enabled = true;
    }

    private void DeactivateVignetteAndChromaticAberration()
    {
        m_VignetteAndChromaticAberration.enabled = false;
    }
    #endregion

    private void ActivateBloom()
    {
        m_Bloom.enabled = true;
    }

    private void DeactivateBloom()
    {
        m_Bloom.enabled = false;
    }

    #region FocusTarget
    private void SetNewFocusTarget()
    {
        Ray ray = new Ray(transform.position, m_Camera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            m_DepthOfField.focalTransform = hit.transform;
        }
    }
    #endregion

    #region CapturePicture
    private void TakeScreenshot()
    {
        if (m_IsCameraInUse && Input.GetMouseButtonDown(0))
        {
            Application.CaptureScreenshot(m_FileName + m_CurrentScreenshotIndex.ToString() + ".png", m_ScreenshotQuality);

            if (File.Exists(m_FileName + m_CurrentScreenshotIndex.ToString()))
            {
                File.Move(Application.persistentDataPath, "/Screenshots/");
                m_CurrentScreenshotIndex++;
            }
        }
    }
    #endregion
}