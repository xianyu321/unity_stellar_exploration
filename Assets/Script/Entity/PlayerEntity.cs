

using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerEntity : MonoBehaviour
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
    public ChunkCoord nowChunkCoord;
    public ChunkCoord preChunkCoord;
    public List<ItemData> items = new List<ItemData>();
    void Awake()
    {
        for(int i = 0; i < 1; ++i){
            items.Add(new(6, 2));
        }
    }
    private void Start()
    {
        movement = new PlayerMovement(this);
        toolBar.Initialize(this);
        world = WorldManager.Instance.GetOrCreateWorld(new());
        Cursor.lockState = CursorLockMode.Locked;
        EscUI.SetActive(isShowEscUI);
        BackpackUI.SetActive(isShowBackPack);
        selfMask = 1 << LayerMask.NameToLayer("SelfBody");
        PlayerItem.transform.position = new Vector3(0, 60, 0);
        InitData();
    }
    public void InitData()
    {
        nowChunkCoord = preChunkCoord = ChunkManager.ToChunkCoord(transform.position);
        InitChunks();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isShowBackPack)
            {
                BackpackUI.SetActive(false);
                isShowBackPack = false;
                inUI = false;
            }
            else if (!inUI)
            {
                BackpackUI.SetActive(true);
                isShowBackPack = true;
                inUI = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isShowEscUI)
            {
                EscUI.SetActive(false);
                isShowEscUI = false;
                inUI = false;
            }
            else if (!inUI)
            {
                EscUI.SetActive(true);
                isShowEscUI = true;
                inUI = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            isOpenF3 = !isOpenF3;
            if (isOpenF3)
            {
                camLook.cullingMask |= selfMask;
            }
            else
            {
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
        UpdateChunkPos();
        movement.FixedUpdate();
        UpdateAnim();
        ChunkHasDrop();
    }
    void ChunkHasDrop(){
        DropItemEntity drop = DropItemManager.Instance.GetDropByPlayer(collision.pos + collision.size / 2, collision.absorbSize);
        if(drop!=null){
            ItemData data = drop.data;
            foreach(ItemData item in items){
                if(item != null && item.id == data.id && item.num < item.maxNum){
                    item.num += data.num;
                    data.num = 0;
                    if(item.num > item.maxNum){
                        data.num = item.num - item.maxNum;
                        item.num = item.maxNum;
                    }
                    if(data.num == 0){
                        DropItemManager.Instance.RemoveListItem(drop);
                        break;
                    }
                }
            }
            if(data.num > 0){
                for(int i = 0; i < items.Count; ++i){
                    if(items[i] == null){
                        DropItemManager.Instance.RemoveListItem(drop);
                        items[i] = data;
                        break;
                    }
                }
            }
        }
    }
    //处理玩家移动区块改变后自动销毁和生成
    int loadWidth = 3;
    int delWidth = 3;

    void InitChunks()
    {
        for (int i = nowChunkCoord.x - loadWidth; i <= nowChunkCoord.x + loadWidth; ++i)
        {
            for (int j = nowChunkCoord.z - loadWidth; j <= nowChunkCoord.z + loadWidth; ++j)
            {
                // world.AddLoad(i, j);
                addChunks.Enqueue(new(i, j));
            }
        }
    }
    public void Exit(){
        world.SaveAll();
        SceneManager.LoadScene("SampleScene");
    }

    private void UpdateChunkPos()
    {
        // return;
        nowChunkCoord = ChunkManager.ToChunkCoord(transform.position);
        int dis = ChunkManager.GetChunkDis(nowChunkCoord, preChunkCoord);
        UpdateChunks();
        if (dis > 2)
        {
            if (TaskRunner.Instance.GetActiveTaskCount() == 0)
            {
                UpdateChunkTask();
            }
        }
    }
    private Queue<ChunkCoord> addChunks = new Queue<ChunkCoord>();
    private Queue<ChunkCoord> delChunks = new Queue<ChunkCoord>();
    int workTimes = 1;
    void UpdateChunks()
    {
        if (addChunks.Count == 0 && delChunks.Count == 0)
        {
            return;
        }
        int time = workTimes;
        while (time > 0)
        {
            --time;
            if (addChunks.Count > 0)
            {
                ChunkCoord coord = addChunks.Dequeue();
                world.AddLoad(coord.x, coord.z);
            }
        }
        while (time > 0)
        {
            --time;
            if (addChunks.Count > 0)
            {
                ChunkCoord coord = delChunks.Dequeue();
                world.DelShow(coord.x, coord.z);
            }
        }
    }
    private void UpdateChunkTask()
    {
        for (int i = nowChunkCoord.x - loadWidth; i <= nowChunkCoord.x + loadWidth; ++i)
        {
            bool flag = false;
            if (i < preChunkCoord.x - loadWidth || i > preChunkCoord.x + loadWidth)
            {
                flag = true;
            }
            for (int j = nowChunkCoord.z - loadWidth; j <= nowChunkCoord.z + loadWidth; ++j)
            {
                if (flag || j < preChunkCoord.z - loadWidth || j > preChunkCoord.z + loadWidth)
                {
                    // world.AddLoad(i, j);
                    addChunks.Enqueue(new(i, j));
                }
            }
        }
        for (int i = preChunkCoord.x - delWidth; i <= preChunkCoord.x + delWidth; ++i)
        {
            bool flag = false;
            if (i < nowChunkCoord.x - delWidth || i > nowChunkCoord.x + delWidth)
            {
                flag = true;
            }
            for (int j = preChunkCoord.z - delWidth; j <= preChunkCoord.z + delWidth; ++j)
            {
                if (flag || j < nowChunkCoord.z - delWidth || j > nowChunkCoord.z + delWidth)
                {
                    // world.DelShow(i, j);
                    delChunks.Enqueue(new(i, j));
                }
            }
        }
        preChunkCoord = nowChunkCoord;
    }
    //处理视角移动
    private void CalulateCam()
    {
        //处理水平视角
        transform.Rotate(Vector3.up * mouseHorizontal * setting.mouseSensitivity);
        //处理垂直视角
        pitch -= mouseVertical * setting.mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -90, 90);
        playLook.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        PlayerItem.transform.Translate(velocity, Space.World);
        if (isOpenF3)
        {
            camLook.transform.position = playLook.position - playLook.forward * 5f;
        }
        else
        {
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
                if (IsShiftDown)
                {
                    if (toolBar.NowHasItem() && world.PlacedBlock(placedBlockPos, toolBar.GetNowItemID()))
                    {
                        toolBar.GetNowItem().TakeOne();
                    }
                }
                else if (!world.GetBlock(brokenBlockPos).IsCanUse())
                {
                    if (toolBar.NowHasItem() && world.PlacedBlock(placedBlockPos, toolBar.GetNowItemID()))
                    {
                        toolBar.GetNowItem().TakeOne();
                    }
                }
                else
                {
                    world.GetBlock(brokenBlockPos).UseBlock();
                }
            }
        }
    }

    private void UpdateAnim()
    {

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

    protected void dfsByRay(Vector3Int blockPos)
    {
        //跳过已访问和在访问结束立马跳出
        if (visited.Contains(blockPos) || endDfs)
            return;
        //更新选中坐标
        placedBlockPos = brokenBlockPos;
        brokenBlockPos = blockPos;
        if (world.GetBlock(blockPos).IsEntity())
        {
            endDfs = true;
            dfsFind = true;
            return;
        }
        visited.Add(blockPos);
        //判断射线有没有经过这个坐标
        foreach (Vector3Int direction in directions)
        {
            Vector3Int nextPos = blockPos + direction;
            if (RayHit.IntersectRayWithVoxel(ray, nextPos, out hitInfo, contactDis))
            {
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