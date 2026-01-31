using UnityEngine;
using UnityEngine.UI;

public class PieceScript : MonoBehaviour
{
    
    [SerializeField] private Sprite[] sprites = new Sprite[24];
    private PieceColor color;
    private Image image;
    private int[] position = new int[2];  // [row, col]

    private GameControllerScript gameController;

    // Piece colors for the game
    public enum PieceColor 
    {
        Blue,
        Green,
        Pink,
        Purple,
        Red,
        Yellow
    }

    void Start()
    {
        image = GetComponent<Image>();
        image.color = Color.white;      // reset color
        color = ChooseColor();
        ApplySprite(color, 0);
        gameController = GameControllerScript.Instance;
    }

    // Choose a random color for the piece
    public PieceColor ChooseColor() 
    {
        int count = SettingsHolder.colors;
        PieceColor randomColor = (PieceColor)Random.Range(0, count);
        return randomColor;
    }


    // Apply the sprite based on the color and condition
    public void ApplySprite(PieceColor color, int condition) 
    {
        int index = 0;
        switch (color) {
            case PieceColor.Blue:
                index = 0;
                break;
            case PieceColor.Green:
                index = 4;
                break;
            case PieceColor.Pink:
                index = 8;
                break;
            case PieceColor.Purple:
                index = 12;
                break;
            case PieceColor.Red:
                index = 16;
                break;
            case PieceColor.Yellow:
                index = 20;
                break;
        }
        index += condition;
        image.sprite = sprites[index];
    }

    public int[] GetPosition() { return position; }
    public void SetPosition(int row, int col) { position[0] = row; position[1] = col; }
    public PieceColor GetColor() { return color; }

    // Notifies the GameController which piece was clicked
    public void OnClick()
    {
        gameController.OnClickPiece(this);
    }

    // set color and apply matching sprite
    // used for re-coloring pieces while object is pooled or at shuffle
    public void SetColor(PieceColor newColor)
    {
        color = newColor;
        ApplySprite(newColor, 0);
    }

}