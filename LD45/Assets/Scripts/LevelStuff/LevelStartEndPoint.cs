using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStartEndPoint : MonoBehaviour
{
    public enum LevelPointType
    {
        Start,
        End
    }

    public LevelPointType pointType;

    public Vector3 direction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        switch (pointType)
        {
            case LevelPointType.Start:
                Gizmos.color = Color.green;
                break;

            case LevelPointType.End:
                Gizmos.color = Color.red;
                break;
        }

        Gizmos.DrawSphere(transform.position, 1);

        if (direction != new Vector3(0, 0, 0))
        {
            Gizmos.DrawLine(transform.position, (transform.position + direction.normalized*2));
        }
    }
}
