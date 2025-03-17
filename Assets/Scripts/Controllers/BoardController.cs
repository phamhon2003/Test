using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;

    //private bool m_isDragging;

    private Camera m_cam;

    private Collider2D m_hitCollider;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    private bool m_gameOver;
    public Cell[] bottomCells;//

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);
        ////////////////////////////////////
        int bottomCellCount = 5;
        bottomCells = new Cell[bottomCellCount];

        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        if (prefabBG == null)
        {
            Debug.LogError("Failed to load PREFAB_CELL_BACKGROUND!");
            return;
        }

        float boardWidth = m_gameSettings.BoardSizeX;
        float centerX = (boardWidth - 1) / 2f; // Trung tâm bảng theo trục X

        // Vị trí của ô bottomCell đầu tiên để căn giữa cả cụm
        float startX = centerX - 3.5f; // Lùi về 2.5 để cả nhóm 5 ô nằm chính giữa

        // Căn chỉnh theo trục Y
        float boardHeight = m_gameSettings.BoardSizeY;
        float bottomY = -(boardHeight / 2f) - 1.5f; // Đặt bên dưới bảng chính

        for (int i = 0; i < bottomCellCount; i++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);
            go.transform.position = new Vector3(startX + i, bottomY, 0); // Đặt chính xác vị trí theo trục X
            go.transform.SetParent(this.transform);

            Cell cell = go.GetComponent<Cell>();
            cell.Setup(i, -1); // Đặt bottomCells với tọa độ y = -1

            bottomCells[i] = cell;
        }

        ///////////////////////////////////////
        Fill();
    }

    private void Fill()
    {
        m_board.Fill();
        //FindMatchesAndCollapse();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                //StopHints();
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        if (!m_hintIsShown)
        {
            m_timeAfterFill += Time.deltaTime;
            if (m_timeAfterFill > m_gameSettings.TimeForHint)
            {
                m_timeAfterFill = 0f;
                //ShowHint();
            }
        }

        //if (Input.GetMouseButtonDown(0))
        //{
        //    var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        //    if (hit.collider != null)
        //    {
        //        m_isDragging = true;
        //        m_hitCollider = hit.collider;
        //    }
        //}

        if (Input.GetMouseButtonUp(0))
        {
            if (m_hitCollider != null)//
            {
                Cell cell = m_hitCollider.GetComponent<Cell>();
                if (cell != null)
                {
                    MoveItemToBottom(cell);

                    if (CheckWinCondition())
                    {
                        Debug.Log("You win!");
                        m_gameManager.SetState(GameManager.eStateGame.GAME_OVER);
                    }
                    else if (CheckLoseCondition())
                    {
                        Debug.Log("You lose!");
                        m_gameManager.SetState(GameManager.eStateGame.GAME_OVER);
                    }
                }
            }//
            //ResetRayCast();
        }

        //if (Input.GetMouseButton(0) && m_isDragging)
        //{
        //    var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        //    if (hit.collider != null)
        //    {
        //        if (m_hitCollider != null && m_hitCollider != hit.collider)
        //        {
        //            StopHints();

        //            Cell c1 = m_hitCollider.GetComponent<Cell>();
        //            Cell c2 = hit.collider.GetComponent<Cell>();
        //            if (AreItemsNeighbor(c1, c2))
        //            {
        //                IsBusy = true;
        //                SetSortingLayer(c1, c2);
        //                m_board.Swap(c1, c2, () =>
        //                {
        //                    FindMatchesAndCollapse(c1, c2);
        //                });

        //                ResetRayCast();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        ResetRayCast();
        //    }
        //}
    }

    //private void ResetRayCast()
    //{
    //    m_isDragging = false;
    //    m_hitCollider = null;
    //}

    //private void FindMatchesAndCollapse(Cell cell1, Cell cell2)
    //{
    //    if (cell1.Item is BonusItem)
    //    {
    //        cell1.ExplodeItem();
    //        StartCoroutine(ShiftDownItemsCoroutine());
    //    }
    //    else if (cell2.Item is BonusItem)
    //    {
    //        cell2.ExplodeItem();
    //        StartCoroutine(ShiftDownItemsCoroutine());
    //    }
    //    else
    //    {
    //        List<Cell> cells1 = GetMatches(cell1);
    //        List<Cell> cells2 = GetMatches(cell2);

    //        List<Cell> matches = new List<Cell>();
    //        matches.AddRange(cells1);
    //        matches.AddRange(cells2);
    //        matches = matches.Distinct().ToList();

    //        if (matches.Count < m_gameSettings.MatchesMin)
    //        {
    //            m_board.Swap(cell1, cell2, () =>
    //            {
    //                IsBusy = false;
    //            });
    //        }
    //        else
    //        {
    //            OnMoveEvent();

    //            CollapseMatches(matches, cell2);
    //        }
    //    }
    //}

    //private void FindMatchesAndCollapse()
    //{
    //    List<Cell> matches = m_board.FindFirstMatch();

    //    if (matches.Count > 0)
    //    {
    //        CollapseMatches(matches, null);
    //    }
    //    else
    //    {
    //        m_potentialMatch = m_board.GetPotentialMatches();
    //        if (m_potentialMatch.Count > 0)
    //        {
    //            IsBusy = false;

    //            m_timeAfterFill = 0f;
    //        }
    //        else
    //        {
    //            //StartCoroutine(RefillBoardCoroutine());
    //            StartCoroutine(ShuffleBoardCoroutine());
    //        }
    //    }
    //}

    private List<Cell> GetMatches(Cell cell)
    {
        List<Cell> listHor = m_board.GetHorizontalMatches(cell);
        if (listHor.Count < m_gameSettings.MatchesMin)
        {
            listHor.Clear();
        }

        List<Cell> listVert = m_board.GetVerticalMatches(cell);
        if (listVert.Count < m_gameSettings.MatchesMin)
        {
            listVert.Clear();
        }

        return listHor.Concat(listVert).Distinct().ToList();
    }

    private void CollapseMatches(List<Cell> matches, Cell cellEnd)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }

        if(matches.Count > m_gameSettings.MatchesMin)
        {
            m_board.ConvertNormalToBonus(matches, cellEnd);
        }

        StartCoroutine(ShiftDownItemsCoroutine());
    }

    private IEnumerator ShiftDownItemsCoroutine()
    {
        m_board.ShiftDownItems();

        yield return new WaitForSeconds(0.2f);

        m_board.FillGapsWithNewItems();

        yield return new WaitForSeconds(0.2f);

        //FindMatchesAndCollapse();
    }

    private IEnumerator RefillBoardCoroutine()
    {
        m_board.ExplodeAllItems();

        yield return new WaitForSeconds(0.2f);

        m_board.Fill();

        yield return new WaitForSeconds(0.2f);

        //FindMatchesAndCollapse();
    }

    private IEnumerator ShuffleBoardCoroutine()
    {
        m_board.Shuffle();

        yield return new WaitForSeconds(0.3f);

        //FindMatchesAndCollapse();
    }


    private void SetSortingLayer(Cell cell1, Cell cell2)
    {
        if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
        if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    }

    private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    {
        return cell1.IsNeighbour(cell2);
    }

    internal void Clear()
    {
        m_board.Clear();
    }

    //private void ShowHint()
    //{
    //    m_hintIsShown = true;
    //    foreach (var cell in m_potentialMatch)
    //    {
    //        cell.AnimateItemForHint();
    //    }
    //}

    //private void StopHints()
    //{
    //    m_hintIsShown = false;
    //    foreach (var cell in m_potentialMatch)
    //    {
    //        cell.StopHintAnimation();
    //    }

    //    m_potentialMatch.Clear();
    //}
    private bool isAnimating = false; // Biến kiểm tra hiệu ứng có đang chạy không

    public void MoveItemToBottom(Cell cell)
    {
        if (cell == null || cell.IsEmpty || isAnimating) return; // Nếu đang chạy hiệu ứng thì không di chuyển tiếp
        if (!(cell.Item is NormalItem)) return;

        // Tìm ô trống đầu tiên trong bottomCells
        Cell targetBottomCell = bottomCells.FirstOrDefault(c => c.IsEmpty);

        if (targetBottomCell == null)
        {
            Debug.Log("Bottom cells are full! Cannot move more items.");
            return;
        }

        isAnimating = true; 

        Transform itemTransform = cell.Item.View.transform;

        itemTransform.DOScale(0.5f, 0.2f).OnComplete(() =>
        {
            Vector3 targetPosition = targetBottomCell.transform.position;
            
            itemTransform.DOMove(targetPosition, 0.3f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                // Kiểm tra lại xem bottomCell có bị chiếm chưa
                if (targetBottomCell.IsEmpty)
                {                  
                    itemTransform.DOScale(1f, 0.2f).OnComplete(() =>
                    {
                        targetBottomCell.Assign(cell.Item);
                        targetBottomCell.ApplyItemPosition(true);
                        cell.Free(); 

                        Debug.Log($"Item moved to bottom cell [{targetBottomCell.BoardX}]");

                        isAnimating = false; 
                        CheckAndClearBottomCells();
                    });
                }
                else
                {
                    isAnimating = false; 
                }
            });
        });
    }

    private void CheckAndClearBottomCells()
    {
        Dictionary<NormalItem.eNormalType, List<Cell>> groups = new Dictionary<NormalItem.eNormalType, List<Cell>>();

        foreach (var cell in bottomCells)
        {
            if (cell.IsEmpty || !(cell.Item is NormalItem)) continue;

            NormalItem item = (NormalItem)cell.Item;
            if (!groups.ContainsKey(item.ItemType))
            {
                groups[item.ItemType] = new List<Cell>();
            }
            groups[item.ItemType].Add(cell);
        }

        foreach (var group in groups.Values)
        {
            if (group.Count >= 3)
            {
                foreach (var cell in group)
                {
                    Transform itemTransform = cell.Item.View.transform;

                    itemTransform.DOScale(0f, 0.3f).OnComplete(() =>
                    {
                        cell.ExplodeItem();
                        cell.Free(); 
                    });
                }
            }
        }
    }
    public bool CheckWinCondition()
    {
        foreach (var cell in m_board.Cells)
        {
            if (!cell.IsEmpty) return false;
        }
        return true;
    }

    public bool CheckLoseCondition()
    {
        foreach (var cell in bottomCells)
        {
            if (cell.IsEmpty) return false;
        }
        return true;
    }
    public List<Cell> GetAvailableCells()
    {
        List<Cell> availableCells = new List<Cell>();

        foreach (Cell cell in m_board.Cells)
        {
            if (!cell.IsEmpty && cell.Item is NormalItem)
            {
                availableCells.Add(cell);
            }
        }

        return availableCells;
    }
}


