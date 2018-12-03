using UnityEngine;

public class Wall : MonoBehaviour {

    [SerializeField] bool playerPassed;
    [SerializeField] bool gameOverWall;
    Vector3 playerPosition;

    [SerializeField] Collider2D[] myColliders;

    GameManager gm;

    void Start ()
    {
        playerPosition = BodyManager.instance.MyPlayerSpawnPosition.position;
        myColliders = gameObject.GetComponentsInChildren<Collider2D>();
        gm = GameManager.instance;
	}
	
	void Update ()
    {
        if (!gameOverWall)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * GameManager.MyWallSpeed);

            if (transform.position.z >= playerPosition.z && !playerPassed)
            {
                playerPassed = true;
                gm.CheckWinCondition(this);

                foreach (Collider2D col in myColliders)
                {
                    col.enabled = false;
                }

                Destroy(gameObject, 5);
            }
        }

    }

}
