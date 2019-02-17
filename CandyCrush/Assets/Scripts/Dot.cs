using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    // column = 현재 블럭의 x좌표
    public int column;
    // row = 현재 블럭의 y좌표
    public int row;
    // 이동시 매칭이 성립되지 않았을 경우 되돌아오기 위해 사용되는 블럭의 원래 x좌표
    public int previousColumn;
    // 이동시 매칭이 성립되지 않았을 경우 되돌아오기 위해 사용되는 블럭의 원래 y좌표
    public int previousRow;
    // 이동을 원하는 지점의 X좌표
    public int targetX;
    // 이동을 원하는 지점의 Y좌표
    public int targetY;
    // 블럭들이 3개 이상 매칭된 상태인가?
    public bool isMatched = false;

    // 현재 선택한 블럭에 의해 스와이프 당하는 블럭
    public GameObject otherDot;

    // FindMatches 스크립트의 레퍼런스
    private FindMatches findMatches;
    private Board board;
    
    // 블럭사이의 스와이프를 위해 알아야 하는 터치가 시작되는 지점
    private Vector2 firstTouchPosition;
    // 블럭사이의 스와이프를 위해 알아야하는 터치가 끝나는 지점
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;
    // 어느 방향으로 스와이프 했는가?

    [Header ("Swipe Variables")]
    public float swipeAngle = 0;
    // 터치만 하고 스와이프 하지 않았을 경우, 블록의 움직임을 막기 위한 변수 (최소 .5만큼의 움직임이 있어야함)
    public float swipeResist = .5f;

    [Header ("Powerup Stuff")]
    // 이 블럭이 세로봄인가?
    public bool isColumnBomb;
    // 이 블럭이 가로봄인가?
    public bool isRowBomb;
    // 이 블럭이 한 색을 다 날리는 컬러봄인가?
    public bool isColorBomb;
    public bool isAdjacentBomb;

    // 이 블럭이 먼치킨봄인가?
    public bool isMunchkinBomb;

    // 세로봄의 레퍼런스
    public GameObject columnArrow;
    // 가로봄의 레퍼런스
    public GameObject rowArrow;
    public GameObject colorBomb;
    public GameObject adjacentMarker;
    // 먼치킨봄의 레퍼런스
    public GameObject munchkinArrow;

    // 원래의 색깔
    public Color originalColor;
    

    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;
        isMunchkinBomb = false;
        SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
        originalColor = mySprite.color;

        // 하이라키에서 Board를 가지고 있는 게임 오브젝트를 찾아 할당
        board = FindObjectOfType<Board>();

        findMatches = FindObjectOfType<FindMatches>();
        // 블럭의 초기 좌표 설정
        // targetX = (int)transform.position.x;
        // targetY = (int)transform.position.y;
        // column = targetX;
        // row = targetY;
        // previousRow = row;
        // previousColumn = column;
    }

    // 봄을 테스트하는데 쓰이는 코드
    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(1))
        {
            isMunchkinBomb = true;
            GameObject munchkin = Instantiate(munchkinArrow, transform.position, Quaternion.identity);
            munchkin.transform.parent = this.transform;
        }
    }

    void Update()
    {
        // FindMatches 스크립트에서 제어함
        //FindMatches();

        
        // 매칭된 상태라면 색을 바꿈
        if(isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1f, 1f, 1f, .2f);

            //mySprite.color = originalColor;
        }
        

        targetX = column;
        targetY = row;
        // 블럭의 원하는 위치가 실제 위치와 차이가 있다면(움직여야 한다면)
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            // Lerp를 이용하여 현재위치와 원하는 위치 사이를 쪼개어 부드럽게 이동가능하게 함
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);

            // 정위치가 아닐때 정위치로 맞춰줌
            if(board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            // 블럭이 움직일때 매칭을 찾음
            findMatches.FindAllMatches();
        }
        // 움직일 필요가 없을때 정위치로 맞춰줌
        else
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
        }

        // 블럭의 원하는 위치가 실제 위치와 차이가 있다면(움직여야 한다면)
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            // Lerp를 이용하여 현재위치와 원하는 위치 사이를 쪼개어 부드럽게 이동가능하게 함
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            // 정위치가 아닐때 정위치로 맞춰줌
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();

        }
        // 움직일 필요가 없을때 정위치로 맞춰줌
        else
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
        }
    }

    // 매칭성립이되지않았을때 제자리로 돌려놓고 매칭이 되었을때 블럭을 파괴하기 위한 코루틴
    public IEnumerator CheckMoveCo()
    {
        // 컬러봄을 움직여 * 다른 색 블록과 매칭
        if(isColorBomb)
        {
            findMatches.MatchPiecesOfColor(otherDot.tag);
           isMatched = true;
        }
        // 다른 색 블록을 움직여 컬러봄과 매칭
        else if(otherDot.GetComponent<Dot>().isColorBomb)
        {
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            otherDot.GetComponent<Dot>().isMatched = true;
        }

        if(isMunchkinBomb)
        {
            findMatches.GetDirectionalPieces(column, row, swipeAngle);
            isMatched = true;
            findMatches.AddScoreToManager();
        }

        yield return new WaitForSeconds(.5f);
        // 스와이프 당하는 블럭이 존재한다면
        if(otherDot != null)
        {
            // 현재 블럭도, 스와이프 당하는 블럭도 매치가 성립되지 않는다면
            if(!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                // 다시 제자리로 돌려놓음
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column = column;

                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;
            }
            else
            {
                board.DestroyMatches();
            }
            //otherDot = null;
        }
    }

    // 블럭의 터치가 시작되면 아래 연산을 수행
    private void OnMouseDown()
    {
        // 블럭들을 움직일수 있는 상태라면
        if(board.currentState == GameState.move)
        {
            // 입력을 받고 월드 좌표계로 바꿔줌.
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log(firstTouchPosition);
        }
    }

    // 블럭을 스와이프하여 터치가 끝나면 아래 연산을 수행
    private void OnMouseUp()
    {
        if(board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    private void CalculateAngle()
    {
        // 블럭을 터치하고 최소한의 움직임이 있었을 경우,
        if(Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            // 내장된 연산 함수를 이용하여 스와이프 각도를 구해줌
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            Debug.Log(swipeAngle);
            // 계산이 수행되고 블럭들이 이동되는 동안 사용자의 입력을 받지 못하도록 상태를 바꿔줌
            board.currentState = GameState.wait;
            board.currentDot = this;
        }
        else // swipeResist 미만의 스와이프였을 경우 다시 움직일수 있도록 함
        {
            board.currentState = GameState.move;
        }
    }

    private void MovePieces()
    {
        // 오른쪽으로 스와이프 + 맨 오른쪽열의 블럭이 아닐 경우
        if(swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            // 선택한 블럭의 바로 오른쪽에 있는 블럭을 찾아옴
            otherDot = board.allDots[column + 1, row];
            // 원래 좌표를 저장
            previousRow = row;
            previousColumn = column;
            // 스와이프 당하는 블럭을 왼쪽으로 1만큼
            otherDot.GetComponent<Dot>().column -= 1;
            
            // 선택한 블럭을 오른쪽으로 1만큼 이동
            column += 1;
        }
        // 위로 스와이프 + 맨 윗행의 블럭이 아닐경우
        else if(swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            // 선택한 블럭의 바로 위에 있는 블럭을 찾아옴
            otherDot = board.allDots[column, row + 1];
            // 원래 좌표를 저장
            previousRow = row;
            previousColumn = column;
            // 스와이프 당하는 블럭을 아래로 1만큼
            otherDot.GetComponent<Dot>().row -= 1;

            // 선택한 블럭을 위로 1만큼 이동
            row += 1;
        }
        // 왼쪽으로 스와이프 && => ||이 되는 것에 유의! + 맨 왼쪽열의 블럭이 아닐경우
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            // 선택한 블럭의 바로 왼쪽에 있는 블럭을 찾아옴
            otherDot = board.allDots[column - 1, row];
            // 원래 좌표를 저장
            previousRow = row;
            previousColumn = column;
            // 스와이프 당하는 블럭을 오른쪽으로 1만큼
            otherDot.GetComponent<Dot>().column += 1;

            // 선택한 블럭을 왼쪽으로 1만큼 이동
            column -= 1;
        }
        // 아래쪽으로 스와이프 + 맨 아래행의 블럭이 아닐경우
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            // 선택한 블럭의 바로 아래에 있는 블럭을 찾아옴
            otherDot = board.allDots[column, row - 1];
            // 원래 좌표를 저장
            previousRow = row;
            previousColumn = column;
            // 스와이프 당하는 블럭을 위로 1만큼
            otherDot.GetComponent<Dot>().row += 1;

            // 선택한 블럭을 아래쪽으로 1만큼 이동
            row -= 1;
        }
        StartCoroutine(CheckMoveCo());
    }

    // 상하좌우로 연속된 블럭의 매치가 있는지 찾아줌
    private void FindMatches()
    {
        // 블럭이 좌우 양 끝의 블럭이 아닌 경우 좌우로 매칭
        if(column > 0 && column < board.width -1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1 , row];
            // 좌 우 블럭이 할당되어져 있을 경우에
            if(leftDot1 != null && rightDot1 != null)
            {
                // 좌우로 태그가 같다면 매칭
                if(leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        // 블럭이 위아래 끝 블럭이 아닌 경우 위아래로 매칭
        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            // 위 아래 블럭이 할당되어져 있을 경우에
            if(upDot1 != null && downDot1 != null)
            {
                // 위아래로 태그가 같다면 매칭
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }

    // 원래 색으로 되돌려놓는 함수
    public void ChangeColorToOriginalColor()
    {
        SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
        mySprite.color = originalColor;
    }

    // 가로봄 생성
    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    // 세로봄 생성
    public void MakeColumnBomb()
    {
        isColumnBomb = true;
        GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
        color.transform.parent = this.transform;
    }

    public void MakeAdjacentBomb()
    {
        isAdjacentBomb = true;
        GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
        marker.transform.parent = this.transform;
    }

    public void MakeMunchkinBomb()
    {
        isMunchkinBomb = true;
        GameObject munchkin = Instantiate(munchkinArrow, transform.position, Quaternion.identity);
        munchkin.transform.parent = this.transform;
    }
}
