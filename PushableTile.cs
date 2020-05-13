using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableTile : MonoBehaviour
{
    PlayerController player;
    Vector2 lastPos;
    Vector2 currentPos;
    Vector3 startPos;
    public Vector2 targetPos;

    Vector2 lastMoveDir;
    public float moveDelay = .5f;
    float lastMoved;

    [SerializeField]
    bool readyToMove;
    bool falling;
    bool slide;
    // Start is called before the first frame update
    void Start()
    {
        if (!player)
            player = FindObjectOfType<PlayerController>();
        currentPos = transform.position;
        targetPos = currentPos;
        startPos = currentPos;
    }

    // Update is called once per frame
    void Update()
    {
        currentPos = transform.position;
        readyToMove = (currentPos == targetPos) && Time.time >= lastMoved + moveDelay;
        if (currentPos != targetPos)
        {
            transform.position = Vector2.MoveTowards(currentPos, targetPos, 3 * Time.deltaTime);
        }
        else
        {
            lastMoved = Time.deltaTime;
        }

        if (readyToMove)
        {
            Vector2 rayOrigin = Vector2.zero;
            if (lastMoveDir.y > 0)
                rayOrigin = new Vector2(currentPos.x, GetComponent<BoxCollider2D>().bounds.max.y) + new Vector2(0, .02f);
            else if (lastMoveDir.y < 0)
                rayOrigin = new Vector2(currentPos.x, GetComponent<BoxCollider2D>().bounds.min.y) + new Vector2(0, -.02f);
            else if (lastMoveDir.x < 0)
                rayOrigin = new Vector2(GetComponent<BoxCollider2D>().bounds.min.x, currentPos.y) + new Vector2(-.02f, 0);
            else if (lastMoveDir.x > 0)
                rayOrigin = new Vector2(GetComponent<BoxCollider2D>().bounds.max.x, currentPos.y) + new Vector2(.02f, 0);



            if (falling)
                Fall();
            if (slide)
            {
                bool onSlide = Physics2D.Raycast(currentPos, lastMoveDir, .05f, player.slideLayer);
                bool objectInWay = Physics2D.Raycast(rayOrigin, lastMoveDir, .5f, player.unwalkableLayer);

                if (onSlide && !objectInWay)
                    TryPush(lastMoveDir);
                else
                    slide = false;
            }

            if (!player.canMove && !slide)
                player.canMove = true;
        }

    }

    public void TryPush(Vector2 direction)
    {
        Vector2 rayOrigin = Vector2.zero;
        if (direction.y > 0)
            rayOrigin = new Vector2(currentPos.x, GetComponent<BoxCollider2D>().bounds.max.y) + new Vector2(0, .02f);
        else if (direction.y < 0)
            rayOrigin = new Vector2(currentPos.x, GetComponent<BoxCollider2D>().bounds.min.y) + new Vector2(0, -.02f);
        else if (direction.x < 0)
            rayOrigin = new Vector2(GetComponent<BoxCollider2D>().bounds.min.x, currentPos.y) + new Vector2(-.02f, 0);
        else if (direction.x > 0)
            rayOrigin = new Vector2(GetComponent<BoxCollider2D>().bounds.max.x, currentPos.y) + new Vector2(.02f, 0);

        bool canPush = !Physics2D.Raycast(rayOrigin, direction, .5f, player.unwalkableLayer);

        if (readyToMove && canPush)
            Push(direction);
    }
    void Push(Vector2 direction)
    {
        lastPos = currentPos;
        targetPos = currentPos + direction;
        lastMoved = Time.time;
        lastMoveDir = direction;
        player.canMove = false;
    }
    void Fall()
    {
        float scale = Mathf.Lerp(transform.localScale.x, 0, 2 * Time.deltaTime);
        if (scale > .1f)
            transform.localScale = new Vector3(scale, scale, 1);
        else
            ResetTile();
    }

    void ResetTile()
    {
        transform.localScale = new Vector3(1, 1, 1);
        transform.position = startPos;
        targetPos = startPos;
        lastPos = startPos;
        falling = false;
    }
    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.gameObject.layer == LayerMask.NameToLayer("Fall"))
            falling = true;
        if (c.gameObject.layer == LayerMask.NameToLayer("Slide"))
            slide = true;
    }

    private void OnTriggerExit2D(Collider2D c)
    {
        if (c.gameObject.layer == LayerMask.NameToLayer("Fall"))
            falling = false;

    }
}
