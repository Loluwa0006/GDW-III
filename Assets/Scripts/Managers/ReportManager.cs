using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using XCharts.Runtime;

public class ReportManager : MonoBehaviour
{
    [Header("P1 Data")]
    [SerializeField] TMP_Text p1SkillOneUsage;
    [SerializeField] TMP_Text p1SkillTwoUsage;
    [SerializeField] TMP_Text p1SkillOneName;
    [SerializeField] TMP_Text p1SkillTwoName;
    [SerializeField] TMP_Text p1ForesightUsage;
    [SerializeField] TMP_Text p1PerfectDeflects;
    [SerializeField] TMP_Text p1PartialDeflects;
    [SerializeField] TMP_Text p1AverageDeflectTiming;
    [SerializeField] LineChart p1StaminaChart;

    [Header("P2 Data")]
    [SerializeField] TMP_Text p2SkillOneUsage;
    [SerializeField] TMP_Text p2SkillTwoUsage;
    [SerializeField] TMP_Text p2SkillOneName;
    [SerializeField] TMP_Text p2SkillTwoName;
    [SerializeField] TMP_Text p2ForesightUsage;
    [SerializeField] TMP_Text p2PerfectDeflects;
    [SerializeField] TMP_Text p2PartialDeflects;
    [SerializeField] TMP_Text p2AverageDeflectTiming;
    [SerializeField] LineChart p2StaminaChart;
    public class PlayerData
    {
        public Dictionary<int, float> staminaTracker = new();
        public int skillOneUsage;
        public int skillTwoUsage;
        public int foresightUsage;
        public int perfectDeflects;
        public int partialDeflects;
        public float averageTiming;
        public string skillOneName;
        public string skillTwoName;
        public List<float> deflectTimings = new();
    }

    public class TrackerData
    {
        public BaseSpeaker speaker;
        public MatchData.PlayerInfo speakerInfo;
    }

    public Dictionary<BaseSpeaker, PlayerData> speakerDictionary = new();

    bool trackTime;
    float timeElapsed = 0;
   
    public void OnSpeakerDeflect(BaseSpeaker speaker,bool partial, float time)
    {
        PlayerData data = speakerDictionary[speaker];
        if (partial) data.partialDeflects += 1;
        else data.perfectDeflects += 1;
        data.deflectTimings.Add(time);
    }
    public void InitManager(params TrackerData[] speakerData)
    {
        speakerDictionary.Clear();
        int index = 0;
        foreach (var data in speakerData)
        {
            speakerDictionary.Add(data.speaker, new PlayerData());
            speakerDictionary[data.speaker].skillOneName = data.speakerInfo.skillOne.ToString();
            speakerDictionary[data.speaker].skillOneName = data.speakerInfo.skillTwo.ToString();

            if (index == 0)
            {
                p1SkillOneName.text = data.speakerInfo.skillOne.ToString() + " Uses";
                p1SkillTwoName.text = data.speakerInfo.skillTwo.ToString() + " Uses";
            }
            else
            {
                p2SkillOneName.text = data.speakerInfo.skillOne.ToString() + " Uses";
                p2SkillTwoName.text = data.speakerInfo.skillTwo.ToString() + " Uses";
            }

        }
    }

    public void OnMatchStart()
    {
        timeElapsed = 0;
        trackTime = true;       
    }

    public void OnForesightUsed(BaseSpeaker speaker)
    {
        speakerDictionary[speaker].foresightUsage += 1;
    }

    public void OnSkillUsed(BaseSpeaker speaker, int index)
    {
        if (index == 0) speakerDictionary[speaker].skillOneUsage += 1;
        else speakerDictionary[speaker].skillTwoUsage += 1;
    }
    public void OnMatchEnd()
    {
        trackTime = false;

        float deflectTotal = 0;
        int index = 0;
        foreach (var data in speakerDictionary)
        {
            foreach (var timing in data.Value.deflectTimings)
            {
                deflectTotal += timing;
            }
            speakerDictionary[data.Key].averageTiming = deflectTotal / data.Value.deflectTimings.Count;
            if (index == 0)
            {
                InitStaminaChart(p1StaminaChart, data.Key);
            }
            else
            {
                InitStaminaChart(p2StaminaChart, data.Key);
            }
        }
    }

    void InitStaminaChart(LineChart chart, BaseSpeaker speaker)
    {
        var data = speakerDictionary[speaker];

        chart.ClearData();
        foreach (var point in data.staminaTracker)
        {
            chart.AddData(point.Key, point.Value);
        }
    }

    private void Update()
    {
        if (!trackTime) return;
        int prevSecond = Mathf.RoundToInt(timeElapsed);
        timeElapsed += Time.deltaTime;
        int newSecond = Mathf.RoundToInt(timeElapsed);
        if (newSecond > prevSecond)
        {
            foreach (var speaker in speakerDictionary.Keys)
            {
                speakerDictionary[speaker].staminaTracker[newSecond] = speaker.staminaComponent.GetStamina();
            }
        }

    }

}
