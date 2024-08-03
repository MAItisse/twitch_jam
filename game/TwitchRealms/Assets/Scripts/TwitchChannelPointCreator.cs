using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwitchSDK;
using TwitchSDK.Interop;

public class TwitchChannelPointCreator : MonoBehaviour
{
    private CustomRewardDefinition addSphere;
    List<CustomRewardDefinition> _cDefinitionList;

    private void Start()
    {
        _cDefinitionList = new List<CustomRewardDefinition>();
        addSphere = AddChannelPoint("Add Sphere To World", 1, "#FC3903");
        _cDefinitionList.Add(addSphere);
        SetSampleRewards();
    }

    private CustomRewardDefinition AddChannelPoint(string title, int cost, string color, string prompt = "", bool userInputRequired = false)
    {
        var reward = new CustomRewardDefinition
        {
            Title = title,
            Cost = cost,
            BackgroundColor = color,
            IsUserInputRequired = userInputRequired // does not work to be able to read through unity api
        };
        if (prompt != "") reward.Prompt = prompt;
        return reward;
    }

    public void SetSampleRewards()
    {
        foreach (var channelPointReward in _cDefinitionList)
        {
            channelPointReward.IsEnabled = true;
        }

        Twitch.API.ReplaceCustomRewards(_cDefinitionList.ToArray());
        Debug.Log("Rewards set!");

    }

    public void ClearRewards()
    {
        foreach (var channelPointReward in _cDefinitionList)
        {
            channelPointReward.IsEnabled = false;
        }

        Twitch.API.ReplaceCustomRewards(_cDefinitionList.ToArray());
        Debug.Log("Rewards removed!");

    }
}