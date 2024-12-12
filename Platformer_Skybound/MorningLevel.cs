using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace Platformer_Skybound
{
    public class MorningLevel : Form
    {
        private int PlayerInitialPositionX;
        private const int PlayerInitialPositionY = 340;
        public const int GroundLevel = 400;

        private const int TenguInitialPositionX = 2000;
        private const int TenguInitialPositionY = 100;

        private Player _player;
        private Tengu _tengu;
        private System.Windows.Forms.Timer _movementTimer;
        private HashSet<Keys> _pressedKeys; //Set to track currently pressed keys (works for simultaneous presses)

        public HashSet<Keys> PressedKeys { get; private set; }

        private bool _inHorizontalConflict = false; //State tracker used for when both A and D keys are pressed together

        //For pause menu
        private Panel _pauseMenu;
        private Button _backToMainMenuButton;
        private bool _isPaused = false;

        //For current view panel
        private Panel _levelPanel;
        private PictureBox _backgroundPictureBox;
        public const int LevelWidth = 8000;
        private const int ScrollOffset = 400;

        // For Tengu
        private PictureBox _tenguPictureBox;

        private Image _morningClouds;

        private bool _isPlayerCentered = false;

        public MorningLevel()
        {
            _morningClouds = ByteArrayToImage(Resources.clouds_morning);

            InitializeLevel();

            InitializeBackground();
        }

        private Image ByteArrayToImage(byte[] byteArray)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArray))
                {
                    Image image = Image.FromStream(ms);
                    return image;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error converting byte array to Image: " + ex.Message);
                return null;
            }
            
        }

        private void InitializeLevel()
        {
            this.Text = "Level 1";
            this.Size = new Size(800, 600);
            this.BackColor = Color.LightSkyBlue;

            _pressedKeys = new HashSet<Keys>();

            // Creates the level layout
            _levelPanel = new Panel
            {
                Size = new Size(LevelWidth, 600),
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };
            this.Controls.Add(_levelPanel);

            // Membuat lapisan tanah
            Image grassGround = ByteArrayToImage(Resources.Tanah); // Gambar tanah berumput
            Image brownGround = ByteArrayToImage(Resources.TanahCoklat); // Gambar tanah coklat penuh

            int groundHeight = 200; // Total tinggi ground
            Image doubleLayerGround = CreateDoubleLayerGround(grassGround, brownGround, LevelWidth, groundHeight);

            PictureBox groundPictureBox = new PictureBox
            {
                Image = doubleLayerGround,
                Size = new Size(LevelWidth, groundHeight),
                Location = new Point(0, GroundLevel),
                BackColor = Color.Transparent
            };
            _levelPanel.Controls.Add(groundPictureBox);

            PlayerInitialPositionX = (this.ClientSize.Width / 2) - 20;
            _player = new Player(new Point(PlayerInitialPositionX, PlayerInitialPositionY), LevelWidth);
            this.Controls.Add(_player.GetPictureBox());
            _player.GetPictureBox().BringToFront();

            _tengu = new Tengu(3, 5, new Point(TenguInitialPositionX, TenguInitialPositionY));
            _tenguPictureBox = _tengu.GetPictureBox();
            _tenguPictureBox.Parent = _levelPanel; // Set the parent to the scrolling panel
            _tenguPictureBox.BringToFront();
            _levelPanel.Controls.Add(_tenguPictureBox);
            _tenguPictureBox.Visible = false; // Initially hidden

            InitializePauseMenu();

            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;

            _movementTimer = new System.Windows.Forms.Timer
            {
                Interval = 16 // Approx 60 FPS (16ms per frame)
            };
            _movementTimer.Tick += OnMovementTick;
            _movementTimer.Start();

            
        }

        private Image CreateDoubleLayerGround(Image grassGround, Image brownGround, int levelWidth, int groundHeight)
        {
            int grassHeight = grassGround.Height; // Tinggi tanah berumput
            int brownHeight = groundHeight - grassHeight; // Sisa tinggi untuk tanah coklat penuh

            Bitmap groundBitmap = new Bitmap(levelWidth, groundHeight);

            using (Graphics g = Graphics.FromImage(groundBitmap))
            {
                // Gambar tanah coklat penuh di bawah
                for (int x = 0; x < levelWidth; x += brownGround.Width)
                {
                    g.DrawImage(brownGround, x, grassHeight, brownGround.Width, brownHeight);
                }

                // Gambar tanah berumput di atas
                for (int x = 0; x < levelWidth; x += grassGround.Width)
                {
                    g.DrawImage(grassGround, x, 0, grassGround.Width, grassHeight);
                }
            }

            return groundBitmap;
        }



        private void InitializeBackground()
        {
            _backgroundPictureBox = new PictureBox
            {
                Image = _morningClouds,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(LevelWidth, 600),
                Location = new Point(0, 0)
            };
            Image backgroundWithCliffs = AddCliffsToBackground(_morningClouds, LevelWidth, this.ClientSize.Height);

            _backgroundPictureBox = new PictureBox
            {
                Image = backgroundWithCliffs,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(LevelWidth, this.ClientSize.Height),
                Location = new Point(0, 0)
            };
            _levelPanel.Controls.Add(_backgroundPictureBox);
            _backgroundPictureBox.SendToBack();
        }

        private Image AddCliffsToBackground(Image background, int levelWidth, int screenHeight)
        {
            Bitmap bitmapWithCliffs = new Bitmap(levelWidth, screenHeight);

            using (Graphics g = Graphics.FromImage(bitmapWithCliffs))
            {
                // Skala ulang latar belakang ke ukuran level
                g.DrawImage(background, 0, 0, levelWidth, screenHeight);

                // Muat gambar kecil (tile) untuk tebing dari Resources
                Image cliffTile = ByteArrayToImage(Resources.TanahCoklat);

                int tileWidth = 370;
                int tileHeight = 370;

                // Gambar tebing kiri (vertikal)
                for (int y = 0; y < screenHeight; y += tileHeight)
                {
                    g.DrawImage(cliffTile, 0, y, tileWidth, tileHeight);
                }

                // Gambar tebing kanan (vertikal)
                for (int y = 0; y < screenHeight; y += tileHeight)
                {
                    g.DrawImage(cliffTile, levelWidth - tileWidth, y, tileWidth, tileHeight);
                }
            }

            return bitmapWithCliffs;
        }

        private void OnPlayerMoved(int newPlayerX)
        {
            ScrollLevel();
        }
        private void InitializePauseMenu()
        {
            _pauseMenu = new Panel
            {
                Size = new Size(400, 300),
                BackColor = Color.FromArgb(200, 0, 0, 0), // Semi-transparent dark background
                Location = new Point((this.ClientSize.Width - 400) / 2, (this.ClientSize.Height - 300) / 2),
                Visible = false
            };
            this.Controls.Add(_pauseMenu);

            _backToMainMenuButton = new Button
            {
                Text = "Return to Main Menu",
                Size = new Size(200, 50),
                Location = new Point(100, 120),
                ForeColor = Color.White
            };
            _backToMainMenuButton.Click += BackToMainMenuButton_Click;
            _pauseMenu.Controls.Add(_backToMainMenuButton);
        }

        private void BackToMainMenuButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TogglePauseMenu()
        {
            _isPaused = !_isPaused;
            _pauseMenu.Visible = _isPaused;
            _pauseMenu.BringToFront();
            _pauseMenu.Refresh();

            if (_isPaused)
            {
                _movementTimer.Stop();
                _player._physicsTimer.Stop();
            }
            else
            {
                _movementTimer.Start();
                _player._physicsTimer.Start();
            }
        }

        private void OnMovementTick(object sender, EventArgs e)
        {
            if (_isPaused) return;

            if(_pressedKeys.Contains(Keys.A) && _pressedKeys.Contains(Keys.D))
            {
                _inHorizontalConflict = true;
            }
            else
            {
                _inHorizontalConflict = false;
            }

            if (_inHorizontalConflict)
            {
                _player.StopMovement();
            }
            else
            {
                if (_pressedKeys.Contains(Keys.A))
                {
                    _player.HandleKeyDown(Keys.Left, this.ClientSize);
                    ScrollLevel();
                }
                else if (_pressedKeys.Contains(Keys.D))
                {
                    _player.HandleKeyDown(Keys.Right, this.ClientSize);
                    ScrollLevel();
                }
            }

            if (_pressedKeys.Contains(Keys.Space))
            {
                _player.HandleKeyDown(Keys.Up, this.ClientSize);
            }
        }


        private void ScrollLevel()
        {
            int playerX = _player.GetPlayerXPosition();
            int screenCenterX = this.ClientSize.Width / 2 - (_player.GetPictureBox().Width / 2);

            // Kecepatan scrolling latar belakang
            double backgroundSpeedFactor = 5;

            if (playerX < 0)
            {
                _player.SetPositionX(0);
                return;
            }

            else if(playerX > LevelWidth - _player.GetPictureBox().Width)
            {
                _player.SetPositionX(LevelWidth - _player.GetPictureBox().Width);
                return;
            }


            // Jika pemain bergerak ke kanan
            if (_pressedKeys.Contains(Keys.D))
            {
                if (_backgroundPictureBox.Left > -(LevelWidth - this.ClientSize.Width))
                {
                    // Move background to the left
                    _backgroundPictureBox.Left -= (int)(_player.GetSpeed() * backgroundSpeedFactor);

                    // Keep player in the center of the screen
                    _player.SetPositionX(screenCenterX);
                }
            }

            // Jika pemain bergerak ke kiri
            else if (_pressedKeys.Contains(Keys.A))
            {
                if (_backgroundPictureBox.Left < 0)
                {
                    // Move background to the right
                    _backgroundPictureBox.Left += (int)(_player.GetSpeed() * backgroundSpeedFactor);

                    // Keep player in the center of the screen
                    _player.SetPositionX(screenCenterX);
                }
            }

            // Pastikan latar belakang tidak keluar batas
            if (_backgroundPictureBox.Left > 0)
            {
                _backgroundPictureBox.Left = 0;
            }
            else if (_backgroundPictureBox.Left < -(LevelWidth - this.ClientSize.Width))
            {
                _backgroundPictureBox.Left = -(LevelWidth - this.ClientSize.Width);
            }

            // Pastikan pemain tetap berada dalam area yang terlihat
            if (_player.GetPlayerXPosition() < 0)
            {
                _player.SetPositionX(0);
            }
            else if (_player.GetPlayerXPosition() > LevelWidth - _player.GetPictureBox().Width)
            {
                _player.SetPositionX(LevelWidth - _player.GetPictureBox().Width);
            }

            UpdateTenguVisibilityAndPosition();
        }

        private void UpdateTenguVisibilityAndPosition()
        {
            int tenguWorldX = TenguInitialPositionX;
            int tenguScreenX = tenguWorldX + _backgroundPictureBox.Left;

            if (tenguScreenX + _tenguPictureBox.Width > 0 && tenguScreenX < this.ClientSize.Width)
            {
                _tenguPictureBox.Visible = true;
                _tenguPictureBox.Left = tenguScreenX;
                _tenguPictureBox.Top = TenguInitialPositionY;
            }
            else
            {
                _tenguPictureBox.Visible = false;
            }
        }





        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _pressedKeys.Remove(e.KeyCode);

            if (_pressedKeys.Count == 0)
            {
                _player.StopMovement();
            }
        }




        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                TogglePauseMenu();
                return;
            }

            _pressedKeys.Add(e.KeyCode);
        }
    }
}
