using UnityEngine;

public class PlayerMovement
{
    PlayerEntity player;
    WorldEntity world
    {
        get
        {
            return player.world;
        }
    }
    CollisionEntity collision
    {
        get
        {
            return player.collision;
        }
    }
    Transform transform
    {
        get
        {
            return player.transform;
        }
    }
    Animator anim
    {
        get
        {
            return player.anim;
        }
    }
    public bool isGrounded;//地面
    public bool isSprinting;//奔跑
    public bool isSquating = false;//蹲下
    public float frontWalkSpeed = 4f;//向前走路速度
    public float frontSprintSpeed = 7f;//向前跑步时速度
    public float horizontalWalkSpeed = 2.5f;//左右走路时速度
    public float horizontalSprintSeppd = 3f;//左右跑步时速度
    public float backWalkSpeed = 2.5f;//向后走路速度
    public float jumpForce = 8f;//跳跃力度
    public float gravity = -20f;//重力
    private float maxFallSpeed = -30f;//最大下落速度
    float squatSpeed = 0.5f;//蹲下时的速度倍率
    protected float verticalMomentum = 0;//垂直速度向量
    protected bool jumpRequest;//可以跳跃
    protected float horizontal;//左右移动量
    protected float vertical;//前后移动量
    protected Vector3 velocity;//速度矢量
    protected Vector3 nowVelocity;
    protected float groundDeceleration = 0.9f;
    protected float addVelocity = 0.25f;
    public PlayerMovement(PlayerEntity _player)
    {
        player = _player;
    }

    public virtual void Update()
    {
        GetPlayerInput();
    }
    public virtual void FixedUpdate()
    {
        CalculateVelocity();
    }

    private void CalculateVelocity()
    {
        XZMove();// 平移
        XZCollide();// 平移发生碰撞
        YMove();//上下移动
        YCollide();//上下发生碰撞
        Jump();
        ChunkVelocity();//处理阻力和加速度
        UpdateAnim();
    }
    protected virtual void UpdateAnim()
    {
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isSprinting", isSprinting);
        anim.SetBool("isSquating", isSquating);
        
        if (anim && isGrounded && velocity.sqrMagnitude > Mathf.Epsilon)
        {
            Vector3 forwardXZ = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
            Vector3 velocityXZ = new Vector3(velocity.x, 0, velocity.z);
            velocityXZ.Normalize(); // 归一化速度向量
            float angle = Vector3.SignedAngle(forwardXZ, velocityXZ, Vector3.up);
            // 转换为 [0, 360) 的范围
            if (angle < 0)
            {
                angle += 360;
            }
            int dirIndex = GetDirectionIndex(angle);
            string name = animName[dirIndex];
            foreach (string item in animName)
            {
                if (item == name)
                {
                    anim.SetBool(item, true);
                }
                else
                {
                    anim.SetBool(item, false);
                }
            }
        }
        else
        {
            foreach (string item in animName)
            {
                anim.SetBool(item, false);
            }

        }
        if(velocity.y > 0){
            anim.SetBool("isJumping", true);
            anim.SetBool("isFalling", false);
        }else if(velocity.y < 0){
            anim.SetBool("isJumping", false);
            anim.SetBool("isFalling", true);
        }else{
            anim.SetBool("isJumping", false);
            anim.SetBool("isFalling", false);
        }
    }
    string[] animName = new string[]{
        "Forward",
        "ForwardRight",
        "Right",
        "BackwardRight",
        "Backward",
        "BackwardLeft",
        "Left",
        "ForwardLeft"
    };
    private int GetDirectionIndex(float angle)
    {
        // 将360度划分为8个45度的扇区
        if (angle >= 337.5 || angle < 22.5) return 0;
        if (angle >= 22.5 && angle < 67.5) return 1;
        if (angle >= 67.5 && angle < 112.5) return 2;
        if (angle >= 112.5 && angle < 157.5) return 3;
        if (angle >= 157.5 && angle < 202.5) return 4;
        if (angle >= 202.5 && angle < 247.5) return 5;
        if (angle >= 247.5 && angle < 292.5) return 6;
        if (angle >= 292.5 && angle < 337.5) return 7;
        return 0; // 默认返回值，理论上不会到达这里
    }

