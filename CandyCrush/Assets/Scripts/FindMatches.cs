using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 블럭 리스트끼리 서로 이용가능하게 해줌
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board board;
    private GameManager gameManager;
    // 매칭을 저장할 리스트
    public List<GameObject> currentMatches = new List<GameObject>();
    void Start()
    {
        // 하이라키에서 Board 스크립트를 가지고 있는 게임오브젝트를 찾아 board에 할당
        board = FindObjectOfType<Board>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }
    
    // 왼쪽 / 가운데 / 오른쪽 혹은 위 / 가운데 / 아래
    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        // 가로봄을 세로매칭의 가운데에 매칭할때 발동
        if (dot1.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot1.row));
        }

        // 가로봄을 세로매칭의 위에 매칭할때 발동
        if (dot2.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot2.row));
        }

        // 가로봄을 세로매칭의 아래에 매칭할때 발동
        if (dot3.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot3.row));
        }

        return currentDots;
    }

    // 왼쪽 / 가운데 / 오른쪽 혹은 위 / 가운데 / 아래
    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        // 세로봄을 가로매칭의 왼쪽에 매칭할때 발동
        if (dot1.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot1.column));
        }

        // 세로봄을 가로매칭의 가운데에 매칭할때 발동
        if (dot2.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot2.column));
        }

        // 세로봄을 가로매칭의 오른쪽에 매칭할때 발동
        if (dot3.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot3.column));
        }

        return currentDots;
    }

    private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot1.column, dot1.row));
        }

        if (dot2.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot2.column, dot2.row));
        }

        if (dot3.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot3.column, dot3.row));
        }

        return currentDots;
    }

    // currentMatches 리스트에 블럭을 추가하고 해당 블럭을 매칭상태로 바꿔줌
    private void AddToListAndMatch(GameObject dot)
    {
        if (!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    // 이웃한 블럭끼리의 태그가 같을경우 호출되어 매칭시켜주는 함수
    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }

    // 매칭을 찾아주는 코루틴
    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(.2f);

        for(int i = 0; i < board.width; i++)
        {
            for(int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i, j];

                if(currentDot != null)
                {
                    // 2 * 2 매칭
                    Dot currentDotDot = currentDot.GetComponent<Dot>();
                    if(i < board.width - 1 && j < board.height - 1)
                    {
                        GameObject upDot = board.allDots[i, j + 1];
                        GameObject uprightDot = board.allDots[i + 1, j + 1];
                        GameObject rightDot = board.allDots[i + 1, j];
                        if(upDot != null && uprightDot != null && rightDot != null)
                        {
                            if(upDot.tag == currentDot.tag && uprightDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {
                                AddToListAndMatch(upDot);
                                AddToListAndMatch(uprightDot);
                                AddToListAndMatch(rightDot);
                                AddToListAndMatch(currentDot);
                            }
                        }
                    }

                    // 블럭이 가장 왼쪽 열이나 오른쪽 열에 있지 않은지 확인
                    if(i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];
                        
                        GameObject rightDot = board.allDots[i + 1, j];
                        

                        // 블럭의 왼쪽 블럭, 오른쪽 블럭이 모두 존재한다면 매칭 여부를 확인
                        if(leftDot != null && rightDot != null)
                        {
                            Dot rightDotDot = rightDot.GetComponent<Dot>();
                            Dot leftDotDot = leftDot.GetComponent<Dot>();

                            if(leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {
                                // 가로봄을 가로매칭의 가운데에 매칭할때 발동
                                currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot));

                                currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot));

                                currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot));

                                // 세로로 움직여 좌우로 매칭됨
                                GetNearbyPieces(leftDot, currentDot, rightDot);
                            }
                        }
                    }

                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject upDot = board.allDots[i, j + 1];
                        

                        GameObject downDot = board.allDots[i, j - 1];

                        // 블럭의 위 블럭, 아래 블럭이 모두 존재한다면 매칭 여부를 확인
                        if (upDot != null && downDot != null)
                        {
                            Dot upDotDot = upDot.GetComponent<Dot>();
                            Dot downDotDot = downDot.GetComponent<Dot>();

                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                // 세로봄을 세로매칭 발동
                                currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot));

                                // 가로봄 세로매칭 발동
                                currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot));
                                // ``

                                currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot));

                                GetNearbyPieces(upDot, currentDot, downDot);
                            }
                        }
                    }
                }
            }
        }
    }

    // 동일한 색의 블럭을 매칭시켜줌
    public void MatchPiecesOfColor(string color)
    {
        for(int i = 0; i < board.width; i++)
        {
            for(int j = 0; j < board.height; j++)
            {
                if(board.allDots[i, j] != null)
                {
                    // 블럭의 태그를 확인
                    if(board.allDots[i, j].tag == color)
                    {
                        // 색이 같은 블럭을 매칭상태로 바꿔줌
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetAdjacentPieces(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for(int i = column - 1; i <= column + 1; i++)
        {
            for(int j = row - 1; j <=row + 1; j++)
            {
                // 해당 블럭이 있는가?
                if(i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    dots.Add(board.allDots[i, j]);
                    board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                }
            }
        }
        return dots;
    }

    // 세로 한줄의 모든 블럭을 매칭으로 바꿈
    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for(int i = 0; i < board.height; i++)
        {
            if(board.allDots[column, i] != null)
            {
                dots.Add(board.allDots[column, i]);
                board.allDots[column, i].GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }

    // 먼치킨 봄을 위한 코드
    public List<GameObject> GetDirectionalPieces(int column, int row, float swipeAngle)
    {
        List<GameObject> dots = new List<GameObject>();

        // 오른쪽 스와이프 + 맨 오른쪽열의 블럭이 아닐경우
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
           for(int i = column + 1; i < board.width; i++)
           {
               dots.Add(board.allDots[i, row]);
               board.allDots[i, row].GetComponent<Dot>().isMatched = true;
           }
           return dots;
        }
        // 위로 스와이프 + 맨 윗행의 블럭이 아닐경우
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            for (int i = row + 1; i < board.height; i++)
            {
                dots.Add(board.allDots[column, i]);
                board.allDots[column, i].GetComponent<Dot>().isMatched = true;
            }
            return dots;
        }
        // 왼쪽으로 스와이프 && => ||이 되는 것에 유의! + 맨 왼쪽열의 블럭이 아닐경우
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            for (int i = 0; i < column; i++)
            {
                dots.Add(board.allDots[i, row]);
                board.allDots[i, row].GetComponent<Dot>().isMatched = true;
            }
            return dots;
        }
        // 아래쪽으로 스와이프 + 맨 아래행의 블럭이 아닐경우
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            for (int i = 0; i < row; i++)
            {
                dots.Add(board.allDots[column, i]);
                board.allDots[column, i].GetComponent<Dot>().isMatched = true;
            }
            return dots;
        }

        return dots;
    }


    // 먼치킨 봄을 터뜨렸을때 점수를 올리는 함수.
    public void AddScoreToManager()
    {
        gameManager.AddScore();
    }

    // 가로 한줄의 모든 블럭을 매칭으로 바꿈
    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[i, row] != null)
            {
                dots.Add(board.allDots[i, row]);
                board.allDots[i, row].GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }

    // 매칭을 확인후 봄을 만들어주는 함수
    public void CheckBombs()
    {
        // 플레이어가 블록을 움직였다면
        if(board.currentDot != null)
        {
            // "사용자가 움직인 블럭"으로 봄을 만들 수 있는 매칭이 성립된다면
            if(board.currentDot.isMatched)
            {
                // 그 위치에 봄을 만들것이므로 매칭되지 않은 상태로 바꿔주고
                board.currentDot.isMatched = false;

                // 생성할 봄을 정함
                // 오른쪽, 왼쪽으로 스와이프 했을 경우 
                if((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                {
                    board.currentDot.MakeRowBomb();
                    board.currentDot.ChangeColorToOriginalColor();
                }
                // 오른쪽, 왼쪽 스와이프가 아닌경우(세로)
                else
                {
                    board.currentDot.MakeColumnBomb();
                    board.currentDot.ChangeColorToOriginalColor();
                }
                // 생성할 봄을 정함(반반 확률 랜덤생성)
                /* int typeOfBomb = Random.Range(0, 100);
                if(typeOfBomb < 50)
                {
                    // 가로봄 생성
                    board.currentDot.MakeRowBomb();
                    board.currentDot.ChangeColorToOriginalColor();
                }
                else if(typeOfBomb >= 50)
                {
                    // 세로봄 생성
                    board.currentDot.MakeColumnBomb();
                    board.currentDot.ChangeColorToOriginalColor();

                }*/
            }
            // 사용자가 움직이지 않은 블록으로부터 봄을 만들 수 있는 매칭이 성립될경우
            else if(board.currentDot.otherDot != null)
            {
                Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
                // 매칭이 성립되면
                if(otherDot.isMatched)
                {
                    // 아래의 원래는 위와 비슷
                    otherDot.isMatched = false;
                    // 오른쪽, 왼쪽으로 스와이프 했을 경우 
                    if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                    {
                        otherDot.MakeRowBomb();
                        otherDot.ChangeColorToOriginalColor();
                    }
                    // 오른쪽, 왼쪽 스와이프가 아닌경우(세로)
                    else
                    {
                        otherDot.MakeColumnBomb();
                        otherDot.ChangeColorToOriginalColor();
                    }
                    // 생성할 봄을 정함
                    /* int typeOfBomb = Random.Range(0, 100);
                    if (typeOfBomb < 50)
                    {
                        // 가로봄 생성
                        otherDot.MakeRowBomb();
                        otherDot.ChangeColorToOriginalColor();
                    }
                    else if (typeOfBomb >= 50)
                    {
                        // 세로봄 생성
                        otherDot.MakeColumnBomb();
                        otherDot.ChangeColorToOriginalColor();

                    } */
                }
            }
        }
    }
}


// 2 * 2의 매칭을 지원하기 위한 코드 시작
// (처음 생각한 구조) 좋지 않다고 판단하여 주석처리함
/* 
// 위, 아래로 스와이프 하는 경우
if(i > 0 && i < board.width - 1 && j > 1 && j < board.height - 2)
{
    GameObject upDot = board.allDots[i, j + 1];
    GameObject downDot = board.allDots[i, j - 1];
    GameObject leftDot = board.allDots[i - 1, j];
    GameObject rightDot = board.allDots[i + 1, j];

    // 위로 스와이프
    GameObject upupDot = board.allDots[i, j + 2];
    GameObject upleftDot = board.allDots[i - 1, j + 1];
    GameObject upupleftDot = board.allDots[i - 1, j + 2];

    GameObject uprightDot = board.allDots[i + 1, j + 1];
    GameObject upuprightDot = board.allDots[i + 1, j + 2];

    // 아래로 스와이프
    GameObject downdownDot = board.allDots[i, j -2];
    GameObject downleftDot = board.allDots[i - 1, j - 1];
    GameObject downdownleftDot = board.allDots[i - 1, j - 2];

    GameObject downrightDot = board.allDots[i + 1, j - 1];
    GameObject downdownrightDot = board.allDots[i + 1, j - 2];

    if ((upupleftDot != null && upupDot != null && upleftDot != null) && (upupleftDot.tag == currentDot.tag && upupDot.tag == currentDot.tag && upleftDot.tag == currentDot.tag))
    {
        if (!currentMatches.Contains(upupleftDot))
        {
            currentMatches.Add(upupleftDot);
        }
        if (!currentMatches.Contains(upupDot))
        {
            currentMatches.Add(upupDot);
        }
        if (!currentMatches.Contains(upleftDot))
        {
            currentMatches.Add(upleftDot);
        }
        if (!currentMatches.Contains(currentDot))
        {
            currentMatches.Add(currentDot);
        }
        upupleftDot.GetComponent<Dot>().isMatched = true;
        upupDot.GetComponent<Dot>().isMatched = true;
        upleftDot.GetComponent<Dot>().isMatched = true;
        currentDot.GetComponent<Dot>().isMatched = true;
    }

    if ((upupDot != null && upuprightDot != null && uprightDot != null) && upupDot.tag == currentDot.tag && upuprightDot.tag == currentDot.tag && uprightDot.tag == currentDot.tag)
    {
        if (!currentMatches.Contains(upupDot))
        {
            currentMatches.Add(upupDot);
        }
        if (!currentMatches.Contains(upuprightDot))
        {
            currentMatches.Add(upuprightDot);
        }
        if (!currentMatches.Contains(uprightDot))
        {
            currentMatches.Add(uprightDot);
        }
        if (!currentMatches.Contains(currentDot))
        {
            currentMatches.Add(currentDot);
        }
        upupDot.GetComponent<Dot>().isMatched = true;
        upuprightDot.GetComponent<Dot>().isMatched = true;
        uprightDot.GetComponent<Dot>().isMatched = true;
        currentDot.GetComponent<Dot>().isMatched = true;
    }

    if ((downdownleftDot != null && downdownDot != null && downleftDot != null) && downdownleftDot.tag == currentDot.tag && downdownDot.tag == currentDot.tag && downleftDot.tag == currentDot.tag)
    {
        if (!currentMatches.Contains(downdownleftDot))
        {
            currentMatches.Add(downdownleftDot);
        }
        if (!currentMatches.Contains(downdownDot))
        {
            currentMatches.Add(downdownDot);
        }
        if (!currentMatches.Contains(downleftDot))
        {
            currentMatches.Add(downleftDot);
        }
        if (!currentMatches.Contains(currentDot))
        {
            currentMatches.Add(currentDot);
        }
        downdownleftDot.GetComponent<Dot>().isMatched = true;
        downdownDot.GetComponent<Dot>().isMatched = true;
        downleftDot.GetComponent<Dot>().isMatched = true;
        currentDot.GetComponent<Dot>().isMatched = true;
    }

    if ((downdownDot != null && downdownrightDot != null && downrightDot != null) && downdownDot.tag == currentDot.tag && downdownrightDot.tag == currentDot.tag && downrightDot.tag == currentDot.tag)
    {
        if (!currentMatches.Contains(downdownDot))
        {
            currentMatches.Add(downdownDot);
        }
        if (!currentMatches.Contains(downdownrightDot))
        {
            currentMatches.Add(downdownrightDot);
        }
        if (!currentMatches.Contains(downrightDot))
        {
            currentMatches.Add(downrightDot);
        }
        if (!currentMatches.Contains(currentDot))
        {
            currentMatches.Add(currentDot);
        }
        downdownDot.GetComponent<Dot>().isMatched = true;
        downdownrightDot.GetComponent<Dot>().isMatched = true;
        downrightDot.GetComponent<Dot>().isMatched = true;
        currentDot.GetComponent<Dot>().isMatched = true;
    }
}
*/
// 2 * 2의 매칭을 지원하기 위한 코드 끝