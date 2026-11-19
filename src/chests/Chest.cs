using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Chest : CharacterBody2D
{
    AnimationPlayer animationPlayer;
    Sprite2D sprite;
    public LevelManger levelManger;
    public enum Status
    {
        White,Green,Blue,Black,Gloden
    }
    public Status Stats;
    public int Weight;
    public Stack<Chest> Chests = new();
    private bool _isGrabbed = false;
    public bool IsGrabbed
    {
        get => _isGrabbed;
        set
        {
            _isGrabbed = value;
            if(_isGrabbed)
            {
                ZIndex = 100;
                sprite.Scale = new Vector2(1.2f, 1.2f);
            }
            else
            {
                ZIndex = 0;
                sprite.Scale = Vector2.One;
                Velocity = Vector2.Zero;
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        sprite = GetNode<Sprite2D>("Sprite2D");

        Connect(CharacterBody2D.SignalName.MouseEntered,Callable.From(() => {if(levelManger.GrabbedChest != null && !IsGrabbed) PlayAnimation("open");}));
        Connect(CharacterBody2D.SignalName.MouseExited,Callable.From(() => {if(GetCurrentAnimation() == "open") PlayAnimation("normal");}));
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

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if(IsGrabbed)
        {
            Drag();
        }

        MoveAndSlide();
    }
    
    public void Drag()
    {
        if(GlobalPosition.DistanceTo(GetGlobalMousePosition()) < 4)
        {
            Velocity = Vector2.Zero;
            return;
        }
            
        Velocity = GlobalPosition.DirectionTo(GetGlobalMousePosition()) * GlobalPosition.DistanceTo(GetGlobalMousePosition()) * 10;
    }



    public void PlayAnimation(StringName animationName)
    {
        if(animationPlayer.CurrentAnimation != animationName)
            animationPlayer.Play(animationName);
    }

    public StringName GetCurrentAnimation()
    {
        return animationPlayer.CurrentAnimation;
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