    protected virtual void ChunkVelocity()
    {
        nowVelocity.y = velocity.y;
        if (nowVelocity.x != 0 && nowVelocity.y != 0 && velocity.x == 0 && velocity.y == 0)
        {
            nowVelocity.x *= groundDeceleration;
            nowVelocity.y *= groundDeceleration;
            if (nowVelocity.x * nowVelocity.x + nowVelocity.z * nowVelocity.z < 1)
            {
                nowVelocity.x = 0;
                nowVelocity.z = 0;
            }
        }
        else
        {
            if (Mathf.Abs(nowVelocity.x + velocity.x * addVelocity) > Mathf.Abs(velocity.x))
            {
                nowVelocity.x = velocity.x;
            }
            else
            {
                nowVelocity.x += velocity.x * addVelocity;
            }
            if (Mathf.Abs(nowVelocity.z + velocity.z * addVelocity) > Mathf.Abs(velocity.z))
            {
                nowVelocity.z = velocity.z;
            }
            else
            {
                nowVelocity.z += +velocity.z * addVelocity;
            }
        }
        player.PlayerItem.transform.Translate(nowVelocity, Space.World);
    }
    protected virtual void XZMove()
    {
        if (isSprinting)
        {
            velocity = (transform.forward * vertical * frontSprintSpeed + transform.right * horizontal * horizontalSprintSeppd)
            * Time.fixedDeltaTime;
        }
        else
        {
            if (vertical > 0)
            {
                velocity = (transform.forward * vertical * frontWalkSpeed + transform.right * horizontal * horizontalWalkSpeed)
                * Time.fixedDeltaTime;
            }
            else
            {
                velocity = (transform.forward * vertical * backWalkSpeed + transform.right * horizontal * horizontalWalkSpeed)
                * Time.fixedDeltaTime;
            }
        }
        if (isSquating)
        {
            velocity *= squatSpeed;
        }
    }
    protected virtual void XZCollide()
    {
        if (velocity == Vector3.zero)
        {
            return;
        }
        Vector3 tempVelocity = new Vector3(0, 0, velocity.z);
        if (collision.ChunkMoveXZ(world, CollisionEntity.moveFront, tempVelocity) || collision.ChunkMoveXZ(world, CollisionEntity.moveBack, tempVelocity))
        {
            velocity.z = 0;
        }
        tempVelocity.z = 0;
        tempVelocity.x = velocity.x;
        if (collision.ChunkMoveXZ(world, CollisionEntity.moveLeft, tempVelocity) || collision.ChunkMoveXZ(world, CollisionEntity.moveRight, tempVelocity))
        {
            velocity.x = 0;
        }
    }
    protected virtual void YMove()
    {
        //下落或者跳跃
        verticalMomentum += Time.fixedDeltaTime * gravity;
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;
    }
    protected virtual void YCollide()
    {
        if (velocity.y < 0)
        {
            float moveY = collision.ChunkDownSpeed(world, velocity.y);
            if (moveY > velocity.y)
            {
                velocity.y = moveY;
                isGrounded = true;
                verticalMomentum = 0;
            }
            if (velocity.y < maxFallSpeed)
            {
                velocity.y = maxFallSpeed;
            }
        }
        else if (velocity.y > 0)
        {
            float moveY = collision.ChunkUpSpeed(world, velocity.y);
            if (moveY < velocity.y)
            {
                velocity.y = moveY;
                verticalMomentum = 0;
            }
        }
    }
    protected virtual void Jump()
    {
        if (jumpRequest && isGrounded)
        {
            verticalMomentum = jumpForce;
            isGrounded = false;
        }
    }
    //获取玩家输入
    protected virtual void GetPlayerInput()
    {
        if (!player.inUI)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            JumpDown();
            SquatDown();
            SprintDown();
        }
        else
        {
            horizontal = 0;
            vertical = 0;
            jumpRequest = false;
            isSquating = false;
            isSquating = false;
        }
    }
    //判断是否跳跃
    protected virtual void JumpDown()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpRequest = false;
        }
    }
    //判断是否蹲下
    protected virtual void SquatDown()
    {
        if (Input.GetButtonDown("Squat"))
        {
            isSquating = true;
        }
        if (Input.GetButtonUp("Squat"))
        {
            isSquating = false;
        }
    }
    //判断是否奔跑
    protected virtual void SprintDown()
    {
        if (isSquating)
        {
            isSprinting = false;
            return;
        }
        if (Input.GetButtonDown("Sprint") && vertical > 0)
        {
            isSprinting = true;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }
    }

}