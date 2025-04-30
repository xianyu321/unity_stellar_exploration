using TMPro;
using UnityEngine;


public class Player : MonoBehaviour
{
    //在地面
    public bool isGrounded;
    //在奔跑
    public bool isSprinting;
    //是否蹲下
    public bool isSquating = false;
    [Header("走路跑步参数")]
    //向前走路速度
    public float frontWalkSpeed = 4f;
    //向前跑步时速度
    public float frontSprintSpeed = 7f;
    //左右走路时速度
    public float horizontalWalkSpeed = 2.5f;
    //左右跑步时速度
    public float horizontalSprintSeppd = 3f;
    //向后走路速度
    public float backWalkSpeed = 2.5f;
    //跳跃力度
    public float jumpForce = 8f;
    //重力
    public float gravity = -20f;
    //最大下落速度
    public float maxFallSpeed = -20f;
    //蹲下时的速度倍率
    float squatSpeed = 0.5f;
    //玩家高度
    public float playerHeight = 1.8f;
    //碰撞宽度
    public float playerWidth = 0.25f;
    protected float verticalMomentum = 0;//垂直速度向量
    protected bool jumpRequest;//可以跳跃
    protected Transform cam;
    protected World world;
    //横坐标
    protected float horizontal;
    //纵坐标
    protected float vertical;
    //鼠标横坐标
    protected float mouseHorizontal;
    //鼠标纵坐标
    protected float mouseVertical;
    //速度矢量
    protected Vector3 velocity;
    //是否有碰触到方块
    bool isTouchBlock = false;
    //破坏方块的坐标
    Vector3Int highlightBlock;
    //高亮方块的样式
    public Transform highlightBlockStyle;
    //放置方块的坐标
    Vector3Int placeBlock;
    //放置方块的样式
    public Transform placeBlockStyle;
    //射线增量
    public float checkIncrement = 0.1f;
    //最大触碰距离
    public float reach = 8f;
    //显示放置的东西名字
    public TMP_Text selectedBlockText;
    // public int selectedBlockIndex = 1;//显示放置的东西索引

    public ToolBar toolBar;

