using GameStateManagement.Class;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameStateManagement.Class
{
    internal class Enemy
    {
        private Texture2D EnemyTexture;


        private readonly List<Vector2> enemyPositions = new List<Vector2>();

        private Vector2 enemyStartPosition = new Vector2(100, 100);
        private float enemyRadius;
        private float enemySpeed = 1f;
        private Color enemyColor;
        private SoundEffect explosionSound;
        private Random random = new Random();


        public Enemy(Texture2D EnemyTexture, SoundEffect explosionSound)
        {
            this.EnemyTexture = EnemyTexture;
            this.explosionSound = explosionSound;

        }

        public void calcRadius()
        {
            if (EnemyTexture != null)
            {
                if (EnemyTexture.Width > EnemyTexture.Height)
                {
                    enemyRadius = EnemyTexture.Width;
                }
                else
                {
                    enemyRadius = EnemyTexture.Height;
                }

                // Gegner erzeugen
                CreateEnemies();
            }

        }


        public void PlayExplosionSound()
        {
            explosionSound.Play();
        }

        public void CreateEnemies()
        {
            // Feinde erzeugen
            Vector2 position = enemyStartPosition;
            position.X -= EnemyTexture.Width / 2;

            // Eine Zufallszahl zwischen 3 und 10 ermitteln
            int count = random.Next(3, 11);

            // Gegener erzeugen
            for (int i = 0; i < count; i++)
            {
                enemyPositions.Add(position);
                position.X += EnemyTexture.Width + 15f;
            }

            // Farbwert ändern
            switch (count)
            {
                case 3:
                    enemyColor = Color.Red;
                    break;
                case 4:
                    enemyColor = Color.Green;
                    break;
                case 5:
                    enemyColor = Color.Yellow;
                    break;
                case 6:
                    enemyColor = Color.Blue;
                    break;
                case 7:
                    enemyColor = Color.Magenta;
                    break;
                case 8:
                    enemyColor = Color.Yellow;
                    break;
                case 9:
                    enemyColor = Color.White;
                    break;
                case 10:
                    enemyColor = Color.DarkGreen;
                    break;
                default:
                    break;
            }
        }


        public void UpdateEnemies()
        {
            // Startposition verändern
            enemyStartPosition.X += enemySpeed;

            // Bewegungsrichtung umkehren
            if (enemyStartPosition.X > 250)
            {
                enemySpeed *= -1;
            }
            else if (enemyStartPosition.X < 100f)
            {
                enemySpeed *= -1;
            }

            // Alle Feinde abgeschossen? Dann Neue Gegener
            if (enemyPositions.Count == 0 && EnemyTexture != null)
            {
                CreateEnemies();
            }

            // Aktualisieren
            for (int i = 0; i < enemyPositions.Count; i++)
            {
                Vector2 position = enemyPositions[i];
                position.X += enemySpeed;
                enemyPositions[i] = position;
            }
        }





        public Texture2D getEnemyTexture()
        {
            return EnemyTexture;
        }

        public List<Vector2> getEnemyPositions()
        {
            return enemyPositions;
        }

       public float getEnemyRadius()
        {
            return enemyRadius;
        }

        public float getEnemySpeed()
        {
            return enemySpeed;
        }

        public Color getEnemyColor()
        {
            return enemyColor;
        }

        public SoundEffect getExplosioSound()
        {
            return explosionSound;
        }

        public void setEnemyTexture(Texture2D ene)
        {
            this.EnemyTexture = ene;
        }

        public void setEnemyRadius(float radius)
        {
            this.enemyRadius = radius;

        }

        public void setEnemySpeed(float speed)
        {
            this.enemySpeed = speed;
        }

        public void setEnemyColor(Color color)
        {
            this.enemyColor = color;
        }

        public void setExplosionSound(SoundEffect effect)
        {
            this.explosionSound = effect;
        }






    }


}
 

