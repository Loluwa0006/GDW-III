using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Jobs;
using UnityEngine;

public class AnnouncementManager : MonoBehaviour
{

    public const float TWEEN_TO_REGULAR_SPEED_DURATION = 0.35f;


    [SerializeField] GameObject UIPanel;
    [SerializeField] TMP_Text announcementDisplay;

    List<AnnouncementData> queuedAnnouncements = new();
    AnnouncementData currentAnnouncement;

    public void QueueNewAnnouncement(params AnnouncementData[]  data)
    {
        foreach (var item in data)
        {
            queuedAnnouncements.Add(item);
        }
        queuedAnnouncements = queuedAnnouncements.OrderByDescending(a => a.priority).ToList();
        if (currentAnnouncement == null)
        {
            DisplayAnnouncement(queuedAnnouncements[0]);
        }
    }


    public void DisplayAnnouncement(AnnouncementData data)
    {
        currentAnnouncement = data;
        UIPanel.SetActive(true);
        announcementDisplay.text = data.announcementText;
        Time.timeScale = data.customTimescale;

       
    }
    private void Update()
    {
        if (currentAnnouncement != null)
        {
            currentAnnouncement.announcementDuration -= Time.unscaledDeltaTime;
            if (currentAnnouncement.announcementDuration <= 0.0f)
            {
                OnAnnouncementOver();
            }
        }
    }

    void OnAnnouncementOver()
    {
    
        if (queuedAnnouncements.Count <= 0)
        {
            UIPanel.SetActive(false);
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, TWEEN_TO_REGULAR_SPEED_DURATION)
            .SetEase(Ease.OutQuad);
            currentAnnouncement = null;
        }
        else
        {
            var next = queuedAnnouncements[0];
            queuedAnnouncements.RemoveAt(0);
            DisplayAnnouncement(next);
        }
        
    }

}


public class AnnouncementData
{
    public float customTimescale = 1.0f;
    public string announcementText = string.Empty;
    public float announcementDuration = 1.0f;
    public int priority = 1;


    public AnnouncementData() { }

    public  AnnouncementData(AnnouncementData data)
    {
        customTimescale = data.customTimescale;
        announcementText = data.announcementText;
        announcementDuration = data.announcementDuration;
        priority = data.priority;
    }
}



