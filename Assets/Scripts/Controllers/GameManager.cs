using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject Win, Lose; 
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        MOVES
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        AutoPlayWin,///
        AutoPlayLose///
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        Debug.Log($"==> Đang set state: {state}");
        if (State == state) return; // Tránh set lại trạng thái cũ

        State = state;

        if (State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
       
    }

    public void LoadLevel(eLevelMode mode)
    {
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings);

        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), this);
        }

        m_levelCondition.ConditionCompleteEvent += GameOver;

        State = eStateGame.GAME_STARTED;
    }

    public void GameOver()
    {
        StartCoroutine(WaitBoardController());
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController()
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = eStateGame.GAME_OVER;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameOver;

            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }
    public void OnItemTapped(Cell cell)//
    {
        if (m_boardController != null)
        {
            m_boardController.MoveItemToBottom(cell);

            // Kiểm tra điều kiện thắng/thua sau khi di chuyển
            if (m_boardController.CheckWinCondition())
            {
                Debug.Log("You win!");
                GameOver(true); // Thắng
            }
            else if (m_boardController.CheckLoseCondition())
            {
                Debug.Log("You lose!");
                GameOver(false); // Thua
            }
        }
    }
    public void GameOver(bool isWin)
    {
        StartCoroutine(WaitBoardController(isWin));
    }
    
    private IEnumerator WaitBoardController(bool isWin)
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = eStateGame.GAME_OVER;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameOver;
            Destroy(m_levelCondition);
            m_levelCondition = null;
        }

        // Hiển thị thông báo thắng/thua
        if (isWin)
        {
            Debug.Log("Congratulations! You won the game!");
            // Hiển thị UI thắng
            Win.SetActive(true);
            Lose.SetActive(false);
        }
        else
        {
            Debug.Log("Game over! You lost the game.");
            // Hiển thị UI thua
            Win.SetActive(false);
            Lose.SetActive(true);
        }
    }
    public void StartAutoPlay()
    {
        if (State != eStateGame.AutoPlayWin) return;
        StopAllCoroutines();
        StartCoroutine(AutoPlayRoutine());
    }

    public void StopAutoPlay()
    {
        StopAllCoroutines();
    }
    private IEnumerator AutoPlayRoutine()
    {

        yield return new WaitForSeconds(1f);

        while (State == eStateGame.AutoPlayWin)
        {
            Debug.Log("==> AutoPlay đang thực hiện hành động...");

            yield return new WaitForSeconds(0.5f); 

            if (m_boardController == null) continue;

            List<Cell> validCells = m_boardController.GetAvailableCells();
            if (validCells.Count == 0)
            {
                Debug.Log("==> Không còn nước đi! Dừng AutoPlay.");
                GameOver(true);
                break;
            }

            // Ưu tiên chọn ô giúp tạo nhóm 3 nhanh nhất
            Cell bestMove = FindBestMove(validCells);
            if (bestMove != null)
            {
                m_boardController.MoveItemToBottom(bestMove);
            }
            else
            {
                Debug.LogWarning("Không tìm thấy nước đi tốt, chọn ngẫu nhiên.");
                Cell randomCell = validCells[UnityEngine.Random.Range(0, validCells.Count)];
                m_boardController.MoveItemToBottom(randomCell);
            }
           
            if (m_boardController.CheckWinCondition())
            {
                Debug.Log("AutoPlay: You win!");
                GameOver(false);
                break;
            }
        }
    }
    private Cell FindBestMove(List<Cell> validCells)
    {
        foreach (var cell in validCells)
        {
            // Giả lập di chuyển để xem có tạo được nhóm 3 không
            if (WouldCreateMatch(cell))
            {
                return cell; 
            }
        }

        // Nếu không có nhóm 3 ngay lập tức, chọn ô gần nhất với nhóm 2
        foreach (var cell in validCells)
        {
            if (WouldCreateAlmostMatch(cell))
            {
                return cell;
            }
        }

        return null; 
    }
    private bool WouldCreateMatch(Cell cell)
    {
        // Giả lập di chuyển item xuống bottomCells
        NormalItem item = cell.Item as NormalItem;
        if (item == null) return false;

        Dictionary<NormalItem.eNormalType, int> count = new Dictionary<NormalItem.eNormalType, int>();
        foreach (var bottomCell in m_boardController.bottomCells)
        {
            if (!bottomCell.IsEmpty && bottomCell.Item is NormalItem bottomItem)
            {
                if (!count.ContainsKey(bottomItem.ItemType))
                    count[bottomItem.ItemType] = 0;

                count[bottomItem.ItemType]++;
            }
        }

        // Nếu sau khi di chuyển có nhóm 3, return true
        if (count.ContainsKey(item.ItemType) && count[item.ItemType] == 2)
        {
            return true;
        }

        return false;
    }
    private bool WouldCreateAlmostMatch(Cell cell)
    {
        // Giả lập di chuyển item vào bottomCells
        NormalItem item = cell.Item as NormalItem;
        if (item == null) return false;

        Dictionary<NormalItem.eNormalType, int> count = new Dictionary<NormalItem.eNormalType, int>();
        foreach (var bottomCell in m_boardController.bottomCells)
        {
            if (!bottomCell.IsEmpty && bottomCell.Item is NormalItem bottomItem)
            {
                if (!count.ContainsKey(bottomItem.ItemType))
                    count[bottomItem.ItemType] = 0;

                count[bottomItem.ItemType]++;
            }
        }

        // Nếu sau khi di chuyển sẽ có nhóm 2 (chuẩn bị tạo nhóm 3), ưu tiên chọn ô này
        if (count.ContainsKey(item.ItemType) && count[item.ItemType] == 1)
        {
            return true;
        }

        return false;
    }
    public void StartAutoPlayLose()
    {
        if (State != eStateGame.AutoPlayLose) return;
        StopAllCoroutines();
        StartCoroutine(AutoPlayLoseRoutine());
    }
    private IEnumerator AutoPlayLoseRoutine()
    {
        yield return new WaitForSeconds(1f);
        while (State == eStateGame.AutoPlayLose)
        {
           
            yield return new WaitForSeconds(0.5f);

            if (m_boardController == null) continue;

            List<Cell> validCells = m_boardController.GetAvailableCells();
            if (validCells.Count == 0)
            {
                GameOver(false);
                break;
            }

            Cell worstMove = FindWorstMove(validCells);
            if (worstMove != null)
            {
                m_boardController.MoveItemToBottom(worstMove);
            }
            else
            {
                Cell randomCell = validCells[UnityEngine.Random.Range(0, validCells.Count)];
                m_boardController.MoveItemToBottom(randomCell);
            }

            // Kiểm tra nếu thua
            if (m_boardController.CheckLoseCondition())
            {
                Debug.Log("AutoPlay: You lose!");
                GameOver(false);
                break;
            }
        }
    }
    private Cell FindWorstMove(List<Cell> validCells)
    {
        foreach (var cell in validCells)
        {
            if (!WouldCreateMatch(cell))
            {
                return cell;
            }
        }
        return null;
    }
}
