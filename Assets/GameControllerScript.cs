using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class GameControllerScript : MonoBehaviour
{
    [SerializeField] private BoardManagerScript boardManager;
    public static GameControllerScript Instance { get; private set; }

    private PieceScript[,] pieces;
    private int rows;
    private int cols;

    

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rows = SettingsHolder.Rows;
        cols = SettingsHolder.Cols;
        pieces = new PieceScript[rows, cols];
        boardManager.Build(rows, cols);
        StartCoroutine(ApplyIconsToGroups());
    }

    // It will be called just once by BoardManager, when board is created 
    public void InitializePiece(int row, int col, PieceScript newPiece)
    {
        pieces[row, col] = newPiece;
    }

    // Handle everything at piece click
    public void OnClickPiece(PieceScript piece)
    {
        int[] position = piece.GetPosition();
        List<int> group = FindGroup(position[0], position[1]);
        if (group.Count < 2) return;
        StartCoroutine(DoFallSequence(group));
    }

    private IEnumerator DoFallSequence(List<int> group)
    {
        // disable and collect
        List<PieceScript> removed = DisablePieces(group);

        // logical gravity and refill
        ApplyLogicalGravity();
        ReuseRemoved(removed);

        // place reused pieces above the screen for animation
        foreach (var p in removed)
        {
            int[] pos = p.GetPosition();
            boardManager.PlacePieceAboveColumn(p, pos[1], -1f);
        }

        // animate visuals 
        yield return boardManager.AnimateFallAll(0.30f);
        StartCoroutine(ApplyIconsToGroups());

    }


    // Disable pieces and return the list of disabled pieces for reuse
    List<PieceScript> DisablePieces(List<int> group)
    {
        List<PieceScript> removed = new List<PieceScript>();
        foreach (int id in group)
        {
            int row = id / cols;
            int col = id % cols;
            PieceScript piece = pieces[row, col];
            if (piece != null)
            {
                piece.gameObject.SetActive(false);
                removed.Add(piece);
                pieces[row, col] = null;
            }
        }
        return removed;
    }

    // Reuse disabled pieces: assign a new random color, enable them and update logical positions in pieces[,]
    public void ReuseRemoved(List<PieceScript> removed)
{
    int index = 0;

    for (int c = 0; c < cols; c++)
    {
        for (int r = 0; r < rows; r++)
        {
            if (pieces[r, c] != null) continue;

            var p = removed[index++];

            p.SetColor(p.ChooseColor());
            pieces[r, c] = p;
            p.SetPosition(r, c);
            p.gameObject.SetActive(true);
        }
    }
}

    // Trace all groups
    // If there is a deadlock situation handles it
    List<List<int>> TraceGroups()
    {
        List<List<int>> allGroups = new List<List<int>>();
        bool[,] traced = new bool[rows, cols];
        bool hasBlast = false;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!traced[r, c])
                {
                    List<int> group = FindGroup(r, c);
                    if (group.Count >= 2) { hasBlast = true; }
                    for (int i = 0; i < group.Count; i++)
                    {
                        traced[group[i] / cols, group[i] % cols] = true;
                    }
                    allGroups.Add(group);
                }
            }
        }
        if (!hasBlast)
        {
            MIX(allGroups);
        }
        return allGroups;
    }

    // Shuffle the board
    public void MIX(List<List<int>> allGroups)
    {
        // Re-color for every piece
        foreach (var group in allGroups)
        {
            foreach (var id in group)
            {
                int row = id / cols;
                int col = id % cols;
                PieceScript piece = pieces[row, col];
                piece.SetColor(piece.ChooseColor());
            }
        }
        // Makes sure there is blast
        int randRow = Random.Range(0, rows);
        int randCol = Random.Range(0, cols);
        PieceScript randomPiece = pieces[randRow, randCol];
        if (randomPiece != null)
        {
            if (randRow - 1 >= 0)
            {
                PieceScript abovePiece = pieces[randRow - 1, randCol];
                randomPiece.SetColor(abovePiece.GetColor());
            }
            else if (randRow + 1 < rows)
            {
                PieceScript belowPiece = pieces[randRow + 1, randCol];
                randomPiece.SetColor(belowPiece.GetColor());
            }
            else if (randCol - 1 >= 0)
            {
                PieceScript leftPiece = pieces[randRow, randCol - 1];
                randomPiece.SetColor(leftPiece.GetColor());
            }
            else if (randCol + 1 < cols)
            {
                PieceScript rightPiece = pieces[randRow, randCol + 1];
                randomPiece.SetColor(rightPiece.GetColor());
            }
        }
    }

    // Find all connected pieces of the same color with BFS
    List<int> FindGroup(int startRow, int startCol)
    {
        List<int> group = new List<int>();

        PieceScript startPiece = pieces[startRow, startCol];
        PieceScript.PieceColor startColor = startPiece.GetColor();


        bool[,] visited = new bool[rows, cols];
        Queue<int> queue = new Queue<int>();

        int startId = startRow * cols + startCol;
        queue.Enqueue(startId);
        visited[startRow, startCol] = true;

        while (queue.Count > 0)
        {
            int id = queue.Dequeue();
            int r = id / cols;
            int c = id % cols;

            group.Add(id);

            // 4 directions
            Check(r - 1, c);
            Check(r, c + 1);
            Check(r + 1, c);
            Check(r, c - 1);
            
        }
        
        // Checks neighbors colors
        void Check(int r, int c)
        {
            if (r < 0 || r >= rows || c < 0 || c >= cols)
                return;

            if (visited[r, c])
                return;

            PieceScript p = pieces[r, c];
            if (p.GetColor() != startColor)
                return;

            visited[r, c] = true;
            queue.Enqueue(r * cols + c);
        }
        
        return group;
    }


    // Apply gravity logically, called after disabling group elements.
    public void ApplyLogicalGravity()
    {
        for (int c = 0; c < cols; c++)
        {
            int emptyBelow = 0;
            // scan bottom-up so emptyBelow counts nulls under current cell
            for (int r = rows - 1; r >= 0; r--)
            {
                if (pieces[r, c] == null)
                {
                    emptyBelow++;
                }
                else
                {
                    // only move/clear when there are empty cells below
                    if (emptyBelow > 0)
                    {
                        pieces[r + emptyBelow, c] = pieces[r, c];
                        pieces[r, c] = null;
                    }
                    // update stored position on the piece
                    pieces[r + emptyBelow, c].SetPosition(r + emptyBelow, c);
                }
            }
        }
    }

    // Return piece at logical coordinates
    public PieceScript GetPieceAt(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols) return null;
        return pieces[row, col];
    }

    // Apply sprite according to condition
    IEnumerator ApplyIconsToGroups()
    {
        yield return null;

        var groups = TraceGroups(); 

        foreach (var group in groups)
        {
            int size = group.Count;
            int condition = 0;
            if (size >= SettingsHolder.thirdCondition) condition = 3;
            else if (size >= SettingsHolder.secondCondition) condition = 2;
            else if (size >= SettingsHolder.firstCondition) condition = 1;
            else condition = 0;

            foreach (int id in group)
            {
                int r = id / cols;
                int c = id % cols;
                pieces[r, c].ApplySprite(pieces[r, c].GetColor(), condition);
            }
        }
    }
 
}