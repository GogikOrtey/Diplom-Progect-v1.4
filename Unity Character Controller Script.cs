using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_01 : MonoBehaviour
{
    CharacterController characterController;

    public float Speed = 2;
    public float SpeedRot = 2;
    float Gravity = 20;
    private bool isRunning;
    private Vector3 moveDirection;

    public bool isOverWallGo = true; // Режим передвижения игрока. Если true, то автоматический

    float objSpeed = 2f; // Стандартная скорость
    const float suspendSpeed = 0.1f; // Стандартное замедление
    float minSpeed = 0.5f; // Минимальная скорость, во время замедления

    // Этот метод вызывается, если перед игроком препятствие, и ему нужно замедлиться
    public void PlayerControl_Suspend(float currSusSpeed = suspendSpeed) // Скорость замедления можно как передать в метод, так и нет
    {
        if (Speed > minSpeed)
        {
            Speed -= currSusSpeed;
        }
    }

    // Этот метод вызывается, если препятствие перед игроком находится угрожающе близко
    public void PlayerControl_EmergyStop()
    {
        Speed = 0;
        //print("Полная остановка");
    }

    // Этот метод вызывается, если впереди нет препятствий
    public void PlayerControl_StandartSpeedMove()
    {
        if (Speed < objSpeed)
        {
            if (Speed < minSpeed)
            {
                Speed = minSpeed;
            }
            else
            {
                Speed += suspendSpeed;
            }
        }
        else
        {
            Speed = objSpeed;
        }
    }

    // Этот метод вызывается, когда игроку нужно повернуться влево
    public void PlayerControl_TurnLeft()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y - SpeedRot, transform.rotation.eulerAngles.z);
    }

    // Этот метод вызывается, когда игроку нужно повернуться вправо
    public void PlayerControl_TurnRight()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + SpeedRot, transform.rotation.eulerAngles.z);
    }

    void Start()
    {
        TryGetComponent(out characterController);
    }

    void Update()
    {
        if (isOverWallGo == true)
        {
            OverWallGoMove();
        }
        else
        {
            StandartMove();
        }
    }

    // Стандартный режим управления персонажем, пользователем
    void StandartMove()
    { 
        Vector2 inputs = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), 1);
        isRunning = Input.GetKey(KeyCode.LeftShift);

        moveDirection = new Vector3(0, moveDirection.y, inputs.y * Speed);

        if ((Input.GetKey(KeyCode.D)) || (Input.GetKey(KeyCode.RightArrow)))
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + SpeedRot, transform.rotation.eulerAngles.z);
        }
        if ((Input.GetKey(KeyCode.A)) || (Input.GetKey(KeyCode.LeftArrow)))
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y - SpeedRot, transform.rotation.eulerAngles.z);
        }

        if (characterController.isGrounded)
        {
            moveDirection.y = 0;
            //if (Input.GetButtonDown("Jump"))
            //{
            //    moveDirection.y = JumpSpeed;
            //}
        }

        moveDirection.y -= Gravity * Time.deltaTime; 
        characterController.Move(transform.TransformVector(moveDirection) * Time.deltaTime);        
    }
    
    // Автоматический режим движения персонажа
    void OverWallGoMove()
    {
        moveDirection = new Vector3(0f * Speed, moveDirection.y, Speed);

        moveDirection.y -= Gravity * Time.deltaTime;
        characterController.Move(transform.TransformVector(moveDirection) * Time.deltaTime);
    }
}
