using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// 消除3~4个玩具时是40积分，5~80个玩具时是10*n，比如有10个相连的消除则获得10*10=100积分。
/// </summary>
public enum GameStatus
{
    Wait,
    Start,
    Stop,
    Add,
    End,
}

public class Game : MonoBehaviour
{
    public GameObject menuLayer;
    public GameObject resultLayer;
    public Transform winLayer;
    public Transform loseLayer;

    public AudioClip clip;

    public List<Sprite> blockSpriteList;
    public Transform blockContainer;
    public Transform blockOutContainer;
    public GameObject blockPref;

    public GameObject zhadanxiaochu, huojianxiaochu;
    public GameObject daodanfeixing;

    public List<GameObject> goodList;

    public GameObject btnStop, btnStart, btnVolume;

    public Image fgBar;
    public Text lblScore, lblLevel;

    public Transform spArrow;

    [SerializeField]
    private List<RectTransform> guideList;

    private AudioSource audioSource;

    private int col = 10; // 列数
    private int row = 10;//横数

    //private int[,] grid;
    private Block[,] grid;
    private List<Block> specialBlockList;
    private List<Block> simpleBlockList;
    List<Block> closedBlockList;

    private float blockStartX = -365.5f;
    private float blockStartY = -268.7f;
    private float blockOffsetX = 79.5f;
    private float blockOffsetY = 59f;

    Config config;

    private int curLevel;
    private int maxLevel;

    private int curScore;
    private int totalScore;

    private const int totalTime = 100;
    private int residueTime; //剩余时间

    private int guideStep = 1;

    public static GameStatus gameStatus;

    public static bool volumOpen = true;

    void Awake()
    {
        grid = new Block[row, col];
        closedBlockList = new List<Block>();
        specialBlockList = new List<Block>();
        simpleBlockList = new List<Block>();

        var guideMask = FindObjectOfType<GuideMask>();
        guideMask.Init();
        GuideMask.Self.Close();

        audioSource = GameObject.Find("GameManager").GetComponent<AudioSource>();
        audioSource.volume = Game.volumOpen ? 1 : 0;
        btnVolume.transform.Find("spDisable").gameObject.SetActive(!Game.volumOpen);
    }

    void Start()
    {
        config = Config.Instance;

        curLevel = 1;
        maxLevel = PlayerPrefs.GetInt("maxLevel", 1);

        totalScore = PlayerPrefs.GetInt("totalScore", 0);

        gameStatus = GameStatus.Wait;


        onBtnClick("btnMenu");
    }

    void clear()
    {
        for(int i = specialBlockList.Count - 1; i >= 0; i--)
        {
            GameObject.Destroy(specialBlockList[i].gameObject);
        }
        for (int i = simpleBlockList.Count - 1; i >= 0; i--)
        {
            GameObject.Destroy(simpleBlockList[i].gameObject);
        }

        specialBlockList.Clear();
        simpleBlockList.Clear();
    }

