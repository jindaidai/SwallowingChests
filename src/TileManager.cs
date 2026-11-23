using Godot;
using System;

public partial class TileManager : TileMapLayer
{
    public Vector2I? FadedCell;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    public void CheckFadedCell(Vector2I cell)
    {
        Vector2I? newFadedCell = GetCellAtlasCoords(cell) == new Vector2I(2,0) ? cell : null;

        if(FadedCell == newFadedCell)
            return;
        
        FadedCell = newFadedCell;
        NotifyRuntimeTileDataUpdate();
    }

    public override bool _UseTileDataRuntimeUpdate(Vector2I coords)
    {
        return GetCellAtlasCoords(coords) == new Vector2I(2,0);
    }

    public override void _TileDataRuntimeUpdate(Vector2I coords, TileData tileData)
    {
        base._TileDataRuntimeUpdate(coords, tileData);
        if(coords == FadedCell)
            tileData.SetModulate(new Color(1,1,1,0.3f));
        else
            tileData.SetModulate(Colors.White);
    }
}
