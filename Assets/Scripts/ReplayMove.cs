using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ReplayMove : MonoBehaviour
{
    [SerializeField] private GameObject replayMove;
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject replayMoveIndex;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void generateReplayMoveIndex( string index) {
        var spawnedMove = Instantiate(replayMoveIndex, new Vector3(0, 0), Quaternion.identity);
        spawnedMove.transform.SetParent(parent.transform);
        TextMeshProUGUI mText = spawnedMove.GetComponent<TextMeshProUGUI>();
        mText.text = index;
    }
    public void generateReplayMove(string moveText){
       
        var spawnedMove = Instantiate(replayMove, new Vector3(0, 0, -1), Quaternion.identity);
        spawnedMove.transform.SetParent(parent.transform);
        TextMeshProUGUI mText = spawnedMove.GetComponent<TextMeshProUGUI>();
        mText.text = moveText;
    }
    //  void OnMouseDown()
    // {
    //     Debug.Log("Pressed on " + GetComponent<TextMeshProUGUI>().text);
    // }
    public void clicked () {
        Debug.Log("Pressed on " + GetComponent<TextMeshProUGUI>().text);
    }
}
