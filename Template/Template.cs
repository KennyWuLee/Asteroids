using System;
using System.Text;
using SharpDX;
using System.Collections.Generic;

namespace Template
{
    // Use these namespaces here to override SharpDX.Direct3D11
    using SharpDX.Toolkit;
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    /// <summary>
    /// Simple Template game using SharpDX.Toolkit.
    /// </summary>
    public class Template : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;
        private SpriteFont arial16Font;
        private SpriteFont arial32Font;
        private Matrix view;
        private Matrix projection;

        private Ship ship;
        private List<Asteroid> asteroids;
        private List<Laser> lasers;
        private Message message;
        private Explosion explosion;

        private BasicEffect basicEffect;
        private TextureCube[] skyboxes;
        private Effect skyboxEffect;
        GeometricPrimitive cube;
        private KeyboardManager keyboard;
        private KeyboardState keyboardState;
        private int lives;
        private int level;
        private int score;
        private Random random;
        private bool firstPerson;
        private float enterTimer;
        private int power;
        private int fireRate;
        private int asteroidSpeed;
        private bool active;
        /// <summary>
        /// Initializes a new instance of the <see cref="Template" /> class.
        /// </summary>
        public Template()
        {
            // Creates a graphics manager. This is mandatory.
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";

            // Initialize input keyboard system
            keyboard = new KeyboardManager(this);

            asteroids = new List<Asteroid>();
            lasers = new List<Laser>();
            message = new Message();
            random = new Random();
            lives = 3;
            level = 1;
            score = 0;
            firstPerson = false;
            enterTimer = 0f;
            skyboxes = new TextureCube[3];
            power = 0;
            fireRate = 0;
            asteroidSpeed = 0;
            active = true;
            explosion = new Explosion();
        }

        protected override void Initialize()
        {
            // Modify the title of the window
            Window.Title = "Template";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Instantiate a SpriteBatch
            spriteBatch = ToDisposeContent(new SpriteBatch(GraphicsDevice));

            // Loads a sprite font
            // The [Arial16.xml] file is defined with the build action [ToolkitFont] in the project
            arial16Font = Content.Load<SpriteFont>("Arial16");
            arial32Font = Content.Load<SpriteFont>("Arial16");

            // Creates a basic effect
            basicEffect = ToDisposeContent(new BasicEffect(GraphicsDevice));
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.EnableDefaultLighting();

            skyboxEffect = Content.Load<Effect>("skybox");
            cube = GeometricPrimitive.Cube.New(GraphicsDevice);

            skyboxes[0] = Content.Load<TextureCube>("DeepSpaceRedWithPlanet");
            skyboxes[1] = Content.Load<TextureCube>("DeepSpaceBlueWithPlanet");
            skyboxes[2] = Content.Load<TextureCube>("DeepSpaceGreenWithPlanet");

            ship = new Ship();
            Ship.loadContent(Content);
            Asteroid.loadContent(Content);
            Laser.loadContent(Content);
            Explosion.loadContent(Content);
            message.displayMessage("Level 1", 2f);

            //Asteroid test = new Asteroid(8, new Vector3(40, 0, 0), new Vector3(-4, 0, 0), new Vector3(1, 1, 1), ship);
            //asteroids.Add(test);

            Asteroid.generateAstroids(200, asteroids, random, ship);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var time = (float)gameTime.ElapsedGameTime.Milliseconds / 1000f;
            if (score >= 1000 * level)
            {
                asteroids.Clear();
                Asteroid.resetSpecialCount();
                Asteroid.setLevel(level);
                Asteroid.generateAstroids(200 + 100 * level, asteroids, random, ship);
                level++;
                lives++;
          
                lasers.Clear();
                message.displayMessage("Level " + level, 2f);
            }
            if (lives < 0)
            {
                active = false;
                message.displayMessage("Game Over");
            }

            // Calculates the world and the view based on the model size
            if (firstPerson)
                view = Matrix.LookAtRH(Vector3.Zero, Vector3.Zero + ship.getForward(), ship.getUp());
            else
                view = Matrix.LookAtRH(new Vector3(0.0f, 0f, 50f), new Vector3(0, 0, 0), Vector3.UnitY);
            projection = Matrix.PerspectiveFovRH(0.9f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, 0.1f, 1500.0f);

            // Update basic effect for rendering the Primitive
            basicEffect.View = view;
            basicEffect.Projection = projection;
            if(active)
                handleInput(time);

            Vector3 shipMovement = ship.move(time);
            offsetWorld(-shipMovement);
            ship.Update(time);
            foreach (Asteroid a in asteroids)
                a.Update(time);
            foreach (Laser l in lasers)
                l.Update(time);
            message.Update(time);
            explosion.Update(time);

            //ship asteroid collision
            foreach (Asteroid a in asteroids)
                if (a.collidesWith(ship))
                {
                    explosion.explode();
                    message.displayMessage("You Died!", 1f);
                    asteroids.Clear();
                    Asteroid.generateAstroids(200 + 100 * level, asteroids, random, ship);
                    lives--;
                    lasers.Clear();
                    reset();
                    break;
                }
            List<Asteroid> newAstroids = new List<Asteroid>();
            foreach (Laser l in lasers)
            {
                //remove out of Range
                if (!l.isInRange())
                    l.kill();
                //collision with asteroids
                foreach (Asteroid a in asteroids)
                    if (l.collidesWith(a))
                    {
                        Asteroid temp = laserAstroidCollision(l, a);
                        if (temp != null)
                            newAstroids.Add(temp);
                    }
            }
            lasers.RemoveAll(l => !l.isAlive());
            asteroids.RemoveAll(a => !a.isAlive());
            asteroids.AddRange(newAstroids);
        }

