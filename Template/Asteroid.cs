using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    using SharpDX;
    using SharpDX.Toolkit.Content;
    using SharpDX.Toolkit.Graphics;

    class Asteroid : Drawable
    {
        private static Model modelHigh, modelMedium, modelLow;
        private static Texture2D textureHigh, textureMedium, textureLow;
        private static float scaleHigh, scaleMedium, scaleLow;
        private static Vector3 offsetHigh, offsetMedium, offsetLow;

        private const int ASTEROID_SPEED = 20;
        private const float RANGE = 750f;
        private const float RANGE_FRACTION = 0.1f;
        private const float MINIMUM_ASTEROID = 8;

        private Vector3 rotationVelocity;
        private Vector3 firstVelocity;
        private Matrix rotation;
        private Ship ship;
        public bool special;
        Random random;
        private static int specialCount;
        private static int level;
        public Asteroid(float radius, Vector3 position, Vector3 velocity, Vector3 rotationVelocity, Ship ship)
        {
            random = new Random();
            this.radius = radius;
            this.position = position;
            this.velocity = velocity;
            firstVelocity = velocity;
            this.rotationVelocity = rotationVelocity;
            this.ship = ship;
            rotation = Matrix.Identity;  
            if (specialCount <= 10 * level)
            {
                 special = true;
                 specialCount++;
            }
            else
                special = false;

        }
        public static void setLevel(int levels)
        {
            level = levels;
        }
        public void Update(float time)
        {
            rotation *= Matrix.RotationX(rotationVelocity.X * time) *
                        Matrix.RotationY(rotationVelocity.Y * time) *
                        Matrix.RotationZ(rotationVelocity.Z * time);
            position += velocity * time;
            //wrap around physics
            if (Math.Abs(position.X) > RANGE)
                position.X -= Math.Sign(position.X) * 2 * RANGE;
            if (Math.Abs(position.Y) > RANGE)
                position.Y -= Math.Sign(position.Y) * 2 * RANGE;
            if (Math.Abs(position.Z) > RANGE)
                position.Z -= Math.Sign(position.Z) * 2 * RANGE;
        }

        public void Draw(BasicEffect basicEffect, bool firstPerson)
        {
            Vector3 cameraPosition;
            if(firstPerson)
                cameraPosition = Vector3.Zero;
            else
                cameraPosition = new Vector3(0, 0, 50);

            Model model;
            Matrix transform;
            float sizeOnScreen = radius / Vector3.Distance(position, cameraPosition);
            //Console.Out.Write(sizeOnScreen + "\t");
            if(sizeOnScreen > .025)
            {
                //Console.Out.WriteLine("high");
                basicEffect.Texture = textureHigh;
                model = modelHigh;
                transform = Matrix.Translation(offsetHigh) *
                            Matrix.Scaling(scaleHigh * radius);
            }
            else if (sizeOnScreen > .015)
            {
                //Console.Out.WriteLine("medium");
                basicEffect.Texture = textureMedium;
                model = modelMedium;
                transform = Matrix.Translation(offsetMedium) *
                            Matrix.Scaling(scaleMedium * radius);
            }
            else
            {
                //Console.Out.WriteLine("low");
                basicEffect.Texture = textureLow;
                model = modelLow;
                transform = Matrix.Translation(offsetLow) *
                            Matrix.Scaling(scaleLow * radius);
            }
            
            if (isInDangerZone())
                basicEffect.DiffuseColor = (Vector4)Color.Red;
            else if(special)
                basicEffect.DiffuseColor = (Vector4)Color.Purple;
            else
                basicEffect.DiffuseColor = (Vector4)Color.White;

            basicEffect.World = transform *
                                rotation *
                                Matrix.Translation(position);
            //actual Matrices are passed in using basicEffect
            model.Draw(basicEffect.GraphicsDevice, Matrix.Identity, Matrix.Identity, Matrix.Identity, basicEffect);
        }

        public static void loadContent(ContentManager Content)
        {
            modelHigh = Content.Load<Model>("rock1Model");
            textureHigh = Content.Load<Texture2D>("rock1Texture");
            modelMedium = Content.Load<Model>("rock2Model");
            textureMedium = Content.Load<Texture2D>("rock2Texture");
            modelLow = Content.Load<Model>("rock3Model");
            textureLow = Content.Load<Texture2D>("rock3Texture");

            BoundingSphere h, m, l;
            h = modelHigh.CalculateBounds();
            m = modelMedium.CalculateBounds();
            l = modelLow.CalculateBounds();

            scaleHigh = 1 / h.Radius;
            scaleMedium = 1 / m.Radius;
            scaleLow = 1 / l.Radius;
            offsetHigh = - h.Center;
            offsetMedium = - m.Center;
            offsetLow = - l.Center;
        }

        public void setAngularVelocity(Vector3 angularVelocity)
        {
            rotationVelocity = angularVelocity;
        }

        public void setVelocity(Vector3 velocity)
        {
            this.velocity = velocity;
        }

        private bool isInDangerZone()
        {
            return distanceTo(ship) < ship.getDangerZone() + radius;
        }

        public void halfSize()
        {
            if (radius == 16 || radius == 32)
                radius /= 2;
        }

        public Asteroid clone()
        {
            return new Asteroid(radius, position, velocity, rotationVelocity, ship);
        }

        public static void generateAstroids(int number, List<Asteroid> asteroids, Random random, Ship ship)
        {
            for (int i = 0; i < number; ++i)
            {
                float radius = (float)(MINIMUM_ASTEROID * Math.Pow(2, random.Next(0, 3)));
                Vector3 position;
                do
                    position = new Vector3(random.NextFloat(-675, 675), random.NextFloat(-675, 675), random.NextFloat(-675, 675));
                while (closeToOtherObject(position, asteroids));
                Vector3 velocity = new Vector3(random.NextFloat(-ASTEROID_SPEED, ASTEROID_SPEED), random.NextFloat(-ASTEROID_SPEED, ASTEROID_SPEED), random.NextFloat(-ASTEROID_SPEED, ASTEROID_SPEED));
                Vector3 angularVelocity = new Vector3(random.NextFloat(-1, 1), random.NextFloat(-1, 1), random.NextFloat(-1, 1));
                asteroids.Add(new Asteroid(radius, position, velocity, angularVelocity, ship));
            }
        }

        private static bool closeToOtherObject(Vector3 position, List<Asteroid> asteroids)
        {
            //ship at (0,0,0)
            if (position.Length() < RANGE * RANGE_FRACTION)
                return true;
            foreach (Asteroid a in asteroids)
                if (Vector3.Distance(position, a.getPosition()) < RANGE * RANGE_FRACTION)
                    return true;
            return false;
        }

        public void slowAsteroid()
        {
            velocity *= .9f;
        }
        public void resetAsteroidSpeed()
        {
            velocity = firstVelocity;
        }
        public static void resetSpecialCount()
        {
            specialCount = 0;
        }
    }
}
