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
        private int _movementRange = 200;
        public int MovementSpeed { get; private set; }
        private int _initialX;
        public int WorldX { get; private set; }

        private System.Windows.Forms.Timer _animationTimer;

        public Werewolf(int health, int movementSpeed, Point startPosition)
            : base(health, movementSpeed, startPosition)
        {
            _animations = new Dictionary<string, (Image spriteSheet, int frameCount)>();
            MovementSpeed = movementSpeed;

            _initialX = startPosition.X;
            WorldX = startPosition.X;

            // Memuat animasi gerakan ke kanan dan kiri
            LoadAnimation("run_right", Resources.werewolf_run_right, 9); // Animasi gerakan ke kanan
            LoadAnimation("run_left", Resources.werewolf_run_left, 9);  // Animasi gerakan ke kiri

            _currentAnimation = "run_right"; // Default animasi ketika bergerak ke kanan
            _currentFrame = 0;
            _isFacingLeft = false;

            _werewolfPictureBox = new PictureBox
            {
                Size = new Size(WerewolfWidth, WerewolfHeight),
                Location = startPosition,
                BackColor = Color.Transparent
            };

            _animationTimer = new System.Windows.Forms.Timer { Interval = AnimationInterval };
            _animationTimer.Tick += (sender, e) =>
            {
                Animate();
            };
            _animationTimer.Start();

            UpdateSprite();
        }

        public override void Move()
        {
            if (_isFacingLeft)
            {
                WorldX -= MovementSpeed;

                // Reverse direction if reaching the left bound
                if (WorldX <= _initialX - _movementRange)
                {
                    _isFacingLeft = false; // Start moving right
                    SetAnimation(); // Update animasi ketika arah berubah
                }
            }
            else
            {
                WorldX += MovementSpeed;

                if (WorldX >= _initialX + _movementRange)
                {
                    _isFacingLeft = true; // Start moving left
                    SetAnimation(); // Update animasi ketika arah berubah
                }
            }

            _werewolfPictureBox.Left = WorldX;
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
                var spriteSheet = Image.FromStream(ms);
                _animations[animationName] = (spriteSheet, frameCount);
            }
        }

        private void UpdateSprite()
        {
            var (spriteSheet, frameCount) = _animations[_currentAnimation];
            int frameWidth = spriteSheet.Width / frameCount;
            int frameHeight = spriteSheet.Height;

            // Hitung posisi potongan untuk frame saat ini
            Rectangle srcRect = new Rectangle(_currentFrame * frameWidth, 0, frameWidth, frameHeight);

            // Buat gambar frame saat ini
            Bitmap currentFrameImage = new Bitmap(frameWidth, frameHeight);
            using (Graphics g = Graphics.FromImage(currentFrameImage))
            {
                g.DrawImage(spriteSheet, new Rectangle(0, 0, frameWidth, frameHeight), srcRect, GraphicsUnit.Pixel);
            }

            // Ganti gambar lama dengan yang baru
            Image oldImage = _werewolfPictureBox.Image;
            _werewolfPictureBox.Image = currentFrameImage;
            oldImage?.Dispose();
        }

        // Fungsi untuk mengganti animasi sesuai arah
        private void SetAnimation()
        {
            // Cek apakah menghadap kiri atau kanan
            if (_isFacingLeft)
            {
                _currentAnimation = "run_left"; // Gunakan animasi saat bergerak ke kiri
            }
            else
            {
                _currentAnimation = "run_right"; // Gunakan animasi saat bergerak ke kanan
            }
        }
    }
}
