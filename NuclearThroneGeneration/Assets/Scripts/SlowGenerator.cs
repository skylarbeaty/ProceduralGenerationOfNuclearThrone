using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowGenerator : MonoBehaviour {
	//THIS SCRIPT IS FOR DEMONSTRATION PURPOSES
	//this is a slower version of the level generator
	//designed to visually show how it works.
	enum gridSpace {empty, floor, wall};
	gridSpace[,] grid;
	GameObject[,] gridObjects;
	int roomHeight, roomWidth;
	Vector2 roomSizeWorldUnits = new Vector2(35,35);
	float worldUnitsInOneGridCell = 1;
	struct walker{
		public Vector2 dir;
		public Vector2 pos;
	}
	List<walker> walkers;
	float chanceWalkerChangeDir = 0.6f, chanceWalkerSpawn = 0.05f;
	float chanceWalkerDestoy = 0.05f;
	int maxWalkers = 10;
	float percentToFill = 0.25f; 
	public GameObject wallObj, floorObj;
	float timeBetweenLoops = 0.001f, timeBetweenLoopsLong = 0.1f;
	void Start()
	{
		Setup();
		StartCoroutine(FloorLoop());
	}
	void Setup(){
		//find grid size
		roomHeight = Mathf.RoundToInt(roomSizeWorldUnits.x / worldUnitsInOneGridCell);
		roomWidth = Mathf.RoundToInt(roomSizeWorldUnits.y / worldUnitsInOneGridCell);
		//create grid
		grid = new gridSpace[roomWidth,roomHeight];
		gridObjects = new GameObject[roomWidth, roomHeight];
		//set grid's default state
		for (int x = 0; x < roomWidth-1; x++){
			for (int y = 0; y < roomHeight-1; y++){
				//make every cell "empty"
				grid[x,y] = gridSpace.empty;
			}
		}
		//set first walker
		//init list
		walkers = new List<walker>();
		//create a walker 
		walker newWalker = new walker();
		newWalker.dir = RandomDirection();
		//find center of grid
		Vector2 spawnPos = new Vector2(Mathf.RoundToInt(roomWidth/ 2.0f),
										Mathf.RoundToInt(roomHeight/ 2.0f));
		newWalker.pos = spawnPos;
		//add walker to list
		walkers.Add(newWalker);
	}
	Vector2 RandomDirection(){
		//pick random int between 0 and 3
		int choice = Mathf.FloorToInt(Random.value * 3.99f);
		//use that int to chose a direction
		switch (choice){
			case 0:
				return Vector2.down;
			case 1:
				return Vector2.left;
			case 2:
				return Vector2.up;
			default:
				return Vector2.right;
		}
	}
	int NumberOfFloors(){
		int count = 0;
		foreach (gridSpace space in grid){
			if (space == gridSpace.floor){
				count++;
			}
		}
		return count;
	}
	IEnumerator FloorLoop(){
		int iterations = 0;//loop will not run forever
		do{
			bool spawned = false;
			//create floor at position of every walker
			foreach (walker myWalker in walkers){
				if (grid[(int)myWalker.pos.x,(int)myWalker.pos.y] != gridSpace.floor){
					Spawn(myWalker.pos.x,myWalker.pos.y, floorObj);//update visuals
					spawned = true;
				}
				grid[(int)myWalker.pos.x,(int)myWalker.pos.y] = gridSpace.floor;
			}
			//chance: destroy walker
			int numberChecks = walkers.Count; //might modify count while in this loop
			for (int i = 0; i < numberChecks; i++){
				//only if its not the only one, and at a low chance
				if (Random.value < chanceWalkerDestoy && walkers.Count > 1){
					walkers.RemoveAt(i);
					break; //only destroy one per iteration
				}
			}
			//chance: walker pick new direction
			for (int i = 0; i < walkers.Count; i++){
				if (Random.value < chanceWalkerChangeDir){
					walker thisWalker = walkers[i];
					thisWalker.dir = RandomDirection();
					walkers[i] = thisWalker;
				}
			}
			//chance: spawn new walker
			numberChecks = walkers.Count; //might modify count while in this loop
			for (int i = 0; i < numberChecks; i++){
				//only if # of walkers < max, and at a low chance
				if (Random.value < chanceWalkerSpawn && walkers.Count < maxWalkers){
					//create a walker 
					walker newWalker = new walker();
					newWalker.dir = RandomDirection();
					newWalker.pos = walkers[i].pos;
					walkers.Add(newWalker);
				}
			}
			//move walkers
			for (int i = 0; i < walkers.Count; i++){
				walker thisWalker = walkers[i];
				thisWalker.pos += thisWalker.dir;
				walkers[i] = thisWalker;				
			}
			//avoid boarder of grid
			for (int i =0; i < walkers.Count; i++){
				walker thisWalker = walkers[i];
				//clamp x,y to leave a 1 space boarder: leave room for walls
				thisWalker.pos.x = Mathf.Clamp(thisWalker.pos.x, 1, roomWidth-2);
				thisWalker.pos.y = Mathf.Clamp(thisWalker.pos.y, 1, roomHeight-2);
				walkers[i] = thisWalker;
			}
			//check to exit loop
			if ((float)NumberOfFloors() / (float)grid.Length > percentToFill){
				break;
			}
			iterations++;
			if (spawned){
				yield return new WaitForSeconds(timeBetweenLoops);//make it wait
			}
		}while(iterations < 100000);
		StartCoroutine(CreateWalls());//move to next step
	}
	IEnumerator CreateWalls(){
		//loop though every grid space
		for (int x = 0; x < roomWidth-1; x++){
			for (int y = 0; y < roomHeight-1; y++){
				//if theres a floor, check the spaces around it
				if (grid[x,y] == gridSpace.floor){
					bool placed = false;
					//if any surrounding spaces are empty, place a wall
					if (grid[x,y+1] == gridSpace.empty){
						Spawn(x,y+1,wallObj);
						grid[x,y+1] = gridSpace.wall;
						placed = true;
					}
					if (grid[x,y-1] == gridSpace.empty){
						Spawn(x,y-1,wallObj);
						grid[x,y-1] = gridSpace.wall;
						placed = true;
					}
					if (grid[x+1,y] == gridSpace.empty){
						Spawn(x+1,y,wallObj);
						grid[x+1,y] = gridSpace.wall;
						placed = true;
					}
					if (grid[x-1,y] == gridSpace.empty){
						Spawn(x-1,y,wallObj);
						grid[x-1,y] = gridSpace.wall;
						placed = true;
					}
					if (placed){
						yield return new WaitForSeconds(timeBetweenLoops/2);
					}
				}
			}
		}
		StartCoroutine(RemoveSingleWalls());
	}
	IEnumerator RemoveSingleWalls(){
		//loop though every grid space
		for (int x = 0; x < roomWidth-1; x++){
			for (int y = 0; y < roomHeight-1; y++){
				//if theres a wall, check the spaces around it
				if (grid[x,y] == gridSpace.wall){
					//assume all space around wall are floors
					bool allFloors = true;
					//check each side to see if they are all floors
					for (int checkX = -1; checkX <= 1 ; checkX++){
						for (int checkY = -1; checkY <= 1; checkY++){
							if (x + checkX < 0 || x + checkX > roomWidth - 1 || 
								y + checkY < 0 || y + checkY > roomHeight - 1){
								//skip checks that are out of range
								continue;
							}
							if ((checkX != 0 && checkY != 0) || (checkX == 0 && checkY == 0)){
								//skip corners and center
								continue;
							}
							if (grid[x + checkX,y+checkY] != gridSpace.floor){
								allFloors = false;
							}
						}
					}
					if (allFloors){
						grid[x,y] = gridSpace.floor;
						Destroy(gridObjects[x,y]);
						gridObjects[x,y] = null;
						Spawn(x,y,floorObj);
						yield return new WaitForSeconds(timeBetweenLoopsLong);
					}
				}
			}
		}
	}


	void Spawn(float x, float y, GameObject toSpawn){
		//find the position to spawn
		Vector2 offset = roomSizeWorldUnits / 2.0f;
		Vector2 spawnPos = new Vector2(x,y) * worldUnitsInOneGridCell - offset;
		//spawn object
		GameObject obj = Instantiate(toSpawn, spawnPos, Quaternion.identity);
		gridObjects[(int)x,(int)y] = obj;
	}


}
