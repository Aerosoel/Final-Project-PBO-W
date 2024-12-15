using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer_Skybound
{
    public interface IFlying
    {
        protected void AnimateHover();
    }

    public abstract class Monster
    {
        public int Health { get; protected set; }
        public int MovementSpeed { get; protected set; }
        public Point Position { get; protected set; }

        protected Monster(int health, int movementSpeed, Point startPoint)
        {
            Health = health;
            MovementSpeed = movementSpeed;
            Position = startPoint;
        }

        public virtual void TakeDamage(int damage)
        {
            Health -= damage;
            if(Health < 0) Health = 0;
        }

        public bool IsAlive() => Health > 0;

        public abstract void Move();

        protected abstract void Animate();

        public void TakeDamage()
        {
            Health -= 1;
        }
    }
}
