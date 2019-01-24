using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMove : TacticsMove {
    GameObject target;

    // Use this for initialization
    void Start() {
        Init();
    }

    // Update is called once per frame
    void Update() {
        Debug.DrawRay(transform.position, transform.forward);

        if (!turn) {
            return;
        }

        if (!moving) {
            FindNearestTarget();
            CalculatePath();
            FindSelectableTiles();
            actualTargetTile.target = true;
        }
        else {
            Move();
        }
    }

    void CalculatePath() {
        Tile targetTile = GetTargetTile(target);
        FindPath(targetTile);
    }//L'NPC trova dove deve andare (potremmo modificarlo in modo cheprima calcoli un Path che corrisponde ad un pattern , se il PG entra in un cono visivo invece , lo i lascia così in modo che l'NPC ci segua)

    void FindNearestTarget() {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        GameObject nearest = null;
        float distance = Mathf.Infinity;//distance = a infinito

        foreach (GameObject obj in targets) {
            float d = Vector3.Distance(transform.position, obj.transform.position);

            if (d < distance) {
                distance = d;
                nearest = obj;
            }
        }//Calcoliamo la distanza e ci riferiamo al player più vicino rispetto l'NPC 

        target = nearest;
    }
}
