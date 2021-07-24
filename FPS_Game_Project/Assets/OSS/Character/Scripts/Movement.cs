using UnityEngine;

namespace OSS.Character
{
    [RequireComponent(
        typeof(CharacterController), 
        typeof(Animator))
    ]
    public class Movement : MonoBehaviour
    {
        [Tooltip("캐릭터 걷는 속도"), SerializeField] private float walkSpeed = 2.0f;
        [Tooltip("캐릭터 뛰는 속도"), SerializeField] private float sprintSpeed = 4.0f;
        
        [Tooltip("점프 높이"), SerializeField] private float jumpHeight = 1.0f;
        [Tooltip("중력 배수, 기본 중력 = -9.8f"), SerializeField] private float gravityMultiplier = 1f;
        [Tooltip("점프 벡터")] private Vector3 jumpVelocity = Vector3.zero;
    
        
        [Tooltip("캐릭터 컨트롤러")] private CharacterController characterController;


        [Tooltip("플레이어 카메라")] private Camera playerCamera;
        [Tooltip("회전 속도"), SerializeField] private float rotationSpeed = 200.0f;
        [Tooltip("회전한 각도")] private Vector2 rotateValue = Vector2.zero;

        
        [Tooltip("캐릭터 애니메이터")] private Animator animator;
        [Tooltip("캐릭터 애니메이터 움직임(Idle, Run Forward) 상태 변수")] private static readonly int AnimationFloatValueMovement = Animator.StringToHash("movement");

        private void Start()
        {
            SetCursor();
            GetComponents();
        }
        
        /// <summary>
        /// 커서를 화면 중앙야 고정시키고, 안보이게 설정
        /// </summary>
        private void SetCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// 이 스크립트에서 사용되는 Component들을 가져와 변수 설정
        /// </summary>
        private void GetComponents()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            playerCamera = GetComponentInChildren<Camera>();
        }

        /// <summary>
        /// 캐릭터 이동
        /// </summary>
        private void Update()
        {
            // 회전
            Rotation(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // 이동
            Vector3 moveVector = Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            // 캐릭터 이동량에 따른 애니메이션 전환 (Idle, Run)
            animator.SetFloat(AnimationFloatValueMovement, moveVector.magnitude);   //TODO (sungsikoh) : 별도의 클래스에서 애니메이션 제어 

            Jump();
        }

        /// <summary>
        ///  점프
        /// </summary>
        private void Jump()
        {
            // 속도롤 사용하기 때문에 0보다 작을 수 없음.
            if (jumpVelocity.y < 0) jumpVelocity.y = 0.0f;

            // 점프 키가 입력 되었고, 점프의 velocity가 0보다 작거나 같을 때만 점프 함.
            if (Input.GetButtonDown("Jump") && jumpVelocity.y <= 0)
            {
                // jumpVelocity의 y값에 양수를 주어 뛰어 오드로록 함.
                jumpVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * (Physics.gravity.y * gravityMultiplier));
            }

            // 중력에 의해 추락하도록 함.
            jumpVelocity.y += (Physics.gravity.y * gravityMultiplier) * Time.deltaTime;
            characterController.Move(jumpVelocity * Time.deltaTime);
        }

        /// <summary>
        /// 캐릭터 이동
        /// </summary>
        /// <param name="horizontal">수평축 이동 방향 ±1</param>
        /// <param name="vertical">수직축 이동 방향 ±1</param>
        /// <returns> 이동 벡터 </returns>
        private Vector3 Move(in float horizontal, in float vertical)
        {
            Vector3 move = new Vector3(horizontal, 0, vertical); // 로컬 좌표 이동 방향 벡터
            move = playerCamera.transform.TransformDirection(move); // 캐릭터 카메라를 기준으로 월드 좌표 이동 방향 벡터 계산
            move.Normalize(); // 월드 좌표 이동 방향 벡터 정규화

            // 걷는 상태화 달리는 상태를 구분하여 이동량 계산.
            if (Input.GetKey(KeyCode.LeftShift)) move *= sprintSpeed * Time.deltaTime;
            else move *= walkSpeed * Time.deltaTime;

            // 캐릭터 이동
            characterController.Move(move);
            
            return move;
        }

        /// <summary>
        /// 캐릭터 회전
        /// </summary>
        /// <param name="mouseX">마우스 x축 이동량</param>
        /// <param name="mouseY">마우스 y축 이동량</param>
        private void Rotation(in float mouseX, in float mouseY)
        {
            // 마우스 이동량과 회전 속도, deltaTime을 곱하여 현재 프레임에 회전해야 할 회전각을 구함
            // 기존 화전각과 구해진 회전각을 합하여 총 회전각을 구함
            rotateValue.x += rotationSpeed * mouseY * Time.deltaTime; // 총 x축 회전각    
            rotateValue.y += rotationSpeed * mouseX * Time.deltaTime; // 총 y축 회전각

            // 고개를 너무 들거나, 숙이지 않도록 x축 회전각을 제한함 ( 범위 ±80.0f )
            rotateValue.x = Mathf.Clamp(rotateValue.x, -80.0f, 80.0f);

            // 구해진 회전각으로 회전
            this.gameObject.transform.eulerAngles = new Vector3(rotateValue.x, rotateValue.y, 0.0f);
        }
    }
}