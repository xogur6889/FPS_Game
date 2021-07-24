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
    
        [Tooltip("캐릭터 컨트롤러")] private CharacterController characterController;
        [Tooltip("캐릭터 이동 벡터")] private Vector3 playerVelocity = Vector3.zero;

        [Tooltip("캐릭터 애니메이터")] private Animator animator;
        
        [Tooltip("캐릭터 애니메이터 움직임(Idle, Run Forward) 상태 변수")] private static readonly int AnimationFloatValueMovement = Animator.StringToHash("movement");

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            GetComponents();
        }
    
        private void GetComponents()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (playerVelocity.y < 0)
            {
                playerVelocity.y = 0.0f;
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            Vector3 move = new Vector3(horizontal, 0, vertical);
            characterController.Move(move * (Time.deltaTime * walkSpeed));

            animator.SetFloat(AnimationFloatValueMovement, move.magnitude);

            if (Input.GetButtonDown("Jump") && playerVelocity.y <= 0)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * (Physics.gravity.y * gravityMultiplier));
            }

            playerVelocity.y += (Physics.gravity.y * gravityMultiplier) * Time.deltaTime;
            characterController.Move(playerVelocity * Time.deltaTime);
        }
    }
}