using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class PlayerController : MonoBehaviour
{
    public LayerMask unwalkableLayer;
    public LayerMask slideLayer;

    public float moveSpeed = 4;

    Vector2 currentPos;
    Vector2 targetPos;
    Vector2 lastPos;
    public Vector2 lastMoveDir;

    bool falling;
    bool sliding;
    bool recovered;
    public bool canMove;
    // Start is called before the first frame update
    void Start()
    {
        currentPos = transform.position;
        targetPos = currentPos;
        lastPos = currentPos;
    }

    // Update is called once per frame
    void Update()
    {
        currentPos = transform.position;
        Vector2 playerInput = Vector2.zero;
        if (currentPos == targetPos)
        {
            if (!falling)
            {
                if (!sliding)
                {
                    if (canMove)
                    {
                        playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                        Vector2 nextMoveDirection = Vector2.zero;

                        if (playerInput.x != 0)
                            nextMoveDirection = new Vector2(playerInput.x, 0);

                        else if (playerInput.y != 0)
                            nextMoveDirection = new Vector2(0, playerInput.y);


                        if (PlayerCanMoveTo(nextMoveDirection))
                            MovePlayer(nextMoveDirection);

                        else if (Physics2D.Raycast(currentPos, nextMoveDirection, 1, unwalkableLayer).collider.GetComponent<PushableTile>())
                            Physics2D.Raycast(currentPos, nextMoveDirection, 1, unwalkableLayer).collider.GetComponent<PushableTile>().TryPush(nextMoveDirection);
                        
                    }
                }
                else
                {
                    if (PlayerCanMoveTo(lastMoveDir) && onSlidingTile())
                        MovePlayer(lastMoveDir);
                    else
                        sliding = false;
                }
            }
            else
            {
                Fall();
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(currentPos, targetPos, moveSpeed*Time.deltaTime);
        }
    }

    void Fall()
    {
        float scale = Mathf.Lerp(transform.localScale.x, 0, 3 * Time.deltaTime);
        if (scale > .1f)
            transform.localScale = new Vector3(scale, scale, 1);
        else
            RecoverPlayer();
    }

    void RecoverPlayer()
    {
        targetPos = lastPos;
        transform.position = lastPos;
        transform.localScale = new Vector3(1, 1, 1);
        falling = false;
        sliding = false;
        recovered = true;
    }    

    void MovePlayer(Vector2 directionToMove)
    {
        recovered = false;
        lastPos = currentPos;
        targetPos = currentPos + directionToMove;
        lastMoveDir = directionToMove;
    }

    bool PlayerCanMoveTo(Vector2 direction){
        return !Physics2D.Raycast(currentPos, direction, 1, unwalkableLayer);}

    bool onSlidingTile(){
        return Physics2D.Raycast(currentPos, lastMoveDir, .05f, slideLayer);}


    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.gameObject.layer == LayerMask.NameToLayer("Fall"))
            falling = true;
        if (c.gameObject.layer == LayerMask.NameToLayer("Slide") && !recovered)
            sliding = true;
    }

    private void OnTriggerExit2D(Collider2D c)
    {
        if (c.gameObject.layer == LayerMask.NameToLayer("Fall"))
            falling = false;

    }
}
