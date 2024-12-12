using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer_Skybound
{
    public class Tengu : Monster, IFlying
    {
        private const int TenguWidth = 110;
        private const int TenguHeight = 130;

        private PictureBox _tenguPictureBox;
        private Dictionary<string, (Image spriteSheet, int frameCount)> _animations;

        private int AnimationInterval = 125;
        private int _movementSpeed;
        private string _currentAnimation;
        private int _currentFrame;
        private bool _isMoving;
        private bool _isFacingLeft;

        // Will be used for hovering movement
        private int _hoverRange = 40; // Maximum distance to hover up and down
        private int _hoverStep = 10;   // Step size for each hover movement
        private bool _hoveringUp = true; // Whether the Tengu is currently moving up
        private int _initialY; // The initial Y position of the Tengu

        private System.Windows.Forms.Timer _animationTimer;

        public Tengu(int health, int movementSpeed, Point startPosition)
            : base(health, movementSpeed, startPosition)
        {
            _animations = new Dictionary<string, (Image spriteSheet, int frameCount)>();
            _initialY = startPosition.Y;

            LoadAnimation("fly", Resources.Tengu_fly, 15);
            LoadAnimation("death", Resources.Tengu_death, 6);

            _currentAnimation = "fly";
            _currentFrame = 0;
            _isFacingLeft = false;

            _tenguPictureBox = new PictureBox
            {
                Size = new Size(TenguWidth, TenguHeight),
                Location = startPosition,
                BackColor = Color.Transparent
            };

            _animationTimer = new System.Windows.Forms.Timer { Interval = AnimationInterval };
            _animationTimer.Tick += (sender, e) => AnimateHover();
            _animationTimer.Start();

            UpdateSprite();
        }

        public void AnimateHover()
        {
            _currentFrame = (_currentFrame + 1) % _animations[_currentAnimation].frameCount;
            UpdateSprite();

            // Hover up and down
            if (_hoveringUp)
            {
                _tenguPictureBox.Top -= _hoverStep;
                if(_tenguPictureBox.Top <= _initialY - _hoverRange)
                {
                    _hoveringUp = false; // Reverse direction when reaching the top
                }
            }
            else
            {
                _tenguPictureBox.Top += _hoverStep;
                if (_tenguPictureBox.Top >= _initialY + _hoverRange)
                {
                    _hoveringUp = true; // Reverse direction when reaching the bottom
                }
            }

        }

        protected override void Move()
        {

        }

        public PictureBox GetPictureBox() => _tenguPictureBox;

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
            Image oldImage = _tenguPictureBox.Image;
            _tenguPictureBox.Image = currentFrameImage;

            // Dispose of the old image to free memory
            oldImage?.Dispose();
        }
        
    }
}
