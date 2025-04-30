

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerEntity:MonoBehaviour
{
    public CollisionEntity collision;
    public PlayerSetting setting = new();
    public GameObject PlayerItem;
    public PlayerMovement movement;
    protected WorldEntity world;//所处世界
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
    protected Transform cam;//玩家摄像机
    protected float horizontal;//左右移动量
    protected float vertical;//前后移动量
    private float pitch = 0f; // 上下视角角度
    protected float mouseHorizontal;//鼠标横坐标
    protected float mouseVertical;//鼠标纵坐标
    protected Vector3 velocity;//速度矢量
    bool isTouchBlock = false;//是否有碰触到方块
    Vector3Int highlightBlock;//破坏方块的坐标
    public Transform highlightBlockStyle;//高亮方块的样式
    //放置方块的坐标
    Vector3Int placeBlock;
    //放置方块的样式
    public Transform placeBlockStyle;
    public float contactDis = 8f;//最大触碰距离
    public ToolBar toolBar;
    public Vector3 pos
    {
        get
        {
            return transform.position;
        }
    }

    private bool _inUI;
    public bool inUI
    {
        get
        {
            return _inUI;
        }
        set
        {
            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        movement = new PlayerMovement(this);
        world = WorldManager.Instance.GetOrCreateWorld(new());
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            inUI = !inUI;
        }
        if (!inUI)
        {
            //获取轴线
            GetPlayerInput();
            //更新鼠标方块
            placeCursorBlock();
        }
    }

    private void FixedUpdate()
    {
        if (!inUI)
        {
            CalculateVelocity();//计算速度
            transform.Rotate(Vector3.up * mouseHorizontal * setting.mouseSensitivity);
            float currentPitch = cam.localEulerAngles.x;
            float targetPitch = currentPitch - mouseVertical;
            // 获取鼠标Y轴输入
            float mouseY = Input.GetAxis("Mouse Y") * setting.mouseSensitivity;

            // 计算新的pitch值
            pitch -= mouseVertical * setting.mouseSensitivity;

            // 限制pitch值在-90到+90度之间
            pitch = Mathf.Clamp(pitch, -90, 90);

            // 应用旋转
            cam.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            // cam.Rotate(Vector3.right * -mouseVertical * setting.mouseSensitivity);
            PlayerItem.transform.Translate(velocity, Space.World);
        }
    }
    
    private void CalculateVelocity()
    {
        XZMove();// 平移
        XZCollide();// 平移发生碰撞
        YMove();//上下移动
        YCollide();//上下发生碰撞
        Jump();
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
        if (isSquating){
            velocity *= squatSpeed;
        }
    }

    protected virtual void XZCollide()
    {
        if(velocity == Vector3.zero){
            return;
        }
        Vector3 tempVelocity = new Vector3(0, 0, velocity.z);
        if (collision.ChunkMoveXZ(world ,CollisionEntity.moveFront, tempVelocity) || collision.ChunkMoveXZ(world, CollisionEntity.moveBack, tempVelocity))
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
            float moveY =  collision.ChunkDownSpeed(world, velocity.y);
            if(moveY > velocity.y){
                velocity.y = moveY;
                isGrounded = true;
                verticalMomentum = 0;
            }
            if(velocity.y < maxFallSpeed){
                velocity.y = maxFallSpeed;
            }
        }
        else if (velocity.y > 0)
        {
            float moveY =  collision.ChunkUpSpeed(world, velocity.y);
            if(moveY < velocity.y){
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

    private void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        JumpDown();
        SquatDown();
        SprintDown();

        if (isTouchBlock)
        {
            if (Input.GetMouseButtonDown(0) && isTouchBlock)
            {
                world.BrokenBlock(brokenBlockPos);
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (toolBar.GetSlotNow().hasItem)
                {
                    // world.GetChunkFromVector3(placeBlock).EditVoxel(placeBlock, toolBar.GetItemIDNow());
                    toolBar.GetSlotNow().itemSlot.Take(1);
                }
            }
            if (highlightBlockStyle != null)
            {
                highlightBlockStyle.gameObject.SetActive(true);
                highlightBlockStyle.position = highlightBlock;
            }
            if (placeBlockStyle != null)
            {
                placeBlockStyle.gameObject.SetActive(true);
                placeBlockStyle.position = placeBlock;
            }
        }
        else
        {
            if (highlightBlockStyle != null)
            {
                highlightBlockStyle.gameObject.SetActive(false);
            }
            if (placeBlockStyle != null)
            {
                placeBlockStyle.gameObject.SetActive(false);
            }
        }
    }
    //判断是否跳跃
    protected virtual void JumpDown()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
        if(Input.GetButtonUp("Jump")){
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
    Vector3Int placedBlockPos;
    Vector3Int brokenBlockPos;
    Ray ray;
    HashSet<Vector3Int> visited;
    bool endDfs;
    bool dfsFind;
    RaycastHit hitInfo;
    // 定义六个方向的向量
    static Vector3Int[] directions = new Vector3Int[]
    {
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), // 左右
        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0), // 上下
        new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)  // 前后
    };
    protected void placeCursorBlock()
    {
        Vector3 startPos = cam.position;
        Vector3Int startBlockPos = VectorTools.Vct3ToVec3Int(startPos);
        placedBlockPos = startBlockPos;
        brokenBlockPos = startBlockPos;
        Debug.Log(startPos);
        Debug.Log(cam.transform.forward);
        ray = new Ray(startPos, cam.transform.forward);
        visited = new();
        endDfs = false;
        dfsFind = false;
        dfsByRay(startBlockPos);
        isTouchBlock = dfsFind;
    }

    protected void dfsByRay(Vector3Int blockPos){
        //跳过已访问和在访问结束立马跳出
        if (visited.Contains(blockPos) || endDfs)
            return;
        //更新选中坐标
        placedBlockPos = brokenBlockPos;
        brokenBlockPos = blockPos;
        if(world.GetBlock(blockPos).IsEntity()){
            endDfs = true;
            dfsFind = true;
            return;
        }
        visited.Add(blockPos);
        //判断射线有没有经过这个坐标
        foreach (Vector3Int direction in directions)
        {
            Vector3Int nextPos = blockPos + direction;
            if(RayHit.IntersectRayWithVoxel(ray, nextPos,out hitInfo, contactDis)){
                dfsByRay(nextPos);
            }
        }
    }

    

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube((collision.pos + collision.size) / 2, collision.size);
    }
}