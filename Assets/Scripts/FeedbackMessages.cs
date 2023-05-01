using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FeedbackMessages : ScriptableObject {
    [TextArea(2, 5)]
    public List<string> wrongHouseReactions;
    [TextArea(2, 5)]
    public List<string> SReactions;
    [TextArea(2, 5)]
    public List<string> AReactions;
    [TextArea(2, 5)]
    public List<string> BReactions;
    [TextArea(2, 5)]
    public List<string> CReactions;
    [TextArea(2, 5)]
    public List<string> DReactions;
    [TextArea(2, 5)]
    public List<string> FReactions;


    public string GetWrongHouseReaction() {
        return wrongHouseReactions[Random.Range(0, wrongHouseReactions.Count)];
    }

    public string GetSReaction() {
        return SReactions[Random.Range(0, SReactions.Count)];
    }
    public string GetAReaction() {
        return AReactions[Random.Range(0, AReactions.Count)];
    }
    public string GetBReaction() {
        return BReactions[Random.Range(0, BReactions.Count)];
    }
    public string GetCReaction() {
        return CReactions[Random.Range(0, CReactions.Count)];
    }
    public string GetDReaction() {
        return DReactions[Random.Range(0, DReactions.Count)];
    }
    public string GetFReaction() {
        return FReactions[Random.Range(0, FReactions.Count)];
    }
}
