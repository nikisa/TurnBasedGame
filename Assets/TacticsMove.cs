
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsMove : MonoBehaviour {
    public bool turn = false;

    List<Tile> selectableTiles = new List<Tile>();
    GameObject[] tiles;

    Stack<Tile> path = new Stack<Tile>();//Conviene usare la Pila dato che il percorso lo si calcola a ritroso
    Tile currentTile;

    public bool moving = false;
    public int move = 5;
    public float jumpHeight = 2;
    public float moveSpeed = 2;
    public float jumpVelocity = 4.5f;

    Vector3 velocity = new Vector3();
    Vector3 heading = new Vector3();

    float halfHeight = 0;

    bool fallingDown = false;
    bool jumpingUp = false;
    bool movingEdge = false;
    Vector3 jumpTarget;

    public Tile actualTargetTile;

    protected void Init() {
        tiles = GameObject.FindGameObjectsWithTag("Tile");

        halfHeight = GetComponent<Collider>().bounds.extents.y;

        TurnManager.AddUnit(this);
    }//Verrà inizializzato nello start dei movimenti relativi ai vari GameObject(es: PlayerMove). Nel caso in cui vengano aggiunte o tolte caselle , dovremo inserlirlo nell'Update

    public void GetCurrentTile() {
        currentTile = GetTargetTile(gameObject);
        currentTile.current = true;
    }

    public Tile GetTargetTile(GameObject target) {
        RaycastHit hit;
        Tile tile = null;

        if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, 1)) {
            tile = hit.collider.GetComponent<Tile>();
        }

        return tile;
    }//Utilizziamo il Raycast per far capire con l'algoritmo di A* la casella in cui è presentre il player (In modo da dare un end point alla IA dell'NPC che ci sta cercando)

    public void ComputeAdjacencyLists(float jumpHeight, Tile target) {
       
        //SE DOVESSIMO FARE UNA MAPPA CON LE CASELLE DINAMICHE DOVREMO SCOMMENTARE LA RIGA SOTTO
        //tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject tile in tiles) {
            Tile t = tile.GetComponent<Tile>();
            t.FindNeighbors(jumpHeight, target);
        }
    }



    /*
     * Come funziona la ricerca in Ampiezza (BFS):
     * 
     *  funzione BFS(G,v):
     *  crea coda Q
     *  inserisci v in Q
     *  marca v
     *  while Q non è vuota:
     *     t ← Q.toglidallacoda()
     *     if t è quello che stavamo cercando:
     *         return t
     *     for all lati e in G.latiincidenti(t) do
     *         u ← G.nodiadiacenti(t,e)
     *         if u non è marcato:
     *              marca u
     *              inserisci u in Q
     *  return none
     * 
     * 
     * 
     * https://www.youtube.com/watch?v=0u78hx-66Xk
     *
     */


    public void FindSelectableTiles() { //Ricerca in ampiezza
        ComputeAdjacencyLists(jumpHeight, null);
        GetCurrentTile();
        //Sappiamo in quale casella siamo e possiamo iniziare la ricerca

        Queue<Tile> process = new Queue<Tile>();

        process.Enqueue(currentTile);
        currentTile.visited = true;
        

        while (process.Count > 0) {
            Tile t = process.Dequeue();

            selectableTiles.Add(t);
            t.selectable = true;

            if (t.distance < move) {
                foreach (Tile tile in t.adjacencyList) {
                    if (!tile.visited) {
                        tile.parent = t;
                        tile.visited = true;
                        tile.distance = 1 + t.distance;
                        process.Enqueue(tile);
                    }
                }
            }
        }
    }

    public void MoveToTile(Tile tile) {
        path.Clear();
        tile.target = true;
        moving = true;

        Tile next = tile;
        while (next != null) {//Quando next = a null significa che abbiamo raggiunto la casella di start
            path.Push(next);
            next = next.parent;//Partiamo dal target , camminiamo passando da parente a parente fino a tornare allo start
        }
    }

    public void Move() {
        if (path.Count > 0) {
            Tile t = path.Peek();
            Vector3 target = t.transform.position;

            //Calculate the unit's position on top of the target tile
            target.y += halfHeight + t.GetComponent<Collider>().bounds.extents.y;

            if (Vector3.Distance(transform.position, target) >= 0.05f) {
                bool jump = transform.position.y != target.y;

                if (jump) {
                    Jump(target);
                }
                else {
                    CalculateHeading(target);
                    SetHorizotalVelocity();
                }

                //Locomotion
                transform.forward = heading;
                transform.position += velocity * Time.deltaTime;
            }
            else {
                //Tile center reached
                transform.position = target;
                path.Pop();
            }
        }
        else {
            RemoveSelectableTiles();
            moving = false;

            TurnManager.EndTurn();
        }
    }//Permette il movimento casella per casella

    protected void RemoveSelectableTiles() {
        if (currentTile != null) {
            currentTile.current = false;
            currentTile = null;
        }

        foreach (Tile tile in selectableTiles) {
            tile.Reset();
        }

        selectableTiles.Clear();
    }

    void CalculateHeading(Vector3 target) {
        heading = target - transform.position;
        heading.Normalize(); //Così i Vector3 mantiene la stessa direzione con la lunghezza = 1.0
    }

    void SetHorizotalVelocity() {
        velocity = heading * moveSpeed;
    }//Varia la velocità di movimento tra le caselle

    void Jump(Vector3 target) {
        if (fallingDown) {
            FallDownward(target);
        }
        else if (jumpingUp) {
            JumpUpward(target);
        }
        else if (movingEdge) {
            MoveToEdge();
        }
        else {
            PrepareJump(target);
        }
    }//Creiamo tutti i casi di salti in modo da differenziare l'azione rendendo l'animazione più corretta

    void PrepareJump(Vector3 target) {
        float targetY = target.y;
        target.y = transform.position.y;

        CalculateHeading(target);

        if (transform.position.y > targetY) {
            fallingDown = false;
            jumpingUp = false;
            movingEdge = true;

            jumpTarget = transform.position + (target - transform.position) / 2.0f;
        }
        else {
            fallingDown = false;
            jumpingUp = true;
            movingEdge = false;

            velocity = heading * moveSpeed / 3.0f;

            float difference = targetY - transform.position.y;

            velocity.y = jumpVelocity * (0.5f + difference / 2.0f);
        }
    }//Tramite la transform capiamo se dovremo saltare verso l'alto o verso il basso , facendo  jumpTarget = transform.position + (target - transform.position) / 2.0f raggiungiamo il limite della casella , così durante il movimento si disattiva la possibvilià di vedere tutte le caselle disponibili(altrimenti si aggiornerebbe ad ogni casella raggiunta durante il movimento) , abbassiamo la velocità di movimento , effettuiamo il salto in modo da evitare che il player/NPC si inclini seguendo la traiettoria dell'asse

    void FallDownward(Vector3 target) {
        velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y <= target.y) {
            fallingDown = false;
            jumpingUp = false;
            movingEdge = false;

            Vector3 p = transform.position;
            p.y = target.y;
            transform.position = p;

            velocity = new Vector3();
        }//Così sappiamo se il salto è stato effettuato
    }

    void JumpUpward(Vector3 target) {
        velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y > target.y) {
            jumpingUp = false;
            fallingDown = true;
        }//Così sappiamo se il salto è stato effettuato
    }

    void MoveToEdge() {
        if (Vector3.Distance(transform.position, jumpTarget) >= 0.05f) {
            SetHorizotalVelocity();
        }
        else {
            movingEdge = false;
            fallingDown = true;

            velocity /= 5.0f;
            velocity.y = 1.5f;
        }
    }


    /*
     * FindLowestF , FindEndTile e FindPath servono pèer l'ottenimento di A*:
     * 
     * Metto il link di uno che secondo me lo spiega abbastanza bene , considera che è parecchio complesso
     * 
     * https://youtu.be/KNXfSOx4eEE
     * 
     */

    protected Tile FindLowestF(List<Tile> list) {
        Tile lowest = list[0];

        foreach (Tile t in list) {
            if (t.f < lowest.f) {
                lowest = t;
            }
        }

        list.Remove(lowest);

        return lowest;
    }

    protected Tile FindEndTile(Tile t) {
        Stack<Tile> tempPath = new Stack<Tile>();

        Tile next = t.parent;
        while (next != null) {
            tempPath.Push(next);
            next = next.parent;
        }

        if (tempPath.Count <= move) {
            return t.parent;
        }

        Tile endTile = null;
        for (int i = 0; i <= move; i++) {
            endTile = tempPath.Pop();
        }

        return endTile;
    }

    protected void FindPath(Tile target) {
        ComputeAdjacencyLists(jumpHeight, target);
        GetCurrentTile();

        List<Tile> openList = new List<Tile>();
        List<Tile> closedList = new List<Tile>();

        openList.Add(currentTile);
        //currentTile.parent = ??
        currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position);
        currentTile.f = currentTile.h;

        while (openList.Count > 0) {
            Tile t = FindLowestF(openList);

            closedList.Add(t);

            if (t == target) {
                actualTargetTile = FindEndTile(t);
                MoveToTile(actualTargetTile);
                return;
            }

            foreach (Tile tile in t.adjacencyList) {
                if (closedList.Contains(tile)) {
                    //Do nothing, already processed
                }
                else if (openList.Contains(tile)) {
                    float tempG = t.g + Vector3.Distance(tile.transform.position, t.transform.position);

                    if (tempG < tile.g) {
                        tile.parent = t;

                        tile.g = tempG;
                        tile.f = tile.g + tile.h;
                    }
                }
                else {
                    tile.parent = t;

                    tile.g = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                    tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                    tile.f = tile.g + tile.h;

                    openList.Add(tile);
                }
            }
        }

        //todo - what do you do if there is no path to the target tile?
        Debug.Log("Path not found");
    }

    public void BeginTurn() {
        turn = true;
    }

    public void EndTurn() {
        turn = false;
    }
}
