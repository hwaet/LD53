using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelSpecification : ScriptableObject
{
    public string narrativeText;

    public string targetBlazon;

    public List<string> distractorBlazons;
}
