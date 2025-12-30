using UnityEngine;

/// <summary>
/// 화면 경계 벽 생성
/// </summary>
public class ScreenBoundary : MonoBehaviour
{
    [SerializeField] private float wallThickness = 0.5f;
    [SerializeField] private float padding = 0.5f;
    
    private void Start()
    {
        CreateBoundaryWalls();
    }
    
    private void CreateBoundaryWalls()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        float camHeight = cam.orthographicSize * 2;
        float camWidth = camHeight * cam.aspect;
        
        // 상단
        CreateWall("TopWall", 
            new Vector2(0, cam.orthographicSize + wallThickness/2 - padding),
            new Vector2(camWidth + wallThickness * 2, wallThickness));
        
        // 하단 (바구니 위)
        CreateWall("BottomWall", 
            new Vector2(0, -cam.orthographicSize - wallThickness/2 + padding + 2f),
            new Vector2(camWidth + wallThickness * 2, wallThickness));
        
        // 왼쪽
        CreateWall("LeftWall", 
            new Vector2(-camWidth/2 - wallThickness/2 + padding, 0),
            new Vector2(wallThickness, camHeight));
        
        // 오른쪽
        CreateWall("RightWall", 
            new Vector2(camWidth/2 + wallThickness/2 - padding, 0),
            new Vector2(wallThickness, camHeight));
    }
    
    private void CreateWall(string name, Vector2 position, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.parent = transform;
        wall.transform.position = position;
        wall.tag = "Wall";
        
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = size;
    }
}
