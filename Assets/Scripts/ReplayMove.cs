using UnityEngine;
using TMPro;
using System;

public class ReplayMove : MonoBehaviour
{
    [SerializeField] private GameObject replayMove;
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject replayMoveIndex;
    private Grid grid;
    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        grid = GameObject.FindGameObjectWithTag("GameController").GetComponent<Grid>();
    }

    //Create a replay move object which holds the chess notation of the move
    public void generateReplayMove(string moveText, int index)
    {
        GameObject spawnedMove = Instantiate(replayMove, new Vector3(0, 0, -1), Quaternion.identity);
        spawnedMove.transform.SetParent(parent.transform); // set the parent for the object to be inside the scrollArea
        TextMeshProUGUI mText = spawnedMove.GetComponent<TextMeshProUGUI>();
        mText.text = moveText;
        spawnedMove.name = $"Move {index}"; //change the text
    }
    //Gets the index of the move from its name and calls a function to set the board to that specific position
    public void clicked()
    {
        int index = Convert.ToInt32(this.name.Substring(4));
        grid.replayNumMoves(index + 1);

    }
    public void underlineText()
    {
        TextMeshProUGUI mText = GetComponent<TextMeshProUGUI>();
        mText.fontStyle = FontStyles.Underline;
    }
    public void resetStyle()
    {
        TextMeshProUGUI mText = GetComponent<TextMeshProUGUI>();
        mText.fontStyle = FontStyles.Normal;
    }
}
