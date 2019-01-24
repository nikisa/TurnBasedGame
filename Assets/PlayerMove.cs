using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove {

	// Use this for initialization
	void Start () {
        Init();
	}

    // Update is called once per frame
    void Update() {

        Debug.DrawRay(transform.position, transform.forward);

        if (!turn) {
            return;
        }


        if (!moving) {
            FindSelectableTiles();
            CheckMouse();
        }
        else {
            Move();
        }
    }
        

        void CheckMouse() {
            if (Input.GetMouseButton(0)) { //GetMouseButton(0) corrisponde al tasto sinistro , se metti 1 corrisponde a quello destro
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if(Physics.Raycast(ray , out hit)) {
                    if(hit.collider.tag == "Tile") {
                        Tile t = hit.collider.GetComponent<Tile>(); //In questo modo si abilita solo se è una casella raggiungibile

                        if (t.selectable) {
                            MoveToTile(t);
                        }
                    }
                }
            }
        }
		
	}
