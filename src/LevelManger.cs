using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class LevelManger : Node2D
{
    TileManager tileMapLayer;
    Node chests;
    private Vector2I? _lastPosition;
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
            chest.Connect("ChestStrengthened", new Callable(this, "StrengthenChest"));
            AmendChestPosition(chest);
            _chests[TransformToGrid(chest.GlobalPosition)] = chest;
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if(@event is InputEventMouseButton mouseEvent)
        {   
            //拖动
            if(mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                Vector2I gridPosition = TransformToGrid(GetGlobalMousePosition());
                if(_chests.TryGetValue(gridPosition, out Chest chest))
                {
                    chest.IsGrabbed = true;
                    _lastPosition = gridPosition;
                    GrabbedChest = chest;
                    GrabbedChest.InputPickable = false;
                    _chests.Remove(gridPosition);
                    GD.Print($"Chest at {gridPosition} is grabbed.");
                }
            }
            //吐出
            else if(mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
            {
                Vector2I gridPosition = TransformToGrid(GetGlobalMousePosition());
                if(_chests.TryGetValue(gridPosition, out Chest chest))
                {
                    if(chest.Spit(out Chest spatChest))
                        _chests[TransformToGrid(spatChest.GlobalPosition)] = spatChest;
                }
            }
            //释放
            else if(!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left && GrabbedChest != null)
            {
                Vector2I gridPosition = TransformToGrid(GrabbedChest.GlobalPosition);
                if(_chests.TryGetValue(gridPosition, out Chest chest))
                {
                    if(chest.IsStrengthened && chest.Stats == Chest.Status.Green)
                    {
                        if(chest.ConnectedChest.Swallow(GrabbedChest))
                        {   
                            chest.ConnectedChest.IsStrengthened = false;
                            chest.IsStrengthened = false;

                            chest.ConnectedChest = null;
                            chest.ConnectedChest = null;

                            GrabbedChest.IsGrabbed = false;
                            GrabbedChest = null;
                        }
                    }
                    else
                    {
                        while(!chest.Swallow(GrabbedChest))
                        {
                            if(chest.Spit(out Chest spatChest))
                                _chests[TransformToGrid(spatChest.GlobalPosition)] = spatChest;
                            else
                            {
                                PutChest();
                                break;
                            }
                        }
                        GrabbedChest.IsGrabbed = false;
                        GrabbedChest = null;
                    }                   
                }
                else
                {
                    GrabbedChest.InputPickable = true;
                    
                    PutChest();
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
        chest.GlobalPosition = tileMapLayer.ToGlobal(tileMapLayer.MapToLocal(TransformToGrid(chest.GlobalPosition)));
    }

    public void StrengthenChest(Chest chest)
    {
        switch(chest.Stats)
        {
            case Chest.Status.Green:
                var candidates = _chests.Where(kv => kv.Value.Stats == Chest.Status.Green && kv.Value != chest).ToArray();
                if(candidates.Length > 0)
                {
                    Chest chest1 = candidates[GD.Randi() % candidates.Length].Value;
                    chest1.IsStrengthened = true;
                    chest.IsStrengthened = true;

                    chest1.ConnectedChest = chest;
                    chest.ConnectedChest = chest1;
                }
                break;
            case Chest.Status.Blue:
                chest.IsStrengthened = true;
                break;
            default:
                break;
        }
    }

    public void PutChest()
    {
        if (_chests.TryAdd(TransformToGrid(GrabbedChest.GlobalPosition), GrabbedChest))
        {
            AmendChestPosition(GrabbedChest);
            _lastPosition = null;
            GrabbedChest.IsGrabbed = false;
            GrabbedChest = null;
            return;
        }
            
        
        _chests.TryAdd(_lastPosition.Value,GrabbedChest);
        GrabbedChest.GlobalPosition = tileMapLayer.ToGlobal(tileMapLayer.MapToLocal(TransformToGrid(_lastPosition.Value)));
        AmendChestPosition(GrabbedChest);

        _lastPosition = null;
        GrabbedChest.IsGrabbed = false;
        GrabbedChest = null;
    }
}
