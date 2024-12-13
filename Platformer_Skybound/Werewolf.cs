using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer_Skybound
{
    public class Werewolf : Monster
    {
        private const int WerewolfWidth = 120;
        private const int WerewolfHeight = 130;

        private PictureBox _werewolfPictureBox;
        private Dictionary<string, (Image spriteSheet, int frameCount)> _animations;

        private int AnimationInterval = 125;
        private string _currentAnimation;
        private int _currentFrame;
        private bool _isMoving;
        private bool _isFacingLeft = true;

        // Will be used for horizontal movement
        private int _movementRange = 100;
        private int _movementSpeed;

        private System.Windows.Forms.Timer _animationTimer;

        public Werewolf(int health, int movementSpeed, Point startPosition)
            : base(health, movementSpeed, startPosition)
        {
            _animations = new Dictionary<string, (Image spriteSheet, int frameCount)>();
            _movementSpeed = movementSpeed;

            LoadAnimation("attack", Resources.werewolf_attack, 6);
            LoadAnimation("run", Resources.werewolf_run, 9);

            _currentAnimation = "run";
            _currentFrame = 0;
            _isFacingLeft = false;

            _werewolfPictureBox = new PictureBox
            {
                Size = new Size(WerewolfWidth, WerewolfHeight),
                Location = startPosition,
                BackColor = Color.Red
            };

            _animationTimer = new System.Windows.Forms.Timer { Interval = AnimationInterval };
            _animationTimer.Tick += (sender, e) =>
            {
                Animate();
                Move();
            };
            _animationTimer.Start();

            UpdateSprite();
        }


        protected override void Move()
        {
            
        }

        protected override void Animate()
        {
            _currentFrame = (_currentFrame + 1) % _animations[_currentAnimation].frameCount;
            UpdateSprite();
        }

        public PictureBox GetPictureBox() => _werewolfPictureBox;

        private void LoadAnimation(string animationName, byte[] spriteData, int frameCount)
        {
            using (MemoryStream ms = new MemoryStream(spriteData))
            {
                _animations[animationName] = (Image.FromStream(ms), frameCount);
            }
        }

        private void UpdateSprite()
        {
            var (spriteSheet, frameCount) = _animations[_currentAnimation];

            int frameWidth = spriteSheet.Width / frameCount;
            int frameHeight = spriteSheet.Height;

            Rectangle srcRect = new Rectangle(_currentFrame * frameWidth, 0, frameWidth, frameHeight);

            Bitmap currentFrameImage = new Bitmap(frameWidth, frameHeight);

            using (Graphics g = Graphics.FromImage(currentFrameImage))
            {
                // Clear the graphics buffer to avoid leftover artifacts
                g.Clear(Color.Transparent);

                if (_isFacingLeft)
                {
                    // Flip the image horizontally
                    g.TranslateTransform(frameWidth, 0); // Move the origin for flipping
                    g.ScaleTransform(-1, 1); // Flip horizontally
                }

                g.DrawImage(spriteSheet, new Rectangle(0, 0, frameWidth, frameHeight), srcRect, GraphicsUnit.Pixel);
            }

            // Replace the old image safely
            Image oldImage = _werewolfPictureBox.Image;
            _werewolfPictureBox.Image = currentFrameImage;

            // Dispose of the old image to free memory
            oldImage?.Dispose();
        }

    }
}
