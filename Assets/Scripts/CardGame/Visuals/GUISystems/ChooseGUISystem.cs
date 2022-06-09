using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink2Unity;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public interface IChoiceSlotReciver
{
    void ChoiceSlotReciver(object msg);
}

public class ChooseGUISystem : MonoBehaviour, IGUISystem
{
    [SerializeField]
    private List<ChoiceSlotObject> chooseButtons = new List<ChoiceSlotObject>();
    [SerializeField]
    private Button lastButton = null;
    [SerializeField]
    private Button nextButton = null;
    [SerializeField]
    private List<ChoiceSlot> allChoices = new List<ChoiceSlot>();
    [SerializeField]
    private UnityEvent<ChoiceSlot> onChoose = new UnityEvent<ChoiceSlot>();

    private int pageIndex = 0;

    public IReadOnlyList<ChoiceSlot> Choices { get => allChoices; }

    public UnityEvent<ChoiceSlot> OnChoose { get => onChoose; }
    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Open(object msg)
    {
        if (!(msg is List<Choice> choices)) return;
        gameObject.SetActive(true);
        pageIndex = 0;
        allChoices.Clear();
        foreach (Choice choice in choices)
        {
            allChoices.Add(new ChoiceSlot { Choice = choice });
        }

        UpdateVisuals();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }


    private class ChoiceConditionPair
    {
        public ChoiceSlot choice;
        public PersonalityType speechType;
    }
    public void RandomReveal(int num)
    {
        List<ChoiceConditionPair> l = new List<ChoiceConditionPair>();
        foreach (ChoiceSlot choice in allChoices)
        {
            var unmasked = choice.PickupAllUnmasked();
            if(unmasked != null)
            {
                foreach (PersonalityType type in unmasked)
                {
                    l.Add(new ChoiceConditionPair { choice = choice, speechType = type });
                }
            }
        }
        for (int i = 0; i < num && l.Count > 0; i++)
        {
            ChoiceConditionPair luck = l[Random.Range(0, l.Count)];
            luck.choice.AddMask(luck.speechType);
            l.Remove(luck);
        }
    }

    public void RandomReveal(SpeechType type, int num)
    {
        List<ChoiceConditionPair> l = new List<ChoiceConditionPair>();
        foreach (ChoiceSlot choice in allChoices)
        {
            if (choice.Choice.SpeechType != type) return;
            var unmasked = choice.PickupAllUnmasked();
            if (unmasked != null)
            {
                foreach (PersonalityType p in unmasked)
                {
                    l.Add(new ChoiceConditionPair { choice = choice, speechType = p });
                }
            }
        }
        for (int i = 0; i < num && l.Count > 0; i++)
        {
            ChoiceConditionPair luck = l[Random.Range(0, l.Count)];
            luck.choice.AddMask(luck.speechType);
            l.Remove(luck);
        }
    }
    public void UpdateVisuals()
    {
        if (chooseButtons.Count == 0) return;
        int page = pageIndex / chooseButtons.Count;
        int totalPage = Mathf.CeilToInt(1f * allChoices.Count / chooseButtons.Count);
        if (lastButton) lastButton.interactable = page > 0;
        if (nextButton) nextButton.interactable = page + 1 < totalPage;
        for (int i = 0; i < chooseButtons.Count; i++)
        {
            if (pageIndex + i < allChoices.Count)
            {
                chooseButtons[i].gameObject.SetActive(true);
                chooseButtons[i].ChoiceSlot = allChoices[pageIndex + i];
                chooseButtons[i].UpdateVisuals();
            }
            else
            {
                chooseButtons[i].gameObject.SetActive(false);
            }
        }
    }




    public List<ChoiceSlot> GetChoiceSlot(SpeechType type)
    {
        return allChoices.FindAll(x => x.Choice.SpeechType == type);
    }

    public void PreviousPage()
    {
        pageIndex -= chooseButtons.Count;
        UpdateVisuals();
    }

    public void NextPage()
    {
        pageIndex += chooseButtons.Count;
        UpdateVisuals();
    }

    public void SelectChoice()
    {
        GameObject o = EventSystem.current.currentSelectedGameObject;
        if (o == null) return;
        ChoiceSlotObject c = o.GetComponentInParent<ChoiceSlotObject>();
        if (c == null) return;
        Debug.Log("Select Choice " + c.ChoiceSlot.Choice.Content);
        OnChoose.Invoke(c.ChoiceSlot);
        SendMessageUpwards(nameof(IChoiceSlotReciver.ChoiceSlotReciver), c.ChoiceSlot, SendMessageOptions.RequireReceiver);
    }
}
