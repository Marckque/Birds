using UnityEngine;
using System.Collections;

public class PlayerControllable : MonoBehaviour
{
    #region Variables
    [Header("CharacterController"), SerializeField]
    private CharacterController m_CharacterController;

    [Header("Camera"), SerializeField]
    private Transform m_Head;
    [SerializeField]
    private Camera m_Camera;
    [SerializeField]
    protected Transform m_CameraRoot;

    [Header("Movement"), SerializeField]
    private AnimationCurve m_AccelerationCurve;
    [SerializeField]
    private AnimationCurve m_DecelerationCurve;
    [SerializeField]
    private float m_RunVelocity = 1f;
    [SerializeField]
    private float m_CrouchVelocity = 1f;
    [SerializeField]
    private float m_FallSpeed = 1f;

    private bool m_Jump;
    private bool m_IsCrouching;
    private bool m_IsOnFloor;
    private bool m_CanJump = true;

    private float m_MaxVelocity = 1f;
    private Vector3 m_Gravity;
    private Vector3 m_CurrentInput;
    private Vector3 m_Direction;
    private Vector3 m_CurrentDirection;
    private Vector3 m_LastDirection;
    private AnimationCurve m_VelocityCurve;
    private float m_AccelerationTime;
    private float m_DecelerationTime;
    private float m_VelocityTime;
    private float m_JumpTime;

    public int CurrentManagerID { get; set; }
    public int PlayerID { get; set; }
    public Camera PlayerCamera { get { return m_Camera; } }
    public Transform PlayerCameraRoot { get { return m_CameraRoot; } }
    #endregion Variables

    protected void Update()
    {
        CheckInputs();
    }

    protected virtual void CheckInputs()
    {
        GetMovementInput();
    }

    /*
    protected void GetWeaponsInput()
    {
        ChangeWeapon();

        if (Input.GetMouseButton(0))
        {
            MainWeaponAction();
        }

        if (Input.GetMouseButton(1))
        {
            SecondaryWeaponAction();
        }
    }
    */

    /*
    #region Inventory
    private void InitialiseWeapons()
    {
        if (m_Weapons.Length == 0)
        {
            Debug.Log("A human should have a weapon."); // In the game at least.
        }
        else
        {
            m_CurrentWeapon = 0;

            foreach (Weapon weapon in m_Weapons)
            {
                weapon.GetCurrentPlayer(this);
            }
    
            UpdateWeaponModel();
        }
    }

    private void ChangeWeapon()
    {
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");

        // Not sure why I need to make sure it's different from 0... But else it keeps on switching from 0 to 1 and vice-versa... !
        if (mouseWheel != 0)
        {
            if (mouseWheel > 0)
            {
                if (m_CurrentWeapon < m_Weapons.Length - 1)
                {
                    m_CurrentWeapon++;
                }
                else
                {
                    m_CurrentWeapon = 0;
                }
            }
            else
            {
                if (m_CurrentWeapon > 0)
                {
                    m_CurrentWeapon--;
                }
                else
                {
                    m_CurrentWeapon = m_Weapons.Length - 1;
                }
            }

            UpdateWeaponModel();
        }
    }

    private void UpdateWeaponModel()
    {
        foreach (Weapon weapon in m_Weapons)
        {
            if (weapon != m_Weapons[m_CurrentWeapon])
            {
                weapon.gameObject.SetActive(false);
            }
            else
            {
                weapon.gameObject.SetActive(true);
            }
        }
    }

    private void MainWeaponAction()
    {
        m_Weapons[m_CurrentWeapon].CanShoot();
    }

    private void SecondaryWeaponAction()
    {
        // Secondary action
    }        
    #endregion Inventory
    */

    #region Movement
    private void GetMovementInput()
    {
        Gravity();

        m_IsCrouching = Input.GetKey(KeyCode.LeftShift);

        m_CurrentInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Movement();

        m_CharacterController.Move(m_Gravity.normalized * m_FallSpeed * Time.deltaTime);
        m_CharacterController.Move(m_Direction.normalized * m_MaxVelocity * Time.deltaTime);
    }

    protected void Movement()
    {
        if (m_CurrentInput != Vector3.zero)
        {
            m_DecelerationTime = 0;
            m_AccelerationTime += Time.deltaTime;
            m_CurrentDirection = transform.forward * m_CurrentInput.z + transform.right * m_CurrentInput.x;
            m_LastDirection = m_CurrentDirection;
        }
        else
        {
            m_AccelerationTime = 0;
            m_CurrentDirection = Vector3.zero;
            m_DecelerationTime += Time.deltaTime;
        }

        m_VelocityCurve = m_CurrentInput != Vector3.zero ? m_AccelerationCurve : m_DecelerationCurve;
        m_VelocityTime = m_AccelerationTime > 0 ? m_AccelerationTime : m_DecelerationTime;

        m_MaxVelocity = m_IsCrouching ? m_CrouchVelocity : m_RunVelocity;
        m_MaxVelocity = m_IsCrouching ? m_CrouchVelocity : m_MaxVelocity *= m_VelocityCurve.Evaluate(m_VelocityTime);

        m_Direction = m_IsCrouching ? m_CurrentDirection : m_LastDirection;
    }

    private void Gravity()
    {
        m_Gravity += Physics.gravity * Time.deltaTime;
    }

    private void OnFloor()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 0.5f))
        {
            m_IsOnFloor = true;
        }
        else
        {
            m_IsOnFloor = false;
        }
    }
    #endregion Movement
}