        private Asteroid laserAstroidCollision(Laser l, Asteroid a)
        {
            int radius = (int)a.getRadius();
            Asteroid newAstroid = null;
            if (a.special)
                powerUp();
            if (radius == 8)
            {
                a.kill();
                score += 100;
            }
            else if (radius == 16 || radius == 32)
            {
                score += 50;
                a.halfSize();
                a.special = false;
                newAstroid = a.clone();
                Vector3 randomAngVel = new Vector3(random.NextFloat(-1, 1), random.NextFloat(-1, 1), random.NextFloat(-1, 1));
                a.setAngularVelocity(randomAngVel);
                newAstroid.setAngularVelocity(randomAngVel);
                Vector3 explosion = Vector3.Cross(l.getVelocity(), a.getVelocity());
                explosion.Normalize();
                a.offset(explosion * a.getRadius());
                newAstroid.offset(explosion * -a.getRadius());
                a.setVelocity(a.getVelocity() + 40f / a.getRadius() * explosion);
                newAstroid.setVelocity(a.getVelocity() - 40f / a.getRadius() * explosion);
            }
            l.kill();
            return newAstroid;
        }
        public void powerUp()
        {
            int x = random.Next(3);
            switch (x)
            {
                case 0://fire faster
                    ship.fastFire();
                    fireRate++;
                    break;
                case 1://fire larger blasts
                    Laser.doubleSize();
                    power++;
                    break;
                case 2://slow asteroids
                    foreach (Asteroid a in asteroids)
                        a.slowAsteroid();
                    asteroidSpeed++;;
                    break;
                default:
                    break;

            }
        }
        public void reset()
        {
            ship.resetCooldown();
            Laser.resetSize();
            foreach (Asteroid a in asteroids)
                a.resetAsteroidSpeed();
            fireRate = 0;
            asteroidSpeed = 0;
            power = 0;
            
        }
        private void offsetWorld(Vector3 offset)
        {
            foreach (Asteroid a in asteroids)
                a.offset(offset);
            foreach (Laser l in lasers)
                l.offset(offset);
        }
        private void handleInput(float time)
        {
            // Get the current state of the keyboard
            keyboardState = keyboard.GetState();
            List<Keys> keys = new List<Keys>();
            keyboardState.GetDownKeys(keys);

            if (keys.Contains(Keys.Up))
                ship.pitch(time);
            if (keys.Contains(Keys.Down))
                ship.pitch(-time);
            if (keys.Contains(Keys.Left))
                ship.yaw(time);
            if (keys.Contains(Keys.Right))
                ship.yaw(-time);

            if (keys.Contains(Keys.Control))
                ship.accelerate(time);
            if (keys.Contains(Keys.Alt))
                ship.accelerate(-time);
            if (keys.Contains(Keys.Enter) && enterTimer <= 0)
            {
                firstPerson = !firstPerson;
                enterTimer = .25f;
            }
            enterTimer -= time;

            if (keys.Contains(Keys.Space) && ship.isAbleToShoot())
            {
                lasers.Add(new Laser(Vector3.Zero, ship.getForward(), ship.getVelocity()));
                ship.resetTimer();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // Use time in seconds directly
            var time = (float)gameTime.TotalGameTime.TotalSeconds;

            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.Default);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            drawSkybox();
            foreach (Asteroid a in asteroids)
                a.Draw(basicEffect, firstPerson);
            if (!firstPerson)
                ship.Draw(basicEffect);
           
            foreach (Laser l in lasers)
                l.Draw(basicEffect, firstPerson, ship);
            if (message.isActive())
                message.Draw(spriteBatch, arial32Font);
            if (explosion.isActive() && !firstPerson)
                explosion.Draw(spriteBatch, basicEffect);
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Asteroids: " + asteroids.Count).AppendLine();
            text.Append("fps: " + 1 / (float)gameTime.ElapsedGameTime.TotalSeconds);
            var scoreText = new StringBuilder("Score: " + score).AppendLine();
            var livesText = new StringBuilder("Lives: " + lives).AppendLine();
            var levelText = new StringBuilder("Level: " + level).AppendLine();
            var fireText = new StringBuilder("Fire Rate: " + fireRate).AppendLine();
            var powerText = new StringBuilder("Power: " + power).AppendLine();
            var asteroidText = new StringBuilder("ASpeed: " + asteroidSpeed).AppendLine();
            List<Keys> keys = new List<Keys>();

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(16, 16), Color.White);
            spriteBatch.DrawString(arial16Font, scoreText.ToString(), new Vector2(GraphicsDevice.BackBuffer.Width - 125, 10), Color.White);
            spriteBatch.DrawString(arial16Font, livesText.ToString(), new Vector2(GraphicsDevice.BackBuffer.Width - 125, GraphicsDevice.BackBuffer.Height - 30), Color.White);
            spriteBatch.DrawString(arial16Font, powerText.ToString(), new Vector2(GraphicsDevice.BackBuffer.Width - 125, GraphicsDevice.BackBuffer.Height - 50), Color.White);
            spriteBatch.DrawString(arial16Font, fireText.ToString(), new Vector2(GraphicsDevice.BackBuffer.Width - 125, GraphicsDevice.BackBuffer.Height - 70), Color.White);
            spriteBatch.DrawString(arial16Font, asteroidText.ToString(), new Vector2(GraphicsDevice.BackBuffer.Width - 125, GraphicsDevice.BackBuffer.Height - 90), Color.White);

            spriteBatch.DrawString(arial16Font, levelText.ToString(), new Vector2(10, GraphicsDevice.BackBuffer.Height - 30), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void drawSkybox()
        {
            Matrix world = Matrix.Scaling(1500);

            skyboxEffect.Parameters["World"].SetValue(world);
            skyboxEffect.Parameters["View"].SetValue(view);
            skyboxEffect.Parameters["Projection"].SetValue(projection);
            skyboxEffect.Parameters["CameraPosition"].SetValue(Vector3.Zero);
            skyboxEffect.Parameters["SkyBoxTexture"].SetResource(skyboxes[level % 3]);


            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullFront);
            cube.Draw(skyboxEffect);
            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullBack);
        }

    }
}
