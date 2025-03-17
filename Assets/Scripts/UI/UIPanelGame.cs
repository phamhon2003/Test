using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGame : MonoBehaviour,IMenu
{
    public Text LevelConditionView;

    [SerializeField] private Button btnPause;
    [SerializeField] private Button AutoPlayWin;
    [SerializeField] private Button AutoPlayLose;
    private UIMainManager m_mngr;

    private void Awake()
    {
        btnPause.onClick.AddListener(OnClickPause);
        AutoPlayWin.onClick.AddListener(OnClickAutoPlayWin);
        AutoPlayLose.onClick.AddListener(OnClickAutoPlayLose);
    }

    private void OnClickPause()
    {
        m_mngr.ShowPauseMenu();
    }
    private void OnClickAutoPlayWin()
    {
        m_mngr.ShowAutoPlayWin();
    }
    private void OnClickAutoPlayLose()
    {
        m_mngr.ShowAutoPlayLose();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
