using UnityEngine;

public class Dropper : MonoBehaviour
{
    /* -------------------------------- Variables ------------------------------- */
    [Header("Dropper")]
    public GameObject dropperBody;
    public GameObject dropperLine;
    public float maxLeftX = -3f;
    public float maxRightX = 3f;
    public float moveSpeed = 15f; // Increased speed for smooth touch following
    public float fruitZ = -1f;

    [Header("Dropped Fruit")]
    public GameObject fruitsContainer;
    public GameObject dropFruit;
    public bool dropFruitCollided = true;

    [Header("Particles")]
    public ParticleSystem particleFallingFruit;

    [Header("Touch Settings")]
    public float dropDelay = 0.2f; // Small delay after dropping to prevent accidental drops
    public float minDragDuration = 0.1f; // Minimum time to hold before allowing a drop

    private GameManager gameManager;
    private float touchStartTime;
    private float lastDropTime;
    private bool isDragging = false;
    private Camera mainCamera;

    /* ------------------------------- Unity Func ------------------------------- */
    // Start is called before the first frame update
    void Start()
    {
        // Get game manager
        gameManager = FindObjectOfType<GameManager>();
        mainCamera = Camera.main;

        // Place first drop fruit (0.1s after game start, for queue to generate)
        Invoke("PlaceDropFruit", 0.1f);
        lastDropTime = -dropDelay; // Allow dropping immediately at start
    }

    // Update is called once per frame
    void Update()
    {
        // Functions that constantly run
        HandleTouchAndMouseInput();
        HideLine();

        // Check for drop fruit colliding
        if (CheckDroppedFruitCollision())
        {
            dropFruit = null; // Reset dropFruit
            PlaceDropFruit(); // Place new fruit to drop
            gameManager.UpdateNextFruitDisplay(); // Update next fruit display
            gameManager.readyToDrop = true;
        }
    }

    /* -------------------------------- Functions ------------------------------- */
    // Function that handles touch and mouse input for both movement and dropping
    void HandleTouchAndMouseInput()
    {
        // Stop taking input if game lost or paused
        if (gameManager.gameLost || PauseMenu.GAME_PAUSED) return;

        // Get dropper position
        Vector3 position = transform.position;

        // Handle touch input (mobile)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
            touchWorldPos.z = position.z; // Keep the same z position

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartTime = Time.time;
                    isDragging = true;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isDragging)
                    {
                        // Move the dropper to follow the touch position on X axis
                        position.x = Mathf.Lerp(position.x, touchWorldPos.x, moveSpeed * Time.deltaTime);
                    }
                    break;

                case TouchPhase.Ended:
                    if (isDragging)
                    {
                        // Drop the fruit when touch is released if we've been dragging long enough
                        float dragDuration = Time.time - touchStartTime;
                        if (dragDuration >= minDragDuration && gameManager.readyToDrop && Time.time > lastDropTime + dropDelay)
                        {
                            DropFruit();
                            lastDropTime = Time.time;
                        }
                        isDragging = false;
                    }
                    break;
            }
        }
        // Handle mouse input (desktop)
        else
        {
            // Move dropper with mouse position when left button is held
            if (Input.GetMouseButton(0))
            {
                if (!isDragging)
                {
                    touchStartTime = Time.time;
                    isDragging = true;
                }

                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = position.z; // Keep the same z position
                position.x = Mathf.Lerp(position.x, mouseWorldPos.x, moveSpeed * Time.deltaTime);
            }
            // Drop fruit when mouse button is released
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                float dragDuration = Time.time - touchStartTime;
                if (dragDuration >= minDragDuration && gameManager.readyToDrop && Time.time > lastDropTime + dropDelay)
                {
                    DropFruit();
                    lastDropTime = Time.time;
                }
                isDragging = false;
            }
        }

        // Clamp the position to the specified boundaries
        position.x = Mathf.Clamp(position.x, maxLeftX, maxRightX);

        // Apply the new position to the dropper object itself
        transform.position = position;
    }

    // Place the drop fruit on the dropper
    void PlaceDropFruit()
    {
        // Take the first fruit in the queue
        GameObject firstFruitInQueue = gameManager.fruitsQueue[0];

        // Instantiate the fruit as a child of dropperBody
        Vector3 spawnPosition = new Vector3(dropperBody.transform.position.x, dropperBody.transform.position.y, fruitZ);
        dropFruit = Instantiate(firstFruitInQueue, spawnPosition, Quaternion.identity);
        dropFruit.transform.SetParent(dropperBody.transform);

        // Get the Rigidbody2D component and turn off simulation
        dropFruit.GetComponent<Rigidbody2D>().simulated = false;
    }

    // Function that drops the fruit in line
    void DropFruit()
    {
        // End if fruit queue is somehow empty
        if (gameManager.fruitsQueue.Count <= 0)
        {
            Debug.LogError("Can't drop next fruit because fruit queue is empty!");
            return;
        }

        // Reset dropFruitCollided
        dropFruitCollided = false;

        // Drop fruit (Change parent and enable simulated on rb2d)
        dropFruit.transform.SetParent(fruitsContainer.transform);
        dropFruit.GetComponent<Rigidbody2D>().simulated = true;

        // Switch ready to drop to false
        gameManager.readyToDrop = false;

        // Add a new fruit to the queue and a new one
        gameManager.fruitsQueue.RemoveAt(0);
        gameManager.AddRandomFruitToQueue();
    }

    // Function that checks if the dropped fruit collided
    bool CheckDroppedFruitCollision()
    {
        if (dropFruit == null || dropFruitCollided) return false; // End if dropFruit null or already collided

        // Get fruit collider
        Collider2D fruitCollider = dropFruit.GetComponent<Collider2D>();
        if (fruitCollider == null) return false; // End if no collider found

        // Check for collisions
        Collider2D[] colliders = Physics2D.OverlapBoxAll(fruitCollider.bounds.center, fruitCollider.bounds.size, 0f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject == dropFruit) continue;

            // Check for specific tags
            if (collider.CompareTag("Floor") || collider.CompareTag("Fruit"))
            {
                // Fruit collided with floor or another fruit
                dropFruitCollided = true;
                return true;
            }
        }
        return false;
    }

    // Function that hides the dropper line if not ready to drop
    void HideLine()
    {
        if (gameManager.readyToDrop) dropperLine.GetComponent<SpriteRenderer>().enabled = true;
        else dropperLine.GetComponent<SpriteRenderer>().enabled = false;
    }
}