using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ReplayMove : MonoBehaviour
{
    [SerializeField] private GameObject replayMove;
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject replayMoveIndex;
    private Grid grid;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void Awake()
    {
        grid = GameObject.FindGameObjectWithTag("GameController").GetComponent<Grid>();
    }
    public void generateReplayMoveIndex(string index)
    {
        var spawnedMove = Instantiate(replayMoveIndex, new Vector3(0, 0), Quaternion.identity);
        spawnedMove.transform.SetParent(parent.transform);
        TextMeshProUGUI mText = spawnedMove.GetComponent<TextMeshProUGUI>();
        mText.text = index;
    }
    public void generateReplayMove(string moveText, int index)
    {

        var spawnedMove = Instantiate(replayMove, new Vector3(0, 0, -1), Quaternion.identity);
        spawnedMove.transform.SetParent(parent.transform);
        TextMeshProUGUI mText = spawnedMove.GetComponent<TextMeshProUGUI>();
        mText.text = moveText;
        spawnedMove.name = $"Move {index}";
    }
    //  void OnMouseDown()
    // {
    //     Debug.Log("Pressed on " + GetComponent<TextMeshProUGUI>().text);
    // }
    public void clicked()
    {
        int index = Convert.ToInt32(this.name.Substring(4));
        Debug.Log(index);
        grid.replayNumMoves(index + 1);

    }
}
