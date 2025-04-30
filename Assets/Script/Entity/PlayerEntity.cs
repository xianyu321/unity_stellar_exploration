

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
    protected Transform cam;//玩家摄像机
    private float pitch = 0f; // 上下视角角度
    protected float mouseHorizontal;//鼠标横坐标
    protected float mouseVertical;//鼠标纵坐标
    protected Vector3 velocity;//速度矢量
    bool isTouchBlock = false;//是否有碰触到方块
    public float contactDis = 8f;//最大触碰距离
    public ToolBar toolBar;
    public bool IsShiftDown = false;

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
        toolBar.Initialize(this);
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
            CalulateCam();
        }
        movement.Update();
    }

    private void FixedUpdate()
    {
        // if (!inUI)
        // {
        //     CalulateCam();
        // }
        movement.FixedUpdate();
    }
    //处理视角移动
    private void CalulateCam(){
        //处理水平视角
        transform.Rotate(Vector3.up * mouseHorizontal * setting.mouseSensitivity);
        //处理垂直视角
        pitch -= mouseVertical * setting.mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -90, 90);
        cam.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        PlayerItem.transform.Translate(velocity, Space.World);
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
                    if(world.PlacedBlock(placedBlockPos, toolBar.GetNowItemID())){
                        toolBar.GetNowItem().TakeOne();
                    }
                }else if(!world.GetBlock(brokenBlockPos).IsCanUse()){
                    if(world.PlacedBlock(placedBlockPos, toolBar.GetNowItemID())){
                        toolBar.GetNowItem().TakeOne();
                    }
                }else{
                    world.GetBlock(brokenBlockPos).UseBlock();
                }
            }
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