using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] InputReader inputReader;

    [Header("Settings")]
    [SerializeField] float moveSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] bool analogMovement;
    [SerializeField] float speedChangeRate = 10.0f;
    [SerializeField][Range(0.0f, 0.3f)] float rotationSmoothTime = .12f;

    [Header("Ground Check")]
    [SerializeField] float groundDistance = .08f;
    [SerializeField] LayerMask groundLayers;

    [Header("Jump Settings")]
    [SerializeField] float maxJumpHeight = 2f;

    private Animator _animator;
    private CharacterController _controller;
    private float _speed;
    private float _targetRotation = 0.0f;
    private Camera _mainCamera;
    private float _rotationVelocity;
    private bool _isRunning;
    private float _animationBlend;

    //jump variable
    private float _verticalVelocity;
    private bool _isJumping = false;
    private bool _isJumpPressed = false;
    private bool _readyToJump = true;
    private bool _isJumpAnimationComplete = false;

    static readonly int Speed = Animator.StringToHash("Speed");
    static readonly int IsJumping = Animator.StringToHash("IsPlayerJumping");
    static readonly int MotionSpeed = Animator.StringToHash("MotionSpeed");

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _mainCamera = Camera.main;

        if ( inputReader == null )
        {
            Debug.LogError("Input Reader not init");
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        inputReader.jumpEvent += OnJump;
        inputReader.runEvent += OnRun;
    }

    private void OnDisable()
    {
        inputReader.jumpEvent -= OnJump;
        inputReader.runEvent -= OnRun;
    }

    private void Update()
    {
        HandleMovement();
        HandleJump();
    }

    void OnJump()
    {
        if(IsGrounded() && _readyToJump)
        {
            _isJumpPressed = true;
        }
    }

    void OnRun(bool performed)
    {
        _isRunning = performed;
    }

    void HandleMovement()
    {
        float targetSpeed = _isRunning ? runSpeed : moveSpeed;

        if (inputReader.Direction == Vector2.zero) targetSpeed = 0.0f;

        Vector3 ccVelocity = _controller.velocity;
        float currentHorizontalSpeed = new Vector3(ccVelocity.x, 0.0f, ccVelocity.z).magnitude;

        float inputMagnitude = analogMovement ? inputReader.Direction.magnitude : 1f;
        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * speedChangeRate);

            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        Vector3 inputDir = new Vector3(inputReader.Direction.x, 0, inputReader.Direction.y).normalized;

        if (inputReader.Direction != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        _animator.SetFloat(Speed, _animationBlend);
        _animator.SetFloat(MotionSpeed, inputMagnitude);
    }

    void HandleJump()
    {
        if (_isJumpPressed && IsGrounded())
        {
            _verticalVelocity = Mathf.Sqrt(maxJumpHeight * -2f * Physics.gravity.y);
            _animator.SetBool(IsJumping,true); // Chuyển sang trạng thái Jump
            _isJumping = true;
            _isJumpPressed = false;
            _readyToJump = false;
            _isJumpAnimationComplete = false; // Reset biến theo dõi animation nhảy
        }

        if (_isJumping)
        {
            // Áp dụng trọng lực và cập nhật vị trí của CharacterController
            _verticalVelocity += Physics.gravity.y * Time.deltaTime;
            _controller.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
        }

        // Chuyển về trạng thái Locomotion khi nhân vật chạm đất và animation nhảy kết thúc
        if (IsGrounded() && _isJumpAnimationComplete)
        {
            _isJumping = false;
            _readyToJump = true;
        }
    }

    public void SetJumpAnimationComplete()
    {
        _isJumpAnimationComplete = true;
        _animator.SetBool(IsJumping, false);// Đặt IsJumping thành false trong Animator
    }

    bool IsGrounded()
    {
        return Physics.SphereCast(transform.position, groundDistance, Vector3.down, out _,groundDistance, groundLayers);
    }
}
