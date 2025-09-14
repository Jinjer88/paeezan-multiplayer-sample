using Nakama;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILeaderboardPage : UIPage
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private Button refreshLeaderboard;
    [SerializeField] private Button backButton;
    [SerializeField] private Transform recordsContainer;
    [SerializeField] private UILeaderboardRecordItem leaderboardRecordItemPrefab;

    public override void OnPageOpen()
    {
        refreshLeaderboard.onClick.AddListener(OnRefreshButtonClicked);
        backButton.onClick.AddListener(GoBack);

        foreach (Transform child in recordsContainer)
            Destroy(child.gameObject);

        if (serverController.LeaderboardRecords != null)
            UpdateRecords(serverController.LeaderboardRecords.Records);
    }

    public override void OnPageClose()
    {
        refreshLeaderboard.onClick.RemoveListener(OnRefreshButtonClicked);
        backButton.onClick.RemoveListener(GoBack);
    }

    private void GoBack()
    {
        uiController.OpenPage<UIMainMenuPage>();
    }

    private async void OnRefreshButtonClicked()
    {
        foreach (Transform child in recordsContainer)
            Destroy(child.gameObject);

        var result = await serverController.GetLeaderboardRecords();
        if (result != null)
            UpdateRecords(result.Records);
    }

    private void UpdateRecords(IEnumerable<IApiLeaderboardRecord> records)
    {
        foreach (var record in records)
        {
            var item = Instantiate(leaderboardRecordItemPrefab, recordsContainer);
            item.SetData(record.Rank, record.Username, record.Score);
        }
    }
}
