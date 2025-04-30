using UnityEngine;

public class PlayerMovement{
    PlayerEntity player;
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
    private float pitch = 0f; // 上下视角角度
    protected float mouseHorizontal;//鼠标横坐标
    protected float mouseVertical;//鼠标纵坐标
    protected Vector3 velocity;//速度矢量
    bool isTouchBlock = false;//是否有碰触到方块
    Vector3Int highlightBlock;//破坏方块的坐标
    public Transform highlightBlockStyle;//高亮方块的样式
    public PlayerMovement(PlayerEntity _player){
        player = _player;
    }

    public void Update(){
        if (Input.GetKeyDown(KeyCode.B))
        {
            player.inUI = !player.inUI;
        }
        if (!player.inUI)
        {
            //获取轴线
            GetPlayerInput();
            //更新鼠标方块
            // placeCursorBlock();
        }
    }
    public void FixedUpdate(){

    }

        private void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // JumpDown();
        // SquatDown();
        // SprintDown();
    }
}