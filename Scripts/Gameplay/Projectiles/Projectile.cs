using Godot;
using System;

public partial class Projectile : Area2D
{
	[Export] public float Speed = 800f; //speed, in pixels
	protected Enemy _target;
	protected float _damage;

	protected Vector2 dir;

    /* public void Init(Vector2 from, Enemy target, float damage, float speed, float critChance, float critMult, float shotSpread, float spreadFalloff)
     {
         GlobalPosition = from;
         _target = target;
         _damage = damage;
         Speed = speed;

         //TODO: implement crit chance and crit mult (Getting damage should be a function that is called and incorporates getting a potential crit)
         //      and shot spread/falloff 

         //do a basic readahead based on distance to target - this could cause issues if the target is very far away/fast moving/on switchbacks/changing how close they are to the tower drastically, and we might need to revisit
         float timeToTarget = ((_target.GlobalPosition - from).Length()) / Speed;


         dir = (target.Follower.GetFuturePosition(timeToTarget) - from).Normalized();

     }*/

    public void Init(Vector2 from, Enemy target, float damage, float speed, float critChance, float critMult, float shotSpread, float spreadFalloff)
    {
        GlobalPosition = from;
        _target = target;
        _damage = damage;
        Speed = speed;

        
        
        if (_target != null && IsInstanceValid(_target))
        {
            dir = (_target.GlobalPosition - GlobalPosition).Normalized();
        }
    }



    /* public override void _PhysicsProcess(double delta)
     {
         //Note: we will likely want to change this to check for anything on the proper layer, instead of the specific enemy instance - that'll future proof us for "spray" weapons.

         if (_target == null || !IsInstanceValid(_target))
         {
             QueueFree();
             return;
         }

         // Fly straight at current target position (no prediction)
         //Vector2 dir = (_target.GlobalPosition - GlobalPosition).Normalized();

         SetGlobalPosition(GlobalPosition + dir * Speed * (float)delta);


         // Cheap hit test – good enough to prove the loop
         if (GlobalPosition.DistanceTo(_target.GlobalPosition) < 8f)
         {
             _target.TakeDamage(_damage);
             QueueFree();
         }


     }*/

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || !IsInstanceValid(_target))
        {
            QueueFree();
            return;
        }

        GlobalPosition += dir * Speed * (float)delta;

        float hitRadius = 32f; 
        if (GlobalPosition.DistanceTo(_target.GlobalPosition) < hitRadius)
        {
            _target.TakeDamage(_damage);
            QueueFree();
        }
    }


}
