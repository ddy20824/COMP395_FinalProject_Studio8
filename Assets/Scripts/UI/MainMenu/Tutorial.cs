using System;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : BasePanel<Tutorial>
{
    [SerializeField] private Button btnNext;
    [SerializeField] private Button btnPrevious;
    [SerializeField] private Button btnClose;
    [SerializeField] private GameObject[] tutorialPages;

    private int currentPageIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        HideMe();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btnNext.onClick.AddListener(() =>
        {
            ShowNextPage();
        });

        btnPrevious.onClick.AddListener(() =>
        {
            ShowPreviousPage();
        });

        btnClose.onClick.AddListener(() =>
        {
            currentPageIndex = 0;
            HideMe();
            MainMenu.Instance.ShowMe();
        });

        UpdatePageVisibility();
    }

    private void ShowNextPage()
    {
        if (currentPageIndex < tutorialPages.Length - 1)
        {
            currentPageIndex++;
            UpdatePageVisibility();
        }
    }

    private void ShowPreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageVisibility();
        }
    }

    private void UpdatePageVisibility()
    {
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            tutorialPages[i].SetActive(i == currentPageIndex);
        }

        btnPrevious.gameObject.SetActive(currentPageIndex > 0 && currentPageIndex < tutorialPages.Length - 1);
        btnNext.gameObject.SetActive(currentPageIndex < tutorialPages.Length - 1);
        btnClose.gameObject.SetActive(currentPageIndex == tutorialPages.Length - 1);
    }
}
