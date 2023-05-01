using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelSpecification : ScriptableObject
{
    [TextArea(2, 5)]
    public string narrativeText;
    [TextArea(1, 3)]
    public string targetBlazon;
    [TextArea(1, 3)]
    public List<string> distractorBlazons;


    [ContextMenu("Validate Blazons")]
    public void ValidateBlazons() {
        try {
            Blazon test = Blazon.Parse(targetBlazon);
        }
        catch(BlazonError e) {
            Debug.LogErrorFormat("targetBlazon Error: {0}",e.Message);
        }
        for (int i = 0; i < distractorBlazons.Count; i++) { 
            try {
                Blazon test = Blazon.Parse(distractorBlazons[i]);
            }
            catch (BlazonError e) {
                Debug.LogErrorFormat("distractorBlazon[{0}] Error: {1}", i, e.Message);
            }
        }
    }
}
