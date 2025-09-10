using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMatchItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI matchIdText;
    [SerializeField] private Button joinMatch;

    public void SetData(string matchId, UnityAction onJoinButtonClicked)
    {
        matchIdText.text = matchId;
        joinMatch.onClick.RemoveAllListeners();
        joinMatch.onClick.AddListener(onJoinButtonClicked);
    }
}