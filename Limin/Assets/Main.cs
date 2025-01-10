using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class Main : MonoBehaviour
{
    public int canvasWidth = 1900;
    public int canvasHeight = 2200;

    public int spacingLeft = 90;
    public int spacingTop = 90;
    public int spacingRight = 90;
    public int spacingBottom = 90;

    public Color backgroundColor = new Color(0.02f, 0.12f, 0.21f);
    public Color letterColor = new Color(.65f, .64f, .27f);

    public bool bold = false;
    public int LetterSpacing = 1;

    public Texture2D canvas;

    string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ .?!\"'*()";

    //----------------------Better dont touch those
    private readonly int LetterWidth = 13;
    private readonly int LetterHeight = 23;
    //---------------------------------------------

    public int verticalLetterSpacing = 1;
    public int horizontalLineSpacing = 22;

    private int carretPositionX;
    private int carretPositionY;

    public string Text;

    private Texture2D sheet;
    private List<Texture2D> letters = new List<Texture2D>();
    private List<Texture2D> lettersBold = new List<Texture2D>();

    void Start()
    {
        carretPositionX = spacingLeft;
        carretPositionY = canvasHeight - spacingTop;

        sheet = Resources.Load<Texture2D>("sheet");

        GenerateLetters(sheet, letters, false);
        GenerateLetters(sheet, lettersBold, true);

        canvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGB24, false, false);

        for (int x = 0; x < canvasWidth; x++)
            for (int y = 0; y < canvasHeight; y++)
                canvas.SetPixel( x,  y, backgroundColor);

        canvas.Apply();
    }

    void Update()
    {

        UpdateCanvas(GetSample());

        if (Input.GetKeyDown(KeyCode.Return))
            SaveTexture(canvas);

        if (Input.GetKeyDown(KeyCode.Backspace))
            StringToText(Text);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SetCarretPosition();

    }

    Texture2D GetSample()
    {
        foreach (var entry in keyToIndex)
            if (Input.GetKeyDown(entry.Key))
            {
                if (!bold)
                    return letters[entry.Value];
                else
                    return lettersBold[entry.Value];
            }

            return null;
    }

    Texture2D GetSample(int index)
    {
        if (!bold)
            return letters[index];
        else
            return lettersBold[index];

        return null;
    }

    public void UpdateCanvas(Texture2D letterTexture)
    {
        if (letterTexture == null || (letterTexture.name == " " && carretPositionY == canvasHeight - spacingTop))
            return;

        for (int x = 0; x < letterTexture.width; x++)
            for (int y = 0; y < letterTexture.height; y++)
                canvas.SetPixel(carretPositionX + x, carretPositionY + y, letterTexture.GetPixel(x, y));

        carretPositionY -= letterTexture.height + verticalLetterSpacing;

        if (carretPositionY < spacingBottom)
            SetCarretPosition();

        canvas.Apply();
    }

    public void SetCarretPosition()
    {
        carretPositionY = canvasHeight - spacingTop;
        if (!(carretPositionX + horizontalLineSpacing > canvasWidth - spacingRight))
            carretPositionX += horizontalLineSpacing;
    }

    public void StringToText(string text)
    {
        text += " ";
        int index = 0;

        for (int i = 0; i < text.Length - 1; i++)
        {
            if (!charToIndex.TryGetValue(Char.ToLower(text[i]), out index))
                continue;

            if (!IsLetter(index))
            {
                if (charToIndex.TryGetValue(Char.ToLower(text[i + 1]), out index))
                    if (IsLetter(index))
                    {
                        int wordLength = GetWordLength(text, i + 1);

                        if (carretPositionY - wordLength * LetterHeight < spacingBottom)
                        {
                            carretPositionY = canvasHeight - spacingTop;
                            carretPositionX += horizontalLineSpacing;
                        }

                        UpdateCanvas(GetSample(charToIndex[Char.ToLower(text[i])]));
                        continue;
                    }
                charToIndex.TryGetValue(Char.ToLower(text[i]), out index);
            }

            UpdateCanvas(GetSample(index));
        }
    }

    public int GetWordLength(string text, int index)
    {
        int length = 0;

        for (int i = index; i < text.Length; i++)
        {
            if (i + 1 == text.Length - 1)
                break;

            charToIndex.TryGetValue(Char.ToLower(text[i + 1]), out index);

            if(!IsLetter(index))
                break;

            length++;
        }

        return length;
    }

    public bool IsLetter(int charIndex)
    {
        if (charIndex == 36 || charIndex == 37 || charIndex == 38 || charIndex == 39 || charIndex == 40 || charIndex == 41 | charIndex == 42 | charIndex == 43 || charIndex == 44)
            return false;
        else
            return true;
    }

    public void GenerateLetters(Texture2D sheet, List<Texture2D> letters, bool bold)
    {
        int currentChar = 0;
        int width;
        int height;

        while (currentChar < characters.Length)
        {
            if (currentChar != 36)
            {
                width = LetterWidth;
                height = LetterHeight;
            }
            else
                {
                    width = LetterWidth;
                    height = (int)((float)LetterHeight / 2.5f);
                }

            Texture2D texure = new Texture2D(width, height, TextureFormat.RGB24, false, false);


            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Color color = sheet.GetPixel((int)(((float)x / (float)width) * 3f) + currentChar * 3, (int)(((float)y / (float)height) * 3f));

                    texure.SetPixel(x, y, backgroundColor);

                    if (x % 4 == 0 || x == width - 1 || (!bold && (x == 1 || x == 3 || x == 5 || x == 7 || x == 9 || x == 11)))
                        continue;

                    if (color.b == 1)
                        texure.SetPixel(x, y, letterColor);

                    if (color.g == 1 && y != 7 && y != 15)     
                        texure.SetPixel(x, y, letterColor);
                }

            texure.name = "" + characters[currentChar];
            texure.Apply();

            letters.Add(texure);
            currentChar++;
        }
    }

    //naufalazzmy https://discussions.unity.com/t/how-to-save-a-texture2d-into-a-png/184699/2
    private void SaveTexture(Texture2D texture)
    {
        //first Make sure you're using RGB24 as your texture format

        //then Save To Disk as PNG
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SaveImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
    }

    private Dictionary<KeyCode, int> keyToIndex = new Dictionary<KeyCode, int>()
        {
            { KeyCode.Alpha0, 0 },
            { KeyCode.Alpha1, 1 },
            { KeyCode.Alpha2, 2 },
            { KeyCode.Alpha3, 3 },
            { KeyCode.Alpha4, 4 },
            { KeyCode.Alpha5, 5 },
            { KeyCode.Alpha6, 6 },
            { KeyCode.Alpha7, 7 },
            { KeyCode.Alpha8, 8 },
            { KeyCode.Alpha9, 9 },
            { KeyCode.A, 10 },
            { KeyCode.B, 11 },
            { KeyCode.C, 12 },
            { KeyCode.D, 13 },
            { KeyCode.E, 14 },
            { KeyCode.F, 15 },
            { KeyCode.G, 16 },
            { KeyCode.H, 17 },
            { KeyCode.I, 18 },
            { KeyCode.J, 19 },
            { KeyCode.K, 20 },
            { KeyCode.L, 21 },
            { KeyCode.M, 22 },
            { KeyCode.N, 23 },
            { KeyCode.O, 24 },
            { KeyCode.P, 25 },
            { KeyCode.Q, 26 },
            { KeyCode.R, 27 },
            { KeyCode.S, 28 },
            { KeyCode.T, 29 },
            { KeyCode.U, 30 },
            { KeyCode.V, 31 },
            { KeyCode.W, 32 },
            { KeyCode.X, 33 },
            { KeyCode.Y, 34 },
            { KeyCode.Z, 35 },
            { KeyCode.Space, 36 },
            { KeyCode.Period, 37 },
            { KeyCode.Question, 38 },
            { KeyCode.Exclaim, 39 },
            { KeyCode.DoubleQuote, 40 },
            { KeyCode.Quote, 41 },
            { KeyCode.KeypadMultiply, 42 },
            { KeyCode.LeftParen, 43 },
            { KeyCode.RightParen, 44 },
        };

    private Dictionary<char, int> charToIndex = new Dictionary<char, int>()
{
    { '0', 0 }, { '1', 1 }, { '2', 2 }, { '3', 3 }, { '4', 4 },
    { '5', 5 }, { '6', 6 }, { '7', 7 }, { '8', 8 }, { '9', 9 },
    { 'a', 10 }, { 'b', 11 }, { 'c', 12 }, { 'd', 13 }, { 'e', 14 },
    { 'f', 15 }, { 'g', 16 }, { 'h', 17 }, { 'i', 18 }, { 'j', 19 },
    { 'k', 20 }, { 'l', 21 }, { 'm', 22 }, { 'n', 23 }, { 'o', 24 },
    { 'p', 25 }, { 'q', 26 }, { 'r', 27 }, { 's', 28 }, { 't', 29 },
    { 'u', 30 }, { 'v', 31 }, { 'w', 32 }, { 'x', 33 }, { 'y', 34 },
    { 'z', 35 },{ ' ', 36 },{ '.', 37 },{ '?', 38 },{ '!', 39 },{ '"', 40 },{ '\'', 41 },{ '*', 42 },{ '(' , 43 },{ ')' , 44 },
};
}


