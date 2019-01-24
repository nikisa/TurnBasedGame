using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    public bool walkable = true;
    public bool current = false;
    public bool target = false;
    public bool selectable = false;

    public List<Tile> adjacencyList = new List<Tile>();

    //BFS (breadth first search)
    public bool visited = false;
    public Tile parent = null; //Serve perché dopo aver raggiunto la casella di destinazione si crea il percorso al contrario verso il parent della casella di destinazione (proseguendo con parent del parent , poi parent del parent del parent fino ad arrivare alla casella d'inizio) in questo modo possiamo identificare quali sono walkable e come arrivarci
    public int distance = 0;//Da utilizzare per fare in modo che ogni PG/NPC possa muoversi di n caselle. n che andremo a modificare direttamente nell'editor (es NPC grosso metteremo distance bassa per farlo muovere pù lentamente)

    //Variabili per il calcolo di A*
    public float f = 0;//costo di g+h (utilizzato per trovare il miglior percorso nel minor tempo possibile)
    public float g = 0;//costo dal parent alla casella attuale
    public float h = 0;//costo dalla casella attuale a quella di destinazione

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (current) {
            GetComponent<Renderer>().material.color = Color.magenta;
        }
        else if (target) {
            GetComponent<Renderer>().material.color = Color.green;
        }
        else if (selectable) {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }//Cambiamo i colori in base al "tipo" di casella

    public void Reset() {
        adjacencyList.Clear();

        current = false;
        target = false;
        selectable = false;

        visited = false;
        parent = null;
        distance = 0;

        f = g = h = 0;
    }//Dopo la fine di ogni turno le informazione relative alle caselle devono resettarsi per poter calcolare i nuovi spostamenti che saranno utilizzabili

    public void FindNeighbors(float jumpHeight, Tile target) {
        Reset();

        CheckTile(Vector3.forward, jumpHeight, target);
        CheckTile(-Vector3.forward, jumpHeight, target);
        CheckTile(Vector3.right, jumpHeight, target);
        CheckTile(-Vector3.right, jumpHeight, target);
    }//Controlla tutte le caselle nelle posizioni adiacenti. Per float jumpHeight ci riferiamo ad una variabile che permette al PG di saltare UP o DOWN di n caselle che andremo a cambiare nella variabile di prefab

    public void CheckTile(Vector3 direction, float jumpHeight, Tile target) {
        Vector3 halfExtents = new Vector3(0.25f, (1 + jumpHeight) / 2.0f, 0.25f);//(1 + jumpHeight) / 2.0f per ootenere il range del salto 
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtents); //Physics.OverlapBox restiutuisce un array di Colliders dato che possiamo spostarci su più di un oggetto (oggetto con un Collider potenzialmente "attivabile")

        foreach (Collider item in colliders) {
            Tile tile = item.GetComponent<Tile>();
            if (tile != null && tile.walkable) { //Se tile == null vuol dire che o si va fuori dalla mappa o che in quella tile è già presente un NPC. Se non è tile e non è walkable non abbiamo bisogno di elaborarla
                RaycastHit hit;

                //il Raycast restituisce true solo se iteragisce con un collider

                if (!Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1) || (tile == target)) {//In questo modo possiamo capire se sopra la casella c'è qualcuno. Mettendo il NOT (!) all'inizio della condizione andremo ad accedere solo se sopra la casella non c'è nulla
                    adjacencyList.Add(tile);//non c'è nulla? Sì , allora la casella è disponibile e la aggiungiamo alla lista di adiacenza
                }
            }
        }
    }
}
