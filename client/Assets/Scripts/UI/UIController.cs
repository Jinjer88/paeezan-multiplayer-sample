using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/UI Controller", order = 51)]
public class UIController : ScriptableObject
{
    private Dictionary<string, UIPage> pages = new Dictionary<string, UIPage>();
    private UIPage currentPage;

    private void OnEnable()
    {
        pages = new Dictionary<string, UIPage>();
        currentPage = null;
    }

    public void AddPage(UIPage newPage)
    {
        string pageName = newPage.GetType().Name;
        if (!pages.ContainsKey(pageName))
        {
            pages.Add(pageName, newPage);
            Debug.Log($"UIController - AddPage, added page: {pageName}");
        }
    }

    public void OpenPage<T>() where T : UIPage
    {
        string pageName = typeof(T).Name;
        Debug.Log($"UIController - OpenPage, target page: {pageName}, current page: {currentPage?.name}");
        if (pages.TryGetValue(pageName, out UIPage pageToOpen))
        {
            if (currentPage != null)
            {
                currentPage.SetPageActive(false);
            }
            currentPage = pageToOpen;
            pageToOpen.SetPageActive(true);
        }
    }
}