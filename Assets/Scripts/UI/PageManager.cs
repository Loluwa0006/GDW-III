using System.Collections.Generic;
using UnityEngine;

public class PageManager : MonoBehaviour
{
    [SerializeField] List<GameObject> pages = new();

    Dictionary<string, GameObject> pageDict = new();

    GameObject currentPage;

    private void Start()
    {
        foreach (var page in pages)
        {
            pageDict[page.name] = page;
            page.SetActive(false);
        }
        currentPage = pages[0];
        currentPage.SetActive(true);
    }

    public void TransitionToNextPage()
    {
        int currentIndex = pages.IndexOf(currentPage);
        int nextPage = currentIndex + 1;
        if (pages.Count  - 1 > nextPage)
        {
            nextPage = 0;
        }

        currentPage.SetActive(false);
        currentPage = pages[nextPage];
        currentPage.SetActive(true);
    }

    public void TransitionToPage(string pageName)
    {
        if (currentPage.name == pageName) { return; }
        if (!pageDict.ContainsKey(pageName)) { return; }
        
        currentPage.SetActive(false);
        currentPage = pageDict[pageName];
        currentPage.SetActive(true);
    }

    public void TransitionToPage(GameObject page)
    {
        if (currentPage == page) { return; }
        if (!pageDict.ContainsKey(page.name))
        {
            Debug.LogWarning("Page did not exist in dict, adding it");
            pageDict[page.name] = page;
            pages.Add(page);
        }

        currentPage.SetActive(false);
        currentPage = page; 
        currentPage.SetActive(true);
    }




}
