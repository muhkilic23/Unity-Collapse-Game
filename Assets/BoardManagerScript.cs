using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;



public class BoardManagerScript : MonoBehaviour
{
    // Needed components
    [SerializeField] private RectTransform boardRect;
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private GameControllerScript gameControllerScript;
    [SerializeField] private GameObject backButton;


    
    // Padding and Spacing
    private Vector2 spacing = new Vector2(8f, 8f);
    private int paddingLeft = 20;
    private int paddingRight = 20;
    private int paddingTop = 300;
    private int paddingBottom = 20;
    private RectOffset padding;

    
    private Vector2[,] slotPositions;
    private int currentRows;
    private int currentCols;

    // cached cell size computed once in Build
    private Vector2 cachedCellSize = Vector2.zero;

    void Awake()
    {
        padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
    }

    public void Build(int rows, int cols)
    {

        grid.spacing = spacing;
        grid.padding = padding;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;

        // compute cell size once and cache it
        cachedCellSize = CalculateCellSize(rows, cols);
        grid.cellSize = cachedCellSize;

        // instantiate pieces and initialize their positions
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject pieceGo = Instantiate(piecePrefab, grid.transform, false);
                pieceGo.GetComponent<PieceScript>().SetPosition(r, c);
                gameControllerScript.InitializePiece(r, c, pieceGo.GetComponent<PieceScript>());
            }
        }

        // finalize layout once and then disable automatic relayouting so hiding pieces won't shift the board
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());

        // capture slot positions so we can realign pieces later when grid is disabled
        currentRows = rows;
        currentCols = cols;
        slotPositions = new Vector2[rows, cols];
        int child = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var rt = grid.transform.GetChild(child).GetComponent<RectTransform>();
                slotPositions[r, c] = rt.anchoredPosition;
                child++;
            }
        }
        grid.enabled = false;
    }

    // Calculate the size of each cell in the grid
    private Vector2 CalculateCellSize(int rows, int cols)
    {
        float availableW = boardRect.rect.width - padding.left - padding.right - spacing.x * (cols - 1);
        float availableH = boardRect.rect.height - padding.top - padding.bottom - spacing.y * (rows - 1);

        float cellW = availableW / cols;
        float cellH = availableH / rows;

        float size = Mathf.Min(cellW, cellH);
        return new Vector2(size, size);
    }

    // Place a piece's RectTransform above the screen for animation
    public void PlacePieceAboveColumn(PieceScript piece, int col, float aboveDistance = -1f)
    {
        if (piece == null || slotPositions == null || col < 0 || col >= currentCols) return;
        var rt = piece.GetComponent<RectTransform>();

        if (aboveDistance < 0f)
        {
            // make spawn height noticeably above the board (fraction of board height) + a bit of cell size
            float boardH = boardRect != null ? boardRect.rect.height : Mathf.Max(cachedCellSize.x, cachedCellSize.y) * currentRows;
            aboveDistance = boardH * 0.6f + Mathf.Max(cachedCellSize.x, cachedCellSize.y) * 1.5f;
        }

        Vector2 topPos = slotPositions[0, col];
        rt.anchoredPosition = topPos + Vector2.up * aboveDistance;
        rt.localScale = Vector3.one;
        piece.gameObject.SetActive(true);
    }

    // Animate all pieces falling into place
    public IEnumerator AnimateFallAll(float duration)
    {
        if (slotPositions == null) yield break;

        var items = new List<(RectTransform rt, Vector2 start, Vector2 end)>(currentRows * currentCols);

        for (int r = 0; r < currentRows; r++)
        {
            for (int c = 0; c < currentCols; c++)
            {
                var piece = GameControllerScript.Instance.GetPieceAt(r, c);
                if (piece == null) continue;

                var rt = piece.GetComponent<RectTransform>();

                Vector2 start = rt.anchoredPosition;
                Vector2 end = slotPositions[r, c];

                // Skip if already at target position
                if ((start - end).sqrMagnitude < 0.01f) continue;
                items.Add((rt, start, end));
            }
        }

        if (items.Count == 0) yield break;

        float invDur = duration > 0f ? 1f / duration : 999999f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * invDur;
            float tt = (t >= 1f) ? 1f : t;

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                it.rt.anchoredPosition = Vector2.Lerp(it.start, it.end, tt);
            }

            yield return null;
        }

        for (int i = 0; i < items.Count; i++)
        {
            items[i].rt.anchoredPosition = items[i].end;
        }
    }

    // back to menu
    public void onclickedBackButton()
    {
        SceneManager.LoadScene(0);
    }
}