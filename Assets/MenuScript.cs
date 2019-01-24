using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuScript {

    [MenuItem("Tools/Assign TileMaterial")]

    public static void AssignTileMaterial() {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        Material material = Resources.Load<Material>("Tile");


        foreach (GameObject t in tiles) {
            t.GetComponent<Renderer>().material = material;
        }
    } //Assegna il material a tutti gli elementi col tag "Tile"

    [MenuItem("Tools/Assign Tile Script")]

    public static void AssignTileScript() {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
       
        foreach (GameObject t in tiles) {
            t.AddComponent<Tile>();
        }
    } //Assegna lo script a tutti gli elementi col tag "Tile"
} //funzione che apparirà direttamente nel menù "Tools" per assegnare material e script a tutti gli elementi con lo stesso tag
