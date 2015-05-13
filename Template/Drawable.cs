using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    using SharpDX;
    using SharpDX.Toolkit.Graphics;

    abstract class Drawable
    {
        protected Vector3 position;
        protected Vector3 velocity;
        protected float radius;
        protected bool alive;

        public Drawable()
        {
            alive = true;
        }

        public Vector3 getPosition()
        {
            return position;
        }

        public Vector3 getVelocity()
        {
            return velocity;
        }

        public float getRadius()
        {
            return radius;
        }

        public void offset(Vector3 offset)
        {
            position += offset;
        }

        public bool collidesWith(Drawable drawable)
        {
            return distanceTo(drawable) < radius + drawable.radius;
        }

        public float distanceTo(Drawable drawable)
        {
            return Vector3.Distance(position, drawable.position);
        }

        public void kill()
        {
            alive = false;
        }

        public bool isAlive()
        {
            return alive;
        }
    }
}
