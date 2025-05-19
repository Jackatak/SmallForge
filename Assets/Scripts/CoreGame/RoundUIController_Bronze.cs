using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundUIController_Bronze : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI roundNumberText;
    [SerializeField] private TextMeshProUGUI gameTimerText;
    [SerializeField] private Transform submissionFeedParent;
    [SerializeField] private GameObject submissionIconPrefab;

    [Header("Left Panel (Progress Rows)")]
    [SerializeField] private List<MetalProgressRow> progressRows;

    [Header("Bronze Recipe Setup")]
    [SerializeField] private List<ResourceObjectSO> metals; // Cu, Zn, Al, Fe, Mn
    [SerializeField] private List<int> targetAmounts;       // 58, 39, 1, 1, 1

    [Header("Dependencies")]
    [SerializeField] private RoundProgressTracker progressTracker;

    private float gameTime;
    private Queue<GameObject> submissionIcons = new Queue<GameObject>();
    private const int MaxSubmissionsShown = 10;

    private void Start()
    {
        roundNumberText.text = "Round: 1";
        SetupLeftPanel();
    }

    private void Update()
    {
        UpdateGameTimer();
        UpdateLeftPanel();
    }

    private void UpdateGameTimer()
    {
        gameTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        gameTimerText.text = $"Time: {minutes:D2}:{seconds:D2}";
    }

    private void SetupLeftPanel()
    {
        for (int i = 0; i < progressRows.Count; i++)
        {
            progressRows[i].icon.sprite = metals[i].sprite;
            progressRows[i].progressText.text = $"0 / {targetAmounts[i]}";
        }
    }

    private void UpdateLeftPanel()
    {
        for (int i = 0; i < progressRows.Count; i++)
        {
            int submitted = progressTracker.GetSubmittedCount(metals[i].GetID());
            progressRows[i].progressText.text = $"{submitted} / {targetAmounts[i]}";
        }
    }

    public void OnMetalSubmitted(ResourceObjectSO metal)
    {
        // Add to feed
        GameObject icon = Instantiate(submissionIconPrefab, submissionFeedParent);
        icon.GetComponent<Image>().sprite = metal.sprite;

        submissionIcons.Enqueue(icon);
        if (submissionIcons.Count > MaxSubmissionsShown)
        {
            Destroy(submissionIcons.Dequeue());
        }
    }
}

[System.Serializable]
public class MetalProgressRow
{
    public Image icon;
    public TextMeshProUGUI progressText;
}