//For later...

//float[][,] canvasArray;
//float[][,] brush;

//public void DrawStroke(int charIndex, float r, float g, float b)
//{
//    int posYupper;
//    int posYlower;

//    canvasArray = new float[3][,];

//    for (int i = 0; i < 3; i++)
//        canvasArray[i] = new float[canvasWidth, canvasHeight];

//    int posX = (int)strokePoints[charIndex][0].x;

//    for (int i = (int)strokePoints[charIndex][0].y; i < (int)strokePoints[charIndex][1].y; i++)
//    {


//        for (int x = 0; x < brush.GetLength(0); x++)
//            for (int y = 0; y < brush.GetLength(1); y++)
//            {
//                canvasArray[0][x + posX, y + i] = canvasArray[0][x + posX, y + i] * (1f - brush[0][x, y]) + (brush[0][x, y] * r);
//                canvasArray[1][x + posX, y + i] = canvasArray[1][x + posX, y + i] * (1f - brush[1][x, y]) + (brush[1][x, y] * g);
//                canvasArray[2][x + posX, y + i] = canvasArray[2][x + posX, y + i] * (1f - brush[2][x, y]) + (brush[2][x, y] * b);
//            }
//    }
//}

//private List<Vector2[]> strokePoints;

//public void GenerateLetters(Texture2D sheet)
//{
//    int currentChar = 0;
//    int width;
//    int height;

//    strokePoints = new List<Vector2[]>();

//    int currentStrokeIndex = 0;

//    for (int i = 0; i < sheet.width; i += 3)
//    {
//        for (int y = 0; y < 3; y++)
//            for (int x = 0; x < 3; x++)
//            {
//                Color color = sheet.GetPixel(x + i, y);

//                if (color.b == 1 && y == 0)
//                {
//                    Vector2[] stroke = new Vector2[2];
//                    stroke[0] = new Vector2(x * 3 + 12, 0);
//                    stroke[1] = new Vector2(x * 3 + 12, 23);
//                    strokePoints.Add(stroke);
//                    x++;
//                    y = 0;
//                }
//                if (color.g == 1)
//                {
//                    Vector2[] stroke = new Vector2[2];
//                    stroke[0] = new Vector2(x * 3 + 12, y * 8);
//                    stroke[1] = new Vector2(x * 3 + 12, y * 6 + (y * 2));
//                    strokePoints.Add(stroke);
//                }
//            }
//    }

//    for (int i = 0; i < strokePoints.Count; i++)
//        for (int n = 0; n < 2; n++)
//            Debug.Log(strokePoints[i][n] + " > " + i);
//}
