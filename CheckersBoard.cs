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

    private Piece selectedPiece;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private void Start()
    {
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
                // Generate our Piece
                GeneratePiece(x, y);
            }
        }
    }
    private void GeneratePiece(int x, int y)
    {
        GameObject go = Instantiate((y > 3) ? blackPiecePrefab : whitePiecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        pieces[x, y] = p;
        MovePiece(p, x, y);
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
            Debug.Log(selectedPiece.name);
        }
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
