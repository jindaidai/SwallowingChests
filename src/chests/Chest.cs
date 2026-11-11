using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Chest : CharacterBody2D
{
    AnimationPlayer animationPlayer;
    public enum Status
    {
        White,Green,Blue,Black,Gloden
    }
    public Status Stats;
    public int Weight;
    public Stack<Chest> Chests = new();

    public override void _Ready()
    {
        base._Ready();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void Swallow(Chest chest)
    {
        if(Weight + chest.Weight >= 2 * Weight)
            return;
        Weight += chest.Weight;
        if(Stats != Status.White)
        {
            Chests.Push(chest);
            chest.Deactivate();
        }
        else
        {
            Stats = chest.Stats switch
            {
                Status.White => Status.Green,
                Status.Gloden => Status.Green,
                Status.Green => Status.Blue,
                Status.Blue => Status.Black,
                _ => Stats
            };
            chest.QueueFree();
        } 
    }

    public void Deactivate()
    {
        DisableMode = CollisionObject2D.DisableModeEnum.Remove;
        ProcessMode = Node.ProcessModeEnum.Disabled;
        Hide();
    }

    public void Activate()
    {
        ProcessMode = Node.ProcessModeEnum.Inherit;
        Show();
    }
}