    public Vector3 pos
    {
        get
        {
            return transform.position;
        }
    }

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
        // Cursor.lockState = CursorLockMode.Locked;
        world.inUI = false;
        //初始化手持方块
        if (selectedBlockText)
            selectedBlockText.text = "手持方块: " + world.blockTypes[toolBar.GetItemIDNow()].blockName;
        // Debug.Log(BlockManager.Instance.Blocks);
        // Debug.Log(BlockManager.Instance.GetBlockByName("Dirt"));
    }

    private void Update()
    {
        return;
        if (Input.GetKeyDown(KeyCode.B))
        {
            world.inUI = !world.inUI;
        }
        if (!world.inUI)
        {
            //获取轴线
            GetPlayerInput();
            //更新鼠标方块
            placeCursorBlock();
        }
    }

    private void FixedUpdate()
    {
        return;
        if (!world.inUI)
        {
            CalculateVelocity();//计算速度
            transform.Rotate(Vector3.up * mouseHorizontal * world.settings.mouseSensitivity);
            cam.Rotate(Vector3.right * -mouseVertical * world.settings.mouseSensitivity);
            transform.Translate(velocity, Space.World);
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

        Vector3 tempVelocity = new Vector3(0, 0, velocity.z);
        if (CheckPlayerMove(moveFront, tempVelocity) || CheckPlayerMove(moveBack, tempVelocity))
        {
            velocity.z = 0;
        }
        tempVelocity.z = 0;
        tempVelocity.x = velocity.x;
        if (CheckPlayerMove(moveLeft, tempVelocity) || CheckPlayerMove(moveRight, tempVelocity))
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
            velocity.y = checkDownSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = checkUpSpeed(velocity.y);
        }
    }

    protected virtual void Jump()
    {
        if (jumpRequest)
        {
            verticalMomentum = jumpForce;
            isGrounded = false;
            jumpRequest = false;
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
            if (Input.GetMouseButtonDown(0))
            {
                // world.GetChunkFromVector3(highlightBlock).EditVoxel(highlightBlock);
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

    protected virtual void JumpDown()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
    }

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

    protected virtual void SprintDown()
    {
        if (isSquating)
        {
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

    protected void placeCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPos = cam.position;
        while (step < reach)
        {
            Vector3 pos = cam.position + cam.forward * step;
            if (world.IsVoxelInWorld(pos) && world.CheckForVoxel(pos))
            {
                highlightBlock = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock = new Vector3Int(Mathf.FloorToInt(lastPos.x), Mathf.FloorToInt(lastPos.y), Mathf.FloorToInt(lastPos.z));
                isTouchBlock = true;
                // highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                // placeBlock.position = lastPos;
                // highlightBlock.gameObject.SetActive(true);
                // placeBlock.gameObject.SetActive(true);
                return;
            }
            lastPos = pos;
            step += checkIncrement;
        }
        isTouchBlock = false;
        // highlightBlock.gameObject.SetActive(false);
        // placeBlock.gameObject.SetActive(false);
    }

    //判断玩家地表可进行跳跃
    protected float checkDownSpeed(float downSpeed)
    {

        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
        )
        {
            isGrounded = true;
            transform.position = new Vector3(transform.position.x, Mathf.Floor(transform.position.y), transform.position.z);
            verticalMomentum = 0;
            return 0;
        }
        downSpeed = Mathf.Max(downSpeed, maxFallSpeed);
        isGrounded = false;
        return downSpeed;
    }
    //判断玩家上方没有阻挡可跳跃
    protected float checkUpSpeed(float upSpeed)
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth))
        )
        {
            verticalMomentum = 0;
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    //前后左右能否移动
    float playerHeightEps = 0.5f;
    int[,] moveFront = new int[2, 2]{
        {-1,1},
        {1,1},
    };
    int[,] moveBack = new int[2, 2]{
        {-1,-1},
        {1,-1}
    };
    int[,] moveLeft = new int[2, 2]{
        {-1,-1},
        {-1,1}
    };
    int[,] moveRight = new int[2, 2]{
        {1,-1},
        {1,1}
    };

    bool CheckPlayerMove(int[,] dxz, Vector3 move)
    {
        Vector3 nextMove = new Vector3(transform.position.x + move.x, transform.position.y, transform.position.z + move.z);
        for (int i = 0; i < 2; ++i)
        {
            if (world.CheckForVoxel(new Vector3(
                nextMove.x + playerWidth * dxz[i, 0],
                nextMove.y + playerHeight,
                nextMove.z + playerWidth * dxz[i, 1]
            )))
            {
                return true;
            }
            for (float h = 0; h < playerHeight; h += playerHeightEps)
            {
                if (world.CheckForVoxel(new Vector3(
                    nextMove.x + playerWidth * dxz[i, 0],
                    nextMove.y + h,
                    nextMove.z + playerWidth * dxz[i, 1]
                )))
                {
                    return true;
                }
            }
        }
        // if (
        // world.CheckForVoxel(new Vector3(nextMove.x + playerWidth * dxz.x, nextMove.y, nextMove.z + playerWidth * dxz.y)) ||
        // world.CheckForVoxel(new Vector3(nextMove.x + playerWidth * dxz.x, nextMove.y + 1f, nextMove.z + playerWidth * dxz.y))
        // )
        // {
        //     return true;
        // }
        return false;
    }

    //判断玩家前方无阻挡可移动
    public bool front
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
                )
                return true;
            else
                return false;
        }
    }
    //判断玩家后面无阻挡可移动
    public bool back
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
                )
                return true;
            else
                return false;
        }
    }
    //判断玩家左面无阻挡可移动
    public bool left
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }
    }
    //判断玩家右面无阻挡可移动
    public bool right
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }
    }
    public Vector3 center
    {
        get
        {
            return new Vector3(pos.x, pos.y + playerHeight / 2, pos.z);
        }
    }
    public Vector3 size
    {
        get
        {
            return new Vector3(playerWidth * 2, playerHeight, playerWidth * 2);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(center, size);
    }
}

// public class OutLine{
//     Vector3 pos;//显示立方体的最小点
//     Vector3 xyz;//显示立方体的长宽高
//     Color color = Color.white;
//     float width = 0.05f;
//     private LineRenderer lineRenderer;
//     public OutLine(){
//         Init();
//     }
//     public OutLine(Vector3 _pos, Vector3 _xyz){
//         pos = _pos;
//         xyz = _xyz;
//         Init();
//     }
//     public OutLine(Vector3 _pos, Vector3 _xyz, Color _color, float _width){
//         pos = _pos;
//         xyz = _xyz;
//         color = _color;
//         width = _width;
//         Init();
//     }

//     public void Init(){
//         lineRenderer = new LineRenderer();
//         lineRenderer.startColor = color;
//         lineRenderer.endColor = color;
//         lineRenderer.startWidth = width;
//         lineRenderer.endWidth = width;
//     }

//     public void SetPos(Vector3 _pos){
//         pos = _pos;
//     }

//     public void SetXYZ(Vector3 _xyz){
//         xyz = _xyz;
//     }

//     public void ShowLine(){

//     }

//     public void ClearLine(){

//     }
// }