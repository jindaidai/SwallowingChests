using Godot;
using System;

public partial class SpawnParticle : GpuParticles2D
{
    public override void _Ready()
    {
        base._Ready();
        Connect("finished", new Callable(this, "OnFinished"));
        OneShot = true;
        Emitting = true;
    }

    public void ChangeColor(Color color)
    {
        var material = (ParticleProcessMaterial)ProcessMaterial;
        material.Color = color;
    }

    public void OnFinished()
    {
        QueueFree();
    }
}
