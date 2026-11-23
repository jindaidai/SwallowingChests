using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class LevelManger : Node2D
{
    TileManager tileMapLayer;
    Node chests;
    
    public Chest GrabbedChest;
    Dictionary<Vector2I, Chest> _chests = new();

    public override void _Ready()
    {
        base._Ready();
        chests = GetNode("Chests");
        tileMapLayer = GetNode<TileManager>("Map");

        foreach (Chest chest in chests.GetChildren())
        {
            chest.levelManger = this;
            AmendChestPosition(chest);
            _chests[TransformToGrid(chest.GlobalPosition)] = chest;
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if(@event is InputEventMouseButton mouseEvent)
        {  
            if(mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                Vector2I gridPosition = TransformToGrid(GetGlobalMousePosition());
                if(_chests.TryGetValue(gridPosition, out Chest chest))
                {
                    chest.IsGrabbed = true;
                    GrabbedChest = chest;
                    GrabbedChest.InputPickable = false;
                    _chests.Remove(gridPosition);
                    GD.Print($"Chest at {gridPosition} is grabbed.");
                }
            }
            else if(mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
            {
                Vector2I gridPosition = TransformToGrid(GetGlobalMousePosition());
                if(_chests.TryGetValue(gridPosition, out Chest chest))
                {
                    Chest SpatChest = chest.Spit();
                    if(SpatChest != null)
                        _chests[TransformToGrid(SpatChest.GlobalPosition)] = SpatChest;
                }
            }
            else if(!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left && GrabbedChest != null)
            {
                Vector2I gridPosition = TransformToGrid(GrabbedChest.GlobalPosition);
                if(_chests.TryGetValue(gridPosition, out Chest chest))
                {
                    chest.Swallow(GrabbedChest);
                    GrabbedChest.IsGrabbed = false;
                    GrabbedChest = null;
                }
                else
                {
                    GrabbedChest.IsGrabbed = false;
                    GrabbedChest.InputPickable = true;
                    AmendChestPosition(GrabbedChest);
                    _chests[TransformToGrid(GrabbedChest.GlobalPosition)] = GrabbedChest;
                    GD.Print($"Chest at {TransformToGrid(GrabbedChest.GlobalPosition)} is released.");
                    GrabbedChest = null;
                    
                }
                
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if(GrabbedChest != null)
        {
            Vector2I nextCell = TransformToGrid(GrabbedChest.GlobalPosition);
            tileMapLayer.CheckFadedCell(nextCell);
        }
    }

    public Vector2I TransformToGrid(Vector2 position)
    {
        Vector2 localPosition = tileMapLayer.ToLocal(position);
        return tileMapLayer.LocalToMap(localPosition);
    }

    public void AmendChestPosition(Chest chest)
    {
        chest.GlobalPosition = tileMapLayer.MapToLocal(TransformToGrid(chest.GlobalPosition));
    }
}
