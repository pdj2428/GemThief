using UnityEngine;
using System.Collections;

public class Gem : MonoBehaviour
{
    private const int HORIZONTAL_BOOM_NUMBER = 8;
    private const int VERTICAL_BOOM_NUMBER = 9;
    private const int SQUARE_BOOM_NUMBER = 10;
    private const int RANDOM_GEM_BOOM_NUMBER = 11;

    [Header("Board Variables")]
    public int colume;
    public int row;
    public int previousColume;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private GameManager gameManager;
    private GameObject otherGem;
    private GameObject target;

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    public float swipeAngle = 0f;
    public float swipeResist = 1f;

    public SpriteRenderer spriteRenderer;
    public Sprite[] sprite;

    public int gemNumber;

    public bool isItem = false;

    public void SetGem(int gem)
    {
        if (gem > 7)
        {
            isItem = true;
        }

        spriteRenderer.sprite = sprite[gem - 1];
        
        gemNumber = gem;
    }

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    

    void Update()
    {
        FindMatches();

        if (Input.GetMouseButtonDown(0))
        {
            CastRay();

            if (target == this.gameObject)
            {
                if(gameManager.currentState == GameState.move)
                {
                    firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && target == this.gameObject)
        {
            if(gameManager.currentState == GameState.move)
            {
                finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();
            }
        }

        if (isMatched)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.2f);
        }

        targetX = colume;
        targetY = row;

        if(Mathf.Abs(targetX - transform.position.x) > 0.1f)
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.3f);
            if(gameManager.allGems[colume, row] != this.gameObject)
            {
                gameManager.allGems[colume, row] = this.gameObject;
            }
        }
        else
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            gameManager.allGems[colume, row] = this.gameObject;
        }
        if (Mathf.Abs(targetY - transform.position.y) > 0.1f)
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.3f);
            if (gameManager.allGems[colume, row] != this.gameObject)
            {
                gameManager.allGems[colume, row] = this.gameObject;
            }
        }
        else
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
            gameManager.allGems[colume, row] = this.gameObject;
        }
    }

    void CastRay()
    {
        target = null;
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f);
        if (hit.collider != null)
        { 
            target = hit.collider.gameObject;
        }
    }

    public IEnumerator CheckMoveCorutine()
    {
        yield return new WaitForSeconds(0.5f);
        if(otherGem != null)
        {
            if(isMatched == false && otherGem.GetComponent<Gem>().isMatched == false)
            {
                otherGem.GetComponent<Gem>().row = row;
                otherGem.GetComponent<Gem>().colume = colume;
                row = previousRow;
                colume = previousColume;
                yield return new WaitForSeconds(0.5f);
                gameManager.currentState = GameState.move;
            }
            else
            {
                gameManager.DestroyMatches();            
            }
            otherGem = null;

        }
       
    }

    private void CalculateAngle()
    {
        if (!gameManager.timerSet)
            return;
        if (isItem)
        {
            if(gemNumber == HORIZONTAL_BOOM_NUMBER)
            {
                for (int i = 0; i < gameManager.allGems.GetLength(1); i++)
                    gameManager.allGems[colume, i].GetComponent<Gem>().isMatched = true;
            }
            else if(gemNumber == VERTICAL_BOOM_NUMBER)
            {
                for (int i = 0; i < gameManager.allGems.GetLength(0); i++)
                    gameManager.allGems[i, row].GetComponent<Gem>().isMatched = true;                
            }           
            else if (gemNumber == SQUARE_BOOM_NUMBER)
            {
                for (int i = 0; i < gameManager.opponentAllGems.GetLength(0); i++)
                {
                    for (int j = 0; j < gameManager.opponentAllGems.GetLength(1); j++)
                    {
                        if (i >= colume - 1 && i <= colume + 1 && j >= row - 1 && j <= row + 1)
                            gameManager.allGems[i, j].GetComponent<Gem>().isMatched = true;
                    }
                }
                isMatched = true;
            }
            else if (gemNumber == RANDOM_GEM_BOOM_NUMBER)
            {
                int randomGemNumber = Random.Range(1, 8);
                for (int i = 0; i < gameManager.allGems.GetLength(0); i++)
                {
                    for (int j = 0; j < gameManager.allGems.GetLength(1); j++)
                    {
                        if( gameManager.allGems[i, j].GetComponent<Gem>().gemNumber == randomGemNumber)
                        {
                            gameManager.allGems[i, j].GetComponent<Gem>().isMatched = true;
                        }
                    }
                }
                isMatched = true;
            }
            gameManager.DestroyMatches();
            gameManager.currentState = GameState.wait;
        }
        else
        {
            if(Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
            {
                swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
                MovePieces();
                gameManager.currentState = GameState.wait;
            }
            else
            {
                gameManager.currentState = GameState.move;
            }
               
        }

    }
    
    private void MovePieces()
    {
        if (!isItem && gameManager.timerSet)
        {
            //오른쪽
            if (swipeAngle > -45 && swipeAngle <= 45 && colume < gameManager.allGems.GetLength(1) - 1)
            {
                otherGem = gameManager.allGems[colume + 1, row];
                previousColume = colume;
                previousRow = row;
                otherGem.GetComponent<Gem>().colume -= 1;
                colume += 1;
            }
            //위
            else if (swipeAngle > 45 && swipeAngle <= 135 && row < gameManager.allGems.GetLength(0) - 1)
            {
                otherGem = gameManager.allGems[colume, row + 1];
                previousColume = colume;
                previousRow = row;
                otherGem.GetComponent<Gem>().row -= 1;
                row += 1;
            }
            //왼쪽
            else if (swipeAngle > 135 || swipeAngle <= -135 && colume > 0)
            {
                otherGem = gameManager.allGems[colume - 1, row];
                previousColume = colume;
                previousRow = row;
                otherGem.GetComponent<Gem>().colume += 1;
                colume -= 1;
            }
            //아래
            else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
            {
                otherGem = gameManager.allGems[colume, row - 1];
                previousColume = colume;
                previousRow = row;
                otherGem.GetComponent<Gem>().row += 1;
                row -= 1;
            }
            StartCoroutine(CheckMoveCorutine());
        }
        
    }

    private void FindMatches()
    {
        if (gameManager.timerSet)
        {
            if (colume > 0 && colume < gameManager.allGems.GetLength(1) - 1)
            {
                GameObject leftGem1 = gameManager.allGems[colume - 1, row];
                GameObject rightGem1 = gameManager.allGems[colume + 1, row];
                if(leftGem1 != null && rightGem1 != null)
                {
                    if (leftGem1.GetComponent<Gem>().gemNumber == gemNumber && rightGem1.GetComponent<Gem>().gemNumber == gemNumber && gameObject != leftGem1 && gameObject != rightGem1)
                    {
                        leftGem1.GetComponent<Gem>().isMatched = true;
                        rightGem1.GetComponent<Gem>().isMatched = true;
                        isMatched = true;
                    }
                }
            }
            if (row > 0 && row < gameManager.allGems.GetLength(0) - 1)
            {
                GameObject upGem1 = gameManager.allGems[colume, row + 1];
                GameObject downGem1 = gameManager.allGems[colume, row - 1];
                if (upGem1 != null && downGem1 != null)
                {
                    if (upGem1.GetComponent<Gem>().gemNumber == gemNumber && downGem1.GetComponent<Gem>().gemNumber == gemNumber && gameObject != upGem1 && gameObject != downGem1)
                    {
                        upGem1.GetComponent<Gem>().isMatched = true;
                        downGem1.GetComponent<Gem>().isMatched = true;
                        isMatched = true;
                    }
                }

                
            }
        }
        
    }
}
