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

    class Ship : Drawable
    {
        private static Model shipModel;
        private static Texture2D shipTexture;
        private static Texture2D thrusterTexture;
        private static float scale;

        private const float SHIP_SIZE = 2.5f;
        private const int DANGER_ZONE = 30;
        private const float MAX_SHIP_SPEED = 50f;
        private const float SHIP_ACCELERATION = 50f;
        private const float TURN_SPEED = 2f;
        private float COOLDOWN = 0.5f;
        private const float THRUSTER_SIZE = 0.4f;
        
        private float cooldownTimer;
        private Matrix rotation;
        private Vector3 forward, up, right;
        private bool thrusterOn;
        public Ship()
        {
            radius = SHIP_SIZE;
            position = Vector3.Zero;
            rotation = Matrix.RotationX((float)Math.PI/2.0f) *
                       Matrix.RotationZ((float)-Math.PI/2.0f);
            forward = Vector3.UnitY;
            up = Vector3.UnitZ;
            right = Vector3.UnitX;
            velocity = new Vector3(0, 0, 0);
            thrusterOn = false;

            cooldownTimer = 0;
        }

        public static void loadContent(ContentManager Content)
        {
            shipModel = Content.Load<Model>("ship");
            shipTexture = Content.Load<Texture2D>("shipTexture");
            thrusterTexture = Content.Load<Texture2D>("thruster2");
            scale = SHIP_SIZE / shipModel.CalculateBounds().Radius;
        }

        public void Update(float time)
        {
            if(cooldownTimer > 0)
                cooldownTimer -= time;
        }

        public void Draw(BasicEffect basicEffect)
        {
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = shipTexture;
            basicEffect.DiffuseColor = (Vector4)Color.White;

            basicEffect.World = Matrix.Scaling(scale) *
                                rotation;

            //actual Matrices are passed in using basicEffect
            shipModel.Draw(basicEffect.GraphicsDevice, Matrix.Identity, Matrix.Identity, Matrix.Identity, basicEffect);
            if (thrusterOn)
            {
                drawThruster(basicEffect);
                thrusterOn = false;
            }
        }
        private void drawThruster(BasicEffect basicEffect)
        {
            basicEffect.DiffuseColor = (Vector4)Color.White;
            GraphicsDevice graphicsDevice = basicEffect.GraphicsDevice;
            GeometricPrimitive plane = GeometricPrimitive.Plane.New(graphicsDevice);
            graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullNone);
            graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.DepthRead);
            graphicsDevice.SetBlendState(graphicsDevice.BlendStates.Additive);
            basicEffect.Texture = thrusterTexture;
            Matrix billboard = Matrix.BillboardRH(Vector3.Zero - Vector3.Multiply(forward, 0.9f * SHIP_SIZE), new Vector3(0, 0, 50), Vector3.UnitY, -Vector3.UnitZ);
            basicEffect.World = Matrix.Scaling(5 * THRUSTER_SIZE) * billboard;
            plane.Draw(basicEffect);
            //clean up
            graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.Default);
            graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.Default);
            graphicsDevice.SetBlendState(graphicsDevice.BlendStates.Default);
        }
        public Vector3 getUp()
        {
            return up;
        }

        public bool isAbleToShoot()
        {
            return cooldownTimer <= 0;
        }

        public void resetTimer()
        {
            cooldownTimer = COOLDOWN;
        }

        public float getDangerZone()
        {
            return DANGER_ZONE * SHIP_SIZE;
        }

        public Vector3 getForward()
        {
            return forward;
        }

        public void pitch(float time)
        {
            rotate(Matrix.RotationAxis(right, TURN_SPEED * time));
        }

        public void yaw(float time)
        {
            rotate(Matrix.RotationAxis(up, TURN_SPEED * time));
        }

        private void rotate(Matrix r)
        {
            rotation *= r;
            forward = (Vector3)Vector3.Transform(forward, r);
            up = (Vector3)Vector3.Transform(up, r);
            right = (Vector3)Vector3.Transform(right, r);
        }

        //negative time for decceleration
        public void accelerate(float time)
        {
            velocity += Vector3.Normalize(forward) * SHIP_ACCELERATION * time;
            if (velocity.Length() > MAX_SHIP_SPEED)
                velocity = Vector3.Normalize(velocity) * MAX_SHIP_SPEED;
            thrusterOn = true;
        }

        public Vector3 move(float time)
        {
            return velocity * time;
        }

        public void resetCooldown()
        {
            COOLDOWN = 0.5f;
        }
       

        public void fastFire()
        {
            if(COOLDOWN>0)
                 COOLDOWN -= 0.05f;
        }
       
    }
}
