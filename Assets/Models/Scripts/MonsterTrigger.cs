using UnityEngine;

public class MonsterTrigger : MonoBehaviour
{
    public Transform monster, monsterAttack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, monster.position) < 0.5f)
        {
            monster.position = transform.position;

            monsterAttack.position = transform.position + transform.forward * 0.7f;
            monsterAttack.rotation = Quaternion.LookRotation(transform.position - monsterAttack.position);
        }
    }
}
