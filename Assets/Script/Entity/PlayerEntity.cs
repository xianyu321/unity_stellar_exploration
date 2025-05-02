

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerEntity:MonoBehaviour
{
    public CollisionEntity collision;
    public PlayerSetting setting = new();
    public GameObject PlayerItem;
    public PlayerMovement movement;
    public WorldEntity world;//所处世界
    public Transform playLook;//玩家视角
    public Camera camLook;//摄像机视角
    private float pitch = 0f; // 上下视角角度
    protected float mouseHorizontal;//鼠标横坐标
    protected float mouseVertical;//鼠标纵坐标
    protected Vector3 velocity;//速度矢量
    bool isTouchBlock = false;//是否有碰触到方块
    public float contactDis = 8f;//最大触碰距离
    public ToolBar toolBar;
    public bool IsShiftDown = false;
    private bool _inUI;
    public GameObject EscUI;
    public GameObject BackpackUI;
    public bool isShowEscUI = false;
    public bool isShowBackPack = false;
    public bool isOpenF3 = false;
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
    protected int selfMask;

    public Animator anim;

    private void Start()
    {
        // cam = GameObject.Find("Main Camera").transform;
        movement = new PlayerMovement(this);
        toolBar.Initialize(this);
        world = WorldManager.Instance.GetOrCreateWorld(new());
        Cursor.lockState = CursorLockMode.Locked;
        EscUI.SetActive(isShowEscUI);
        BackpackUI.SetActive(isShowBackPack);
        selfMask = 1 << LayerMask.NameToLayer("SelfBody");
        this.PlayerItem.transform.position = new Vector3(0, 60, 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if(isShowBackPack){
                BackpackUI.SetActive(false);
                isShowBackPack = false;
                inUI = false;
            }else if(!inUI){
                BackpackUI.SetActive(true);
                isShowBackPack = true;
                inUI = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(isShowEscUI){
                EscUI.SetActive(false);
                isShowEscUI = false;
                inUI = false;
            }else if(!inUI){
                EscUI.SetActive(true);
                isShowEscUI = true;
                inUI = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.F3)){
            isOpenF3 = !isOpenF3;
            if(isOpenF3){
                camLook.cullingMask |= selfMask;
            }else{
                camLook.cullingMask &= ~selfMask;
            }
        }

        if (!inUI)
        {
            //获取轴线
            GetPlayerInput();
            //更新鼠标方块
            placeCursorBlock();
            CalulateCam();
        }
        movement.Update();
    }

    private void FixedUpdate()
    {
        movement.FixedUpdate();
        UpdateAnim();
    }
    //处理视角移动
    private void CalulateCam(){
        //处理水平视角
        transform.Rotate(Vector3.up * mouseHorizontal * setting.mouseSensitivity);
        //处理垂直视角
        pitch -= mouseVertical * setting.mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -90, 90);
        playLook.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        PlayerItem.transform.Translate(velocity, Space.World);
        if(isOpenF3){
            camLook.transform.position = playLook.position - playLook.forward * 5f;
        }else{
            camLook.transform.position = playLook.position;
        }
    }

    private void GetPlayerInput()
    {
        mouseHorizontal = Input.GetAxis("Mouse X") * Time.deltaTime;
        mouseVertical = Input.GetAxis("Mouse Y") * Time.deltaTime;
        IsShiftDown = Input.GetKeyDown(KeyCode.LeftShift);
        if (isTouchBlock)
        {
            if (Input.GetMouseButtonDown(0) && isTouchBlock)
            {
                world.BrokenBlock(brokenBlockPos);
                return;
            }
            if (Input.GetMouseButtonDown(1) && isTouchBlock)
            {
                if(IsShiftDown){
                    if(toolBar.NowHasItem() && world.PlacedBlock(placedBlockPos, toolBar.GetNowItemID())){
                        toolBar.GetNowItem().TakeOne();
                    }
                }else if(!world.GetBlock(brokenBlockPos).IsCanUse()){
                    if(toolBar.NowHasItem() && world.PlacedBlock(placedBlockPos, toolBar.GetNowItemID())){
                        toolBar.GetNowItem().TakeOne();
                    }
                }else{
                    world.GetBlock(brokenBlockPos).UseBlock();
                }
            }
        }
    }

    private void UpdateAnim() {
        
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
        Vector3 startPos = playLook.position;
        Vector3Int startBlockPos = VectorTools.Vct3ToVec3Int(startPos);
        placedBlockPos = startBlockPos;
        brokenBlockPos = startBlockPos;
        ray = new Ray(startPos, playLook.transform.forward);
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