    void generate()
    {
        clear();
        residueTime = totalTime;
        fgBar.fillAmount = 1;
        curScore = 0;
        lblScore.text = "SCORE:0";
        int[,] levelData = config.getLevelData(curLevel);
        lblLevel.text = curLevel.ToString();
        int index = 0;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (levelData[i, j] != 0)
                {
                    index++;
                    int tmpI = i;
                    int tmpJ = j;
                    GameObject blockGo = GameObject.Instantiate(blockPref);
                    blockGo.SetActive(true);
                    Transform blockTrans = blockGo.transform;
                    blockTrans.SetParent(blockContainer);
                    blockTrans.localScale = Vector3.one;
                    blockTrans.localRotation = Quaternion.identity;
                    blockTrans.localPosition = new Vector3(blockStartX + j * blockOffsetX, blockStartY + i * blockOffsetY, 0);
                    Block block = blockTrans.GetComponent<Block>();
                    block.type = levelData[i, j];
                    block.x = tmpI;
                    block.y = tmpJ;
                    block.index = tmpJ * 10 + tmpI; ;
                    block.setSprite(blockSpriteList[levelData[i, j] - 1]);
                    block.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        onBlockClick(block);
                    });
                    grid[i, j] = block;
                    if (levelData[i, j] >= 13)
                    {
                        specialBlockList.Add(block);
                    }
                    else
                    {
                        simpleBlockList.Add(block);
                    }
                }
                else
                {
                    grid[i, j] = null;
                }

            }
        }
        gameStatus = GameStatus.Start;
        if (curLevel == 1)
        {
            guideStep = 1;
            GuideMask.Self.Close();
            GuideMask.Self.Play(guideList[guideStep - 1], guideStep);
        }else
        {
            StopAllCoroutines();
            StartCoroutine(countdown());
            btnStop.gameObject.SetActive(true);
        }
            
        
       
    }

    void addBlock()
    {
        gameStatus = GameStatus.Start;
        List<int> typeList = Config.Instance.addConfig[curLevel - 1];
        //for(int i = 0;i< simpleBlockList.Count; i++)
        //{
        //    if(!typeList.Contains(simpleBlockList[i].type))
        //    {
        //        typeList.Add(simpleBlockList[i].type);
        //    }
        //}
        //if (typeList.Count < 1)
        //typeList = Config.Instance.addConfig[curLevel - 1];
        int tmpCol = 0;
        int tmpRow = 9;
        bool canAdd = true;
        int generateCount = 0;
        while(generateCount < 12)
        {
            //if (grid[0, tmpCol] != null && grid[tmpRow, tmpCol] == null)
            if (grid[tmpRow, tmpCol] == null)
                {
                canAdd = true;
                for (int j = 0; j < specialBlockList.Count; j++)
                {
                    if (tmpCol == specialBlockList[j].y)
                    {
                        canAdd = false;
                        break;
                    }
                }
                
                if(canAdd)
                {
                    GameObject blockGo = GameObject.Instantiate(blockPref);
                    blockGo.SetActive(true);
                    Transform blockTrans = blockGo.transform;
                    blockTrans.SetParent(blockContainer);
                    blockTrans.localScale = Vector3.one;
                    blockTrans.localRotation = Quaternion.identity;
                    blockTrans.localPosition = new Vector3(blockStartX + tmpCol * blockOffsetX, blockStartY + tmpRow * blockOffsetY, 0);
                    Block block = blockTrans.GetComponent<Block>();
                    block.type = typeList[Random.Range(0, typeList.Count)];
                    block.x = tmpRow;
                    block.y = tmpCol;
                    block.index = tmpCol * 10 + tmpRow;
                    block.setSprite(blockSpriteList[block.type - 1]);
                    block.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        onBlockClick(block);
                    });
                    grid[tmpRow, tmpCol] = block;

                    simpleBlockList.Add(block);

                    generateCount += 1;
                }
            }
            tmpCol += 1;
            if(tmpCol > 9)
            {
                tmpCol = 0;
                tmpRow--;
                if (tmpRow < 0) break;
            }
        }
        StartCoroutine(resetGrid());
    }

    void onBlockClick(Block block)
    {
        if (gameStatus != GameStatus.Start) return;
        audioSource.PlayOneShot(clip);
        if(curLevel == 1 && guideStep == 1)
        {
            guideStep += 1;
            GuideMask.Self.Play(guideList[guideStep - 1], guideStep);
            return;
        }
        closedBlockList.Clear();
        if (block.type == 12)//炸弹
        {
            Vector3 pos = block.transform.localPosition;
            zhadanxiaochu.transform.localPosition = pos;
            zhadanxiaochu.gameObject.SetActive(false);
            zhadanxiaochu.gameObject.SetActive(true);
            findExist(block.x - 2, block.y);
            findExist(block.x - 1, block.y);
            findExist(block.x, block.y);
            findExist(block.x + 1, block.y);
            findExist(block.x + 2, block.y);
            findExist(block.x, block.y + 1);
            findExist(block.x - 1, block.y + 1);
            findExist(block.x + 1, block.y + 1);
            findExist(block.x, block.y - 1);
            findExist(block.x - 1, block.y - 1);
            findExist(block.x + 1, block.y - 1);
            findExist(block.x, block.y + 2);
            findExist(block.x, block.y - 2);
            for (int i = 0; i < closedBlockList.Count; i++)
            {
                closedBlockList[i].disappear(true);
                if (specialBlockList.Contains(closedBlockList[i]))
                    specialBlockList.Remove(closedBlockList[i]);
                if (simpleBlockList.Contains(closedBlockList[i]))
                    simpleBlockList.Remove(closedBlockList[i]);
                grid[closedBlockList[i].x, closedBlockList[i].y] = null;
            }
            if (curLevel == 1)
            {
                guideStep += 1;
                if (guideStep <= guideList.Count)
                    GuideMask.Self.Play(guideList[guideStep - 1], guideStep);
                else
                    GuideMask.Self.Close();
            }
            StartCoroutine(resetGrid());
            return;
        }
        else if(block.type == 11) //火箭
        {
            Vector3 pos = block.transform.localPosition;
            huojianxiaochu.transform.localPosition = pos;
            huojianxiaochu.gameObject.SetActive(false);
            huojianxiaochu.gameObject.SetActive(true);

            daodanfeixing.gameObject.SetActive(false);
            
            daodanfeixing.gameObject.SetActive(true);
            daodanfeixing.transform.localPosition = new Vector3(-806, block.transform.localPosition.y, 0);

            float delay = 0.05f;
            for(int y = col - 1; y >= 0; y--)
            {
                delay += 0.1f;
                if (grid[block.x, y] != null && grid[block.x, y].type < 13)
                {
                    grid[block.x, y].disappear(true, delay);
                    if (specialBlockList.Contains(grid[block.x, y]))
                        specialBlockList.Remove(grid[block.x, y]);
                    if (simpleBlockList.Contains(grid[block.x, y]))
                        simpleBlockList.Remove(grid[block.x, y]);
                    grid[block.x, y] = null;
                }                   
            }
            StartCoroutine(resetGrid(delay));
            return;
        }
        
        closedBlockList.Add(block);
        findClosed(block.x, block.y, block.type);

        if (closedBlockList.Count > 2)
        {
            int aaa = Random.Range(0, 4);
            goodList[aaa].SetActive(false);
            goodList[aaa].SetActive(true);

            closedBlockList.Sort((a, b) =>
            {
                return b.index.CompareTo(a.index);
            });
            int createType = 0;
            bool showAdd = false;
            if (closedBlockList.Count >= 5 && closedBlockList.Count <= 7)
            {
                showAdd = true;                
            }
            else if (closedBlockList.Count >= 8 && closedBlockList.Count <= 9)
                createType = 11;
            else if (closedBlockList.Count >= 10)
                createType = 12;
            int tmpIndex = -1;
            if (createType != 0)
            {
                if(curLevel == 1)
                {
                    for(int i = 0;i< closedBlockList.Count; i++)
                    {
                        if (closedBlockList[i].x == 0 && closedBlockList[i].y == 5)
                        {
                            tmpIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    tmpIndex = closedBlockList.IndexOf(block);
                }
                    //tmpIndex = Random.Range(0, closedBlockList.Count);
                GameObject blockGo = GameObject.Instantiate(blockPref);
                blockGo.SetActive(true);
                Transform blockTrans = blockGo.transform;
                blockTrans.SetParent(blockContainer);
                blockTrans.localScale = Vector3.one;
                blockTrans.localRotation = Quaternion.identity;
                blockTrans.localPosition = closedBlockList[tmpIndex].transform.localPosition;
                Block tmpblock = blockTrans.GetComponent<Block>();
                tmpblock.x = closedBlockList[tmpIndex].x;
                tmpblock.y = closedBlockList[tmpIndex].y;
                tmpblock.index = closedBlockList[tmpIndex].index;
                tmpblock.type = createType;
                tmpblock.setSprite(blockSpriteList[createType - 1]);
                grid[tmpblock.x, tmpblock.y] = tmpblock;
                blockTrans.GetComponent<Button>().onClick.AddListener(() =>
                {
                    onBlockClick(tmpblock);
                });
                simpleBlockList.Add(tmpblock);
            }
            for (int i = 0; i < closedBlockList.Count; i++)
            {
                closedBlockList[i].disappear(i != tmpIndex);
                if (specialBlockList.Contains(closedBlockList[i]))
                    specialBlockList.Remove(closedBlockList[i]);
                if (simpleBlockList.Contains(closedBlockList[i]))
                    simpleBlockList.Remove(closedBlockList[i]);
                if (i != tmpIndex)
                    grid[closedBlockList[i].x, closedBlockList[i].y] = null;
            }
            if(showAdd && simpleBlockList.Count > 0)
                simpleBlockList[Random.Range(0, simpleBlockList.Count)].setAdd();

            if (closedBlockList.Count <= 4)
                curScore += 40;
            else
                curScore = closedBlockList.Count * 10;
            lblScore.text = "SCORE:" + curScore.ToString();
            StartCoroutine(resetGrid());
            if(curLevel == 1)
            {
                guideStep += 1;
                if (guideStep <= guideList.Count)
                    GuideMask.Self.Play(guideList[guideStep - 1], guideStep);
                else
                    GuideMask.Self.Close();
            }
        }
    }
    void findClosed(int x, int y, int type)
    {
        //Debug.LogError(x + "   " + y + "   " + type);
        int left = x - 1;
        int right = x + 1;
        int up = y + 1;
        int down = y - 1;
        if(left >= 0)
        {
            //if(grid[leftX, y] != null && !closedBlockList.Contains(grid[leftX, y]) && grid[leftX, y].type == type)
            //{
            //    closedBlockList.Add(grid[leftX, y]);
            //    findClosed(leftX, y, type);
            //}
            checkInsertList(left, y, type);
        }
        if(right < col)
        {
            checkInsertList(right, y, type);
        }
        if(up < row)
        {
            checkInsertList(x, up, type);
        }
        if(down >= 0)
        {
            checkInsertList(x, down, type);
        }
    }

    void findExist(int x, int y)
    {
        if (x < 0 || x >= row || y < 0 || y >= col) return;
        if (grid[x, y] != null && grid[x,y].type < 13)
            closedBlockList.Add(grid[x, y]);
    }

    void checkInsertList(int x, int y, int type)
    {
        if (x < 0 || x >= row || y < 0 || y >= col) return;
        if (grid[x, y] != null && !closedBlockList.Contains(grid[x, y]) && grid[x, y].type == type)
        {
            closedBlockList.Add(grid[x, y]);
            findClosed(x, y, type);
        }
    }

    IEnumerator resetGrid(float delay = 0)
    {
        if (delay != 0)
            yield return new WaitForSeconds(delay);
        else
            yield return null;

        int index = 0;
        for(int y = 0; y < row; y++)
        {
            for(int x = 0; x < col; x++)
            {               
                if (grid[x,y] != null)
                {
                    index++;
                    grid[x, y].index = index;

                    grid[x, y].transform.SetAsLastSibling();
                    for (int i = 0; i < x; i++)
                    {
                        if(grid[i,y] == null)
                        {
                            Block block = grid[x, y];
                            grid[x, y] = null;
                            grid[i, y] = block;
                            block.x = i;
                            block.dotweenMoveY(blockStartY + i * blockOffsetY);
                            break;
                        }
                    }
                }
            }
        }

        checkResult();
    }

    void checkResult()
    {
        if (gameStatus != GameStatus.Start) return;
        bool isWin = true;
        for(int i = 0; i < specialBlockList.Count; i++)
        {
            if(specialBlockList[i].x != 0)
            {
                isWin = false;
                break;
            }
        }

        if(isWin)
        {
            StartCoroutine( showResult(true));
            return;
        }

        bool canBeContinue = false;
        for(int x = 0; x < row; x++)
        {
            for(int y = 0; y < col; y++)
            {
                if(grid[x,y] != null && grid[x,y].type < 13)
                {
                    if(grid[x, y].type == 11 || grid[x, y].type == 12)
                    {
                        canBeContinue = true;
                        break;
                    }
                    closedBlockList.Clear();
                    findClosed(grid[x, y].x, grid[x, y].y, grid[x, y].type);
                    if(closedBlockList.Count >= 3)
                    {
                        canBeContinue = true;
                        break;
                    }
                }
            }
            if (canBeContinue)
                break;
        }

        if (!canBeContinue)
            StartCoroutine( showResult(false));
    }

    Transform curLayer;
    IEnumerator showResult(bool isWin)
    {
        gameStatus = GameStatus.End;
        if(isWin && curLevel + 1 > maxLevel)
        {
            maxLevel = curLevel + 1;
            PlayerPrefs.SetInt("maxLevel", maxLevel);
        }
        yield return new WaitForSeconds(1f);
        resultLayer.gameObject.SetActive(true);
        menuLayer.gameObject.SetActive(false);
        winLayer.gameObject.SetActive(false);
        loseLayer.gameObject.SetActive(false);
        curLayer = isWin ? winLayer : loseLayer;
        curLayer.gameObject.SetActive(true);
        curLayer.transform.localScale = Vector3.zero;
        curLayer.transform.DOScale(Vector3.one, 0.3f);
        totalScore += curScore;

        curLayer.transform.Find("bg/lblScore").GetComponent<Text>().text = curScore.ToString();
        curLayer.transform.Find("bg/lblTotalSocre").GetComponent<Text>().text = totalScore.ToString();
        int starCount = 0;
        if (residueTime >= 70)
        {
            starCount = 3;
        }
        else if (residueTime < 70 && residueTime >= 40)
        {
            starCount = 2;
        }
        else
            starCount = 1;

        if (isWin)
        {
            for(int i = 1; i <= starCount; i++)
            {
                curLayer.Find(string.Format("bg/star_{0}", i)).gameObject.SetActive(true);
            }
            int histroyStarCount = PlayerPrefs.GetInt(string.Format("{0}starCount", curLevel), 0);
            if(starCount > histroyStarCount)
                PlayerPrefs.SetInt(string.Format("{0}starCount", curLevel), starCount);
        }       
        PlayerPrefs.SetInt("totalScore", totalScore);
    }

    IEnumerator countdown()
    {
        while(residueTime >= 0 && gameStatus == GameStatus.Start)
        {
            yield return new WaitForSeconds(1);
            residueTime -= 1;
            fgBar.fillAmount = (float)residueTime / totalTime;
            if(residueTime == 0)
            {
                StartCoroutine(showResult(false));
                break;
            }
        }
    }

    void updateMenu()
    {
        //Debug.LogError(maxLevel);
        for(int i = 1; i <= 15; i++)
        {
            int tmpI = i;
            Transform cell = menuLayer.transform.Find(string.Format("bg/cell_{0}", i));
            if (i <= maxLevel)
            {
                cell.Find("spDown").gameObject.SetActive(true);
                cell.Find("spDown/spCur").gameObject.SetActive(false);
                cell.Find("star_1").gameObject.SetActive(true);
                cell.Find("star_2").gameObject.SetActive(true);
                cell.Find("star_3").gameObject.SetActive(true);
                for (int j = 0; j < PlayerPrefs.GetInt(string.Format("{0}starCount", i), 0); j++)
                {
                    cell.Find(string.Format("star_{0}/Image", j + 1)).gameObject.SetActive(true);
                }
                cell.GetComponent<Button>().onClick.RemoveAllListeners();
                cell.GetComponent<Button>().onClick.AddListener(() =>
                {
                    audioSource.PlayOneShot(clip);
                    curLevel = tmpI;
                    generate();
                    menuLayer.transform.Find("bg").transform.DOScale(Vector3.zero, 0.3f).OnComplete(()=>
                    {
                        menuLayer.gameObject.SetActive(false);
                        resultLayer.gameObject.SetActive(false);
                    });
                });
            }                
            if(i == curLevel)
            {
                cell.Find("spDown/spCur").gameObject.SetActive(true);
                spArrow.SetParent(cell);
                spArrow.transform.localPosition = new Vector3(0, 98, 0);
                Vector3 ooo = spArrow.position;
                spArrow.localScale = Vector3.one;
            }
            
        }
    }


    public void onBtnClick(string name)
    {
        audioSource.PlayOneShot(clip);
        if(name == "btnMenu")
        {
            menuLayer.gameObject.SetActive(true);
            //resultLayer.gameObject.SetActive(false);
            menuLayer.transform.Find("bg").transform.localScale = Vector3.zero;
            menuLayer.transform.Find("bg").transform.DOScale(Vector3.one, 0.3f);
            updateMenu();
        }
        else if(name == "btnVolume")
        {
            Game.volumOpen = !Game.volumOpen;
            audioSource.volume = Game.volumOpen ? 1 : 0;
            btnVolume.transform.Find("spDisable").gameObject.SetActive(!Game.volumOpen);
        }else if(name == "btnHome")
        {
            SceneManager.LoadSceneAsync("LoginScene");
        }
        else if(name == "btnStart")
        {
            gameStatus = GameStatus.Start;
            StartCoroutine(countdown());
            btnStop.SetActive(true);
            btnStart.gameObject.SetActive(false);
        }
        else if(name == "btnStop")
        {
            gameStatus = GameStatus.Stop;
            StopCoroutine(countdown());
            btnStart.gameObject.SetActive(true);
            btnStop.SetActive(false);
        }
        else if(name == "btnRestart")
        {            
            curLayer.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
            {
                resultLayer.gameObject.SetActive(false);
            });
            generate();
        }
        else if(name == "btnNext")
        {
            curLevel += 1;
            if (curLevel > 15)
                curLevel = 1;
            curLayer.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
            {
                resultLayer.gameObject.SetActive(false);
            });
            generate();
        }
    }
}
