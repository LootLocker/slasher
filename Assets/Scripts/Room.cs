using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int width;
    public int height;
    public int minWidth;
    public int maxWidth;
    public int minHeight;
    public int maxHeight;
    public GameObject floor;
    public GameObject wall;
    public int xOffset;

    public List<GameObject> blocks = new List<GameObject>();

    public List<GameObject> noCornerWall = new List<GameObject>();
    public List<GameObject> floors = new List<GameObject>();

    public List<GameObject> exits = new List<GameObject>();

    public Room roomPrefab;

    public GameObject exitWall;

    public void UpdateBlocksList()
    {
        List<GameObject> emptyBlocks = new List<GameObject>(blocks);
        foreach (var block in blocks)
        {
            if (block == null)
            {
                emptyBlocks.Remove(block);
            }
        }
        blocks = emptyBlocks;
    }

    public void DestroyBlockByPosition(Vector3 position)
    {
        GameObject foundBlock = null;
        foreach (var block in blocks)
        {
            if(block.transform.position == position)
            {
                foundBlock = block;
                Destroy(block);
            }
        }
        blocks.Remove(foundBlock);
    }
    public void Generate(bool startRoom = false)
    {
        width = Random.Range(minWidth, maxWidth);
        height = Random.Range(minHeight, maxHeight);
        Vector3 blockPos = Vector3.zero;
        xOffset = Random.Range(1, width-1);
        for (int x = -xOffset; x < width-xOffset; x++)
        {
            for (int y = 0; y < height; y++)
            {
                
                if(x == 0 && y == 0)
                {
                    if (startRoom == true)
                    {
                        //Walls
                        blockPos.x = x;
                        blockPos.z = y;
                        GameObject newBlock = Instantiate(wall, blockPos, transform.localRotation, transform);
                        newBlock.transform.localPosition = blockPos;
                        blocks.Add(newBlock);
                        if ((x == -xOffset && y == 0) || (x == -xOffset && y == height - 1) ||
                            (x == width - xOffset - 1 && y == 0) || (x == width - xOffset - 1 && y == height - 1))
                        {
                            // Corner, do nothing
                        }
                        else if (y != 0)
                        {
                            // Not bottom wall, since that's the rotation that we started with
                            noCornerWall.Add(newBlock);
                        }
                    }
                }
                else if (x == -xOffset || x == width - xOffset - 1 || y == 0 || y == height - 1)
                {
                    //Walls
                    blockPos.x = x;
                    blockPos.z = y;
                    GameObject newBlock = Instantiate(wall, blockPos, transform.localRotation, transform);
                    newBlock.transform.localPosition = blockPos;
                    blocks.Add(newBlock);
                    if((x == -xOffset && y == 0) || (x == -xOffset && y == height -1) || 
                        (x == width - xOffset -1 && y == 0) || (x == width - xOffset - 1 && y == height -1) )
                    {
                        // Corner, do nothing
                    }
                    else if( y != 0)
                    {
                        // Not bottom wall, since that's the rotation that we started with
                        noCornerWall.Add(newBlock);
                    }
                }
                else
                {
                    // Floors
                    blockPos.x = x;
                    blockPos.z = y;
                    GameObject newBlock = Instantiate(floor, blockPos, transform.localRotation, transform);
                    newBlock.transform.localPosition = blockPos;
                    blocks.Add(newBlock);
                    floors.Add(newBlock);
                }
                
            }
        }
        UpdateBlocksList();
        
    }

    public bool IsOverlappingWithOtherRoom(Room otherRoom)
    {
        foreach (var block in floors)
        {
            foreach (var otherBlock in otherRoom.blocks)
            {
                if(block != null && otherBlock != null && block.transform.position == otherBlock.transform.position)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void PickNewExit()
    {
        
        List<GameObject> noCornerWallNoNull = new List<GameObject>(noCornerWall);
        foreach (var block in noCornerWall)
        {
            if(block == null)
            {
                noCornerWallNoNull.Remove(block);
            }
        }
        noCornerWall = noCornerWallNoNull;
        if (noCornerWall.Count > 0)
        {
            exitWall = noCornerWall[Random.Range(0, noCornerWall.Count - 1)];
            
            // Rotate exitwall
            if (exitWall.transform.localPosition.x == -xOffset)
            {
                // Left side
                exitWall.transform.localEulerAngles = new Vector3(0, 90f, 0);
            }
            else if (exitWall.transform.localPosition.x == width - xOffset - 1)
            {
                // Right side
                exitWall.transform.localEulerAngles = new Vector3(0, -90f, 0);
            }
            if (exitWall.transform.localPosition.z == height - 1)
            {
                // Top side
                exitWall.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
            
            UpdateBlocksList();
        }
        else
        {
            exitWall = null;
        }

    }
}
