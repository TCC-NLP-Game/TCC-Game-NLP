﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convai.Scripts;
using Convai.Scripts.Narrative_Design.Models;
using Newtonsoft.Json;
using UnityEngine;
using Logger = Convai.Scripts.Utils.Logger;

/// <summary>
///     Manages the narrative design for a ConvaiNPC.
/// </summary>
[RequireComponent(typeof(ConvaiNPC))]
public class NarrativeDesignManager : MonoBehaviour
{
    public List<SectionChangeEventsData> sectionChangeEventsDataList = new();
    public List<SectionData> sectionDataList = new();
    public List<TriggerData> triggerDataList = new();
    private ConvaiNPC _convaiNpc;
    private string _currentSectionID;
    private NarrativeDesignAPI _narrativeDesignAPI;

    private NarrativeDesignAPI NarrativeDesignAPI => _narrativeDesignAPI ??= new NarrativeDesignAPI();
    private ConvaiNPC ConvaiNpc => _convaiNpc ??= GetComponent<ConvaiNPC>();
    private string CharacterID => ConvaiNpc.characterID;

    private async void Awake()
    {
        _convaiNpc = GetComponent<ConvaiNPC>();
        await Task.WhenAll(UpdateSectionListAsync(), UpdateTriggerListAsync());
    }

    private async void Reset() => await Task.WhenAll(UpdateSectionListAsync(), UpdateTriggerListAsync());

    /// <summary>
    ///     Updates the section list from the server.
    /// </summary>
    public async Task UpdateSectionListAsync()
    {
        List<SectionData> updatedSectionList = await GetSectionListFromServerAsync();
        UpdateSectionDataList(updatedSectionList);
    }

    /// <summary>
    ///     Updates the trigger list from the server.
    /// </summary>
    public async Task UpdateTriggerListAsync() => await ListTriggersAsync(CharacterID);

    /// <summary>
    ///     Invoked when the section event list changes.
    /// </summary>
    public void OnSectionEventListChange()
    {
        foreach (SectionChangeEventsData sectionChangeEventsData in sectionChangeEventsDataList) sectionChangeEventsData.Initialize(this);
    }

    private async Task<List<SectionData>> GetSectionListFromServerAsync()
    {
        try
        {
            string sections = await NarrativeDesignAPI.ListSectionsAsync(CharacterID);
            return JsonConvert.DeserializeObject<List<SectionData>>(sections);
        }
        catch (Exception e)
        {
            Logger.Error($"Please setup API Key properly. FormatException occurred: {e.Message}", Logger.LogCategory.Character);
            throw;
        }
    }

    private async Task ListTriggersAsync(string characterId)
    {
        try
        {
            string triggers = await NarrativeDesignAPI.GetTriggerListAsync(characterId);
            triggerDataList = JsonConvert.DeserializeObject<List<TriggerData>>(triggers);
        }
        catch (FormatException e)
        {
            Debug.LogError($"Format Exception occurred: {e.Message}");
            throw;
        }
    }


    /// <summary>
    ///     Updates the current section.
    /// </summary>
    /// <param name="sectionID"> The section ID to update to. </param>
    public void UpdateCurrentSection(string sectionID)
    {
        if (string.IsNullOrEmpty(_currentSectionID))
        {
            _currentSectionID = sectionID;
            InvokeSectionEvent(_currentSectionID, true);
            return;
        }

        if (_currentSectionID.Equals(sectionID))
            return;

        InvokeSectionEvent(_currentSectionID, false);
        _currentSectionID = sectionID;
        InvokeSectionEvent(_currentSectionID, true);
    }

    private void InvokeSectionEvent(string id, bool isStarting)
    {
        SectionChangeEventsData sectionChangeEventsData = sectionChangeEventsDataList.Find(x => x.id == id);

        if (sectionChangeEventsData == null)
        {
            Logger.Info($"No Section Change Events have been created for sectionID: {id}", Logger.LogCategory.Actions);
            return;
        }

        if (isStarting)
            sectionChangeEventsData.onSectionStart?.Invoke();
        else
            sectionChangeEventsData.onSectionEnd?.Invoke();
    }

    private void UpdateSectionDataList(List<SectionData> updatedSectionList)
    {
        Dictionary<string, SectionData> updatedSectionDictionary = updatedSectionList.ToDictionary(s => s.sectionId);

        foreach (SectionData currentSection in sectionDataList)
            if (updatedSectionDictionary.TryGetValue(currentSection.sectionId, out SectionData updatedSection))
            {
                currentSection.sectionName = updatedSection.sectionName;
                currentSection.objective = updatedSection.objective;
                updatedSectionDictionary.Remove(currentSection.sectionId);
            }

        foreach (SectionData newSection in updatedSectionDictionary.Values) sectionDataList.Add(newSection);

        foreach (SectionChangeEventsData sectionChangeEvent in sectionChangeEventsDataList) sectionChangeEvent.Initialize(this);
    }
}