using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckersBoard : MonoBehaviour
{
    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    // Offsets
    private Vector3 boardOffset = new Vector3(-4.0f, 1, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    private bool isWhite;
    private bool isWhiteTurn;

    private Piece selectedPiece;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private void Start()
    {
        isWhiteTurn = true;
        startDrag.x = -1;
        startDrag.y = -1;
        GenerateBoard();
    }

    private void Update()
    {
        UpdateMouseOver();

        // If it is my turn
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (Input.GetMouseButtonDown(0))
                SelectPiece(x, y);

            if (Input.GetMouseButtonUp(0))
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);

            if (selectedPiece != null)
                UpdatePieceDrag(selectedPiece);
        }
    }
    private void UpdateMouseOver()
    {
        // if its my turn.
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board"))) 
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }
    private void UpdatePieceDrag(Piece p)
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }
    
    private void SelectPiece(int x, int y)
    {
        // Out of bounds
        if (x < 0 || x >= pieces.Length || y < 0 || y >= pieces.Length)
            return;

        Piece p = pieces[x, y];
        if (p != null)
        {
            selectedPiece = p;
            startDrag = mouseOver;
        }
    }
    private void TryMove(int x1, int y1, int x2, int y2)
    {
        if (x1 < 0 || y1 < 0 || x2 < 0 || y2 < 0)
            return;
        // Multiplayer Support
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        // Out of bounds
        if (x2 < 0 || x2 > pieces.Length || y2 < 0 || y2 > pieces.Length)
        {
            if (selectedPiece != null)
                MovePiece(selectedPiece, x1, y1);
            startDrag.x = -1;
            startDrag.y = -1;
            selectedPiece = null;
            return;
        }
        if (selectedPiece != null)
        {
            // If it has not moved
            if (endDrag == startDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            // Check if move is valid
            if (selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                // Did we kill anything
                // If this is a jump
                if (Mathf.Abs(x2 - x1) == 2)
                {
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        Destroy(p);
                    }
                }

                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePiece(selectedPiece, x2, y2);

                EndTurn();
            }
        }
    }

    private void EndTurn() 
    {
        selectedPiece = null;
        startDrag = Vector2.zero;

        isWhiteTurn = !isWhiteTurn;
        CheckVictory();
    }
    private void CheckVictory()
    {

    }

    private void GenerateBoard()
    {
        // Generate White team
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 8; x += 2)
            {
                // Generate our Piece + one in x if y is an odd number
                GeneratePiece(x + (y % 2), y);
            }
        }
        // Generate Black team
        for (int y = 7; y > 4; y--)
        {
            for (int x = 0; x < 8; x += 2)
            {
                // Generate our Piece + one in x if y is an odd number
                GeneratePiece(x + (y % 2), y);
            }
        }
    }
    private void GeneratePiece(int x, int y)
    {
        GameObject go = Instantiate((y > 3) ? blackPiecePrefab : whitePiecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        p.isWhite = y > 3 ? false : true;
        pieces[x, y] = p;
        MovePiece(p, x, y);
    }
    /// <summary>
    ///     To place the pieces in the board. We have 4 variables. x and y with the board and piece offsets.
    /// </summary>
    /// <param name="p">The piece</param>
    /// <param name="x">position x</param>
    /// <param name="y">position y</param>
    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = Vector3.right * x + Vector3.forward * y + boardOffset + pieceOffset;
    }
}
