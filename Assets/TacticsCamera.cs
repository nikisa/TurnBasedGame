using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* 
Movimento e rotazione dell'oggetto TacticsCamera che ha come figlio la camera di gioco
(Aggiungere nella funzione la GetKeyDOwn di un tasto del controller oltre ai bottone presenti
ai lati dello schermo)
     
*/
public class TacticsCamera : MonoBehaviour {

	public void RotateLeft() {
        
        transform.Rotate(Vector3.up, 90, Space.Self);

    }

    public void RotateRight() {

        transform.Rotate(Vector3.up, -90, Space.Self);

    }
}
