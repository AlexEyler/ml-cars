using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CarAgent))]
public class HUD : MonoBehaviour
{
    public List<CarAgent> CarAgents;
    private TextMeshProUGUI textMesh;

    void Start()
    {
        this.textMesh = this.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        StringBuilder builder = new StringBuilder();
        foreach (var carAgent in this.CarAgents)
        {
            if (carAgent != null && carAgent.isActiveAndEnabled)
            {
                builder.AppendLine(carAgent.name);
                builder.AppendLine("-----");
                builder.AppendLine($"Steps: {carAgent.StepCount}");
                builder.AppendLine($"Episodes: {carAgent.CompletedEpisodes}");
                builder.AppendLine($"Successes: {carAgent.SuccessfulCompletions}");
                builder.AppendLine($"Reward: {carAgent.GetCumulativeReward():0,0.00}");
                var maxReward = carAgent.MaxCumulativeReward.HasValue ? $"{carAgent.MaxCumulativeReward:0,0.00}" : "N/A";
                builder.AppendLine($"Max reward: {maxReward}");
                builder.AppendLine("-----");
            }
        }

        this.textMesh.text = builder.ToString();
    }
}
