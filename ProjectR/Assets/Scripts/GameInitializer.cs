using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        WorldManager.Update( Time.deltaTime );

		if ( Input.GetKeyDown( KeyCode.A ) == true )
		{
			Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
            Vector2Int selectedTile = WorldManager.WorldPositionToMapTilePos( mouseWorldPos );

            Pawn pawn = new Pawn(new DataContainer());

			pawn.MapPosition = new Vector2( selectedTile.x + 0.5f, selectedTile.y + 0.5f );
			WorldManager.AddRObject( pawn );
        }

        if ( Input.GetMouseButtonDown( 1 ) == true )
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
            Vector2Int selectedTile = WorldManager.WorldPositionToMapTilePos( mouseWorldPos );

            WorldManager.MoveTest( selectedTile );
        }
    }
}
