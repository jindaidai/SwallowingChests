using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Chest : CharacterBody2D
{
    [Signal] public delegate void ChestStrengthenedEventHandler(Chest chest);
    PackedScene particles;
    AnimationPlayer animationPlayer;
    Node2D rayCast;
    Sprite2D sprite;
    public LevelManger levelManger;
    public enum Status
    {
        White,Green,Blue,Black,Gloden
    }
    [Export] private Status _stats;
    
    public Status Stats 
    {
        get => _stats;
        set
        {
            _stats = value;
            switch(_stats)
            {
                case Status.White:
                    sprite.Modulate = new Color(1,1,1,1);
                    SpawnParticles(new Color(1,1,1,1));
                    Weight = 1;
                    _maxWeight = 1;
                    break;
                case Status.Green:
                    sprite.Modulate = new Color(91/255f,168/255f,66/255f,1);
                    SpawnParticles(new Color(91/255f,168/255f,66/255f,1));
                    Weight = 2;
                    _maxWeight = 2;
                    break;
                case Status.Blue:
                    sprite.Modulate = new Color(60/255f,121/255f,207/255f,1);
                    SpawnParticles(new Color(60/255f,121/255f,207/255f,1));
                    SetCollisionMaskValue(2,false);
                    Weight = 3;
                    _maxWeight = 3;
                    break;
                case Status.Black:
                    sprite.Modulate = new Color(0,0,0,1);
                    SpawnParticles(new Color(0,0,0,1));
                    Weight = 4;
                    _maxWeight = 4;
                    break;
                case Status.Gloden:
                    sprite.Modulate = new Color(255,215,0,1);
                    SpawnParticles(new Color(255,215,0,1));
                    Weight = 2;
                    _maxWeight = 0;
                    break;
            }
        }
    }
    private int _maxWeight;
    public int Weight;
    public Chest ConnectedChest;
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
                ZIndex = 1;
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
    public bool IsStrengthened = false;

    public override void _Ready()
    {
        base._Ready();
        particles = GD.Load<PackedScene>("uid://cuogldqi7px2n");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        sprite = GetNode<Sprite2D>("Sprite2D");
        rayCast  = GetNode<Node2D>("RayCast");
        Stats = _stats;   

        Connect(CharacterBody2D.SignalName.MouseEntered,Callable.From(() => {if(levelManger.GrabbedChest != null && !IsGrabbed) PlayAnimation("open");}));
        Connect(CharacterBody2D.SignalName.MouseExited,Callable.From(() => {if(GetCurrentAnimation() == "open") PlayAnimation("normal");}));
    }

    public bool Swallow(Chest chest)
    {   
        PlayAnimation("swallow");
        if(Stats == Status.White)
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
        else if(chest.Stats == Status.Green)
        {
            EmitSignal("ChestStrengthened", this);

            chest.QueueFree();
        }
        else
        {
            if(Weight + chest.Weight > 2 * _maxWeight)
                return false;
            Weight += chest.Weight;

            Chests.Push(chest);
            chest.Deactivate();
        } 
        return true;
    }

    public bool Spit(out Chest chest)
    {
        foreach(RayCast2D rayCast2D in rayCast.GetChildren())
        {
            if(!rayCast2D.IsColliding())
            {
                if(Chests.TryPop(out Chest chest1))
                {
                    Vector2 dir = rayCast2D.TargetPosition.Normalized();
                    chest1.GlobalPosition = GlobalPosition + dir * 16;
                    chest1.Activate();
                    Weight -= chest1.Weight;

                    chest = chest1;
                    return true;
                }
                else
                {   
                    chest = null;
                    return false;
                }
                
            }
        }
        chest = null;
        return false;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if(IsGrabbed)
            Drag();


        MoveAndSlide();
    }
    
    public void Drag()
    {
        if(GlobalPosition.DistanceTo(GetGlobalMousePosition()) < 2)
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
        Show();
        
        ProcessMode = Node.ProcessModeEnum.Inherit;
    }

    public void SpawnParticles(Color color)
    {
        var particleInstance = particles.Instantiate<SpawnParticle>();
        particleInstance.ChangeColor(color);
        AddChild(particleInstance);
        particleInstance.GlobalPosition = GlobalPosition;
    }
}
