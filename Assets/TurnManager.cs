using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {
    static Dictionary<string, List<TacticsMove>> units = new Dictionary<string, List<TacticsMove>>(); //In caso mettessimo il famiglio conviene considerare tutto come turni da team
    static Queue<string> turnKey = new Queue<string>();//Coda per la gestione dei turni
    static Queue<TacticsMove> turnTeam = new Queue<TacticsMove>();//Coda per gestire i vari turni all'interno di un team

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (turnTeam.Count == 0) {
            InitTeamTurnQueue(); 
        }
    }

    static void InitTeamTurnQueue() {
        List<TacticsMove> teamList = units[turnKey.Peek()];//Usiamo Peek anziché dequeue per ottenere il valore della coda senza però andarla a modificare

        foreach (TacticsMove unit in teamList) {
            turnTeam.Enqueue(unit);
        }

        StartTurn();
    }

    public static void StartTurn() {
        if (turnTeam.Count > 0) {
            turnTeam.Peek().BeginTurn();
        }
    }

    public static void EndTurn() {
        TacticsMove unit = turnTeam.Dequeue(); //Finisce il turno e lo togliamo direttamente dalla coda
        unit.EndTurn();

        if (turnTeam.Count > 0) {
            StartTurn();
        }
        else {
            string team = turnKey.Dequeue();
            turnKey.Enqueue(team);
            InitTeamTurnQueue();
        }
    }

    public static void AddUnit(TacticsMove unit) {
        List<TacticsMove> list;

        if (!units.ContainsKey(unit.tag)) {
            list = new List<TacticsMove>();
            units[unit.tag] = list;

            if (!turnKey.Contains(unit.tag)) {
                turnKey.Enqueue(unit.tag);
            }
        }
        else {
            list = units[unit.tag];
        }

        list.Add(unit);
    }//In questo modo l'unità si aggiunge automaticamente alla lista di appartenenza tenendo conto dei tag
}
