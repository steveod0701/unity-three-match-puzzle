using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 상태 머신을 위한 열거형
public enum GameState
{
    wait,
    move
}

// 백그라운드 타일들을 관리해주는 스크립트
public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public int width;
    public int height;
    // 미리 얼마만큼의 블럭을 준비해놓을 것인지(블럭이 매칭되어 파괴된 후 위에서 내려오는 효과를 위해)
    public int offSet;

    // 블럭들을 담기 위한 배열
    public GameObject[] dots;
    // 생성된 블럭들을 관리하기 위한 이차원 배열
    public GameObject[,] allDots;

    public GameObject tilePrefab;

    // 현재 움직이고 있는 블럭의 레퍼런스
    public Dot currentDot;

    // 생성된 백그라운드 타일을 관리하기 위한 이차원 배열
    private BackgroundTile[,] allTiles;

    // FindMatches 스크립트의 레퍼런스
    private FindMatches findMatches;

    void Start()
    {
        findMatches = FindObjectOfType<FindMatches>();
        allTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        SetUp();
    }
    private void SetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j + offSet);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                // 하이라키가 지저분해지지 않도록 생성된 백그라운드 타일을 공통된 부모인 Board로 묶어줌
                backgroundTile.transform.parent = this.transform;
                // 이름 설정
                backgroundTile.name = "( " + i + ", " + j + " )";

                // 어떤 블럭이 생성될지 랜덤하게 고름
                int dotToUse = Random.Range(0, dots.Length);

                // 아래 while문의 무한루프 방지를 위한 안전장치
                int maxIterations = 0;
                // Instantiate로 보드에 생성하기 전에 매칭되는 블럭이 있나 먼저 찾고, 매칭되는 블럭이 있다면 다른 색의 블럭으로 교체해서 초기 보드에 매칭이 없도록 조절합니다.
                while(MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                }
                maxIterations = 0;

                // 블럭 생성
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);

                //블럭의 초기 좌표 설정
                dot.GetComponent<Dot>().row = j;
                dot.GetComponent<Dot>().column = i;

                // 하이라키가 지저분해지지 않도록 생성된 블럭을 백그라운드 타일에 묶어줌
                dot.transform.parent = this.transform;

                // 이름 설정
                dot.name = "( " + i + ", " + j + " )";
                allDots[i, j] = dot;
            }
        }
    }

    // 어느 지점에서 매치가 일어났는지 확인하고 true, false를 리턴해주는 함수. 게임 초기 보드에 블럭 생성시 매칭되는 블럭을 없애기 위해 사용됨
    private bool MatchesAt(int column, int row, GameObject piece)
    {
        // 2 * 2 매칭
        if (column < width - 1 && row < height - 1)
        {
            if (allDots[column, row + 1] != null && allDots[column + 1, row + 1] != null && allDots[column, row + 1] != null)
            {
                if (allDots[column, row + 1].tag == allDots[column, row].tag && allDots[column + 1, row + 1].tag == allDots[column, row].tag && allDots[column + 1, row].tag == allDots[column, row].tag)
                {
                    return true;
                }
            }
        }

        if(column > 1 && row > 1)
        {
            // 가로의 매칭을 찾아줌
            if(allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
            {
                return true;
            }
            // 세로의 매칭을 찾아줌
            if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        // 위의 if문에서는 보드의 테두리에 걸쳐지는 블럭들의 매칭을 찾아낼 수 없으므로, 
        else if(column <= 1 || row <= 1)
        {
            if(row > 1)
            {
                if(allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
        }
        // 매칭이 성립되지 않았다면 false를 리턴
        return false;
    }

    private bool ColumnOrRow()
    {
        // 가로로 5개 연속의 매칭인지 아닌지 확인
        int numberHorizontal = 0;
        // 세로로 5개 연속의 매칭인지 아닌지 확인
        int numberVertical = 0;

        Dot firstPiece = findMatches.currentMatches[0].GetComponent<Dot>();
        
        if(firstPiece != null)
        {
            foreach (GameObject currentPiece in findMatches.currentMatches)
            {
                Dot dot = currentPiece.GetComponent<Dot>();
                // 처음 블럭 기준으로 y좌표가 같다면
                if(dot.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                // 처음블럭 기준으로 x좌표가 같다면
                if (dot.column == firstPiece.column)
                {
                    numberVertical++;
                }
            }
        }
        
        return (numberVertical == 5 || numberHorizontal == 5);
    }


    private void CheckToMakeBombs()
    {
        if(findMatches.currentMatches.Count >= 4 || findMatches.currentMatches.Count >= 7 || findMatches.currentMatches.Count >= 5)
        {
            // 가로의 매칭 확인
            int numberHorizontal = 0;
            // 세로의 매칭 확인
            int numberVertical = 0;

            Dot firstPiece = findMatches.currentMatches[0].GetComponent<Dot>();

            if (firstPiece != null)
            {
                foreach (GameObject currentPiece in findMatches.currentMatches)
                {
                    Dot dot = currentPiece.GetComponent<Dot>();
                    // 처음 블럭 기준으로 y좌표가 같다면
                    if (dot.row == firstPiece.row)
                    {
                        numberHorizontal++;
                    }
                    // 처음블럭 기준으로 x좌표가 같다면
                    if (dot.column == firstPiece.column)
                    {
                        numberVertical++;
                    }
                }
            }
            // 2 * 2 의 매칭을 확인
            if(numberHorizontal >= 2 && numberVertical >= 2)
            {
                if (currentDot != null)
                {
                    // 사용자가 직접 움직인 블럭에의한 매칭인가?
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isMunchkinBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeMunchkinBomb();
                        }
                    }
                    // 직접 움직이지 않은 블럭이 스와이프되어 매칭이 성립되는가?
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isMunchkinBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeMunchkinBomb();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // 일렬로 4개의 매칭을 확인
                findMatches.CheckBombs();
            }
        }

        // 여러 특수 블럭이 있어서 2*2 매칭의 확인이 모호해질때가 있습니다. 주석을 해제하시면 나란한 5개의 매칭과 ㄱ, ㄴ T 형태의 매칭을 확인할 수 있습니다.
        /* else if(findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8)
        {
            // 나란한 5개의 매칭
            if(ColumnOrRow())
            {
                
                if(currentDot != null)
                {
                    // 사용자가 직접 움직인 블럭에의한 매칭인가?
                    if(currentDot.isMatched)
                    {
                        if(!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    }
                    // 직접 움직이지 않은 블럭이 스와이프되어 매칭이 성립되는가?
                    else
                    {
                        if(currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if(otherDot.isMatched)
                            {
                                if(!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
            }
            //  T자 혹은 ㄱ자 가 되는 5개의 매칭
            else
            {
                if (currentDot != null)
                {
                    // 사용자가 직접 움직인 블럭에의한 매칭인가?
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                            currentDot.ChangeColorToOriginalColor();
                        }
                    }
                    // 직접 움직이지 않은 블럭이 스와이프되어 매칭이 성립되는가?
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isAdjacentBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeAdjacentBomb();
                                    currentDot.ChangeColorToOriginalColor();
                                }
                            }
                        }
                    }
                }
            }
        }*/
    }

    // 특정 위치에 있는 매칭이 된 블럭을 파괴
    private void DestroyMatchesAt(int column, int row)
    {
        if(allDots[column, row].GetComponent<Dot>().isMatched)
        {
            // 봄을 만들수 있나 확인
            if(findMatches.currentMatches.Count >= 4)
            {
                CheckToMakeBombs();
            }
            
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        // FindMatches의 매칭 리스트에서 삭제
        findMatches.currentMatches.Clear();
        StartCoroutine(DecreaseRowCo());
    }

    // 블럭이 매칭되어 빈공간이 생겼을 때 아래로 내려오며 빈 공간을 메워주기 위한 코루틴
    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                // 빈 공간이 생겼다면
                if(allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if(nullCount > 0)
                {
                    // 위의 블럭들을 내려주고
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    // 그 위의 블럭들이 내려온 공간을 빈 공간으로 설정해줌
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    // 코루틴으로 매칭 확인 -> 빈자리 채우기 (반복)
    
    private void RefillBoard()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                // 보드 위의 한 블럭의 공간이 비어있다면
                if(allDots[i, j] == null)
                {
                    // 그 위치를 받아오고
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    // 랜덤한 블럭을 생성해준 후 그 위치의 블럭으로 할당시킴
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                }
            }
        }
    }

    // 보드에 플레이어가 따로 움직이지 않아도 매칭되는 블럭들이 있는지 확인
    private bool MatchesOnBoard()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allDots[i, j] != null)
                {
                    if(allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while(MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        // 블럭을 부순 뒤에 현재 매칭 리스트를 비워주고
        findMatches.currentMatches.Clear();
        currentDot = null;
        // 움직임이 있었을때 Dot 스크립트에서에서 상태를 wait로 바꾸고, 0.5초 후에 다시 블럭들을 이동할수 있게함
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }
}