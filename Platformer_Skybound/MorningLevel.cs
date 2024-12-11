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

        private Player _player;
        private System.Windows.Forms.Timer _movementTimer;
        private HashSet<Keys> _pressedKeys; //Set to track currently pressed keys (works for simultaneous presses)

        private bool _inHorizontalConflict = false; //State tracker used for when both A and D keys are pressed together

        //For pause menu
        private Panel _pauseMenu;
        private Button _backToMainMenuButton;
        private bool _isPaused = false;

        //For current view panel
        private Panel _levelPanel;
        private PictureBox _backgroundPictureBox;
        public const int LevelWidth = 4000;
        private const int ScrollOffset = 400;

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

            // Set the center position
            PlayerInitialPositionX = (this.ClientSize.Width / 2) - 20;

            _player = new Player(new Point(PlayerInitialPositionX, PlayerInitialPositionY), LevelWidth);
            this.Controls.Add(_player.GetPictureBox());

            _levelPanel = new Panel
            {
                Size = new Size(LevelWidth, 600),
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };
            this.Controls.Add(_levelPanel);

            InitializePauseMenu();

            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;

            _movementTimer = new System.Windows.Forms.Timer();
            _movementTimer.Interval = 16; // Approx 60 FPS (16ms per frame)
            _movementTimer.Tick += OnMovementTick;
            _movementTimer.Start();
            PictureBox ground = new PictureBox
            {
                Image = ByteArrayToImage(Resources.Tanah), // Ganti dengan nama resource ground Anda
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(LevelWidth, 200), // Ukuran ground memanjang sepanjang level
                Location = new Point(0, 400),   // Lokasi ground (di bawah posisi pemain)
                BackColor = Color.Transparent // Pastikan transparan jika tidak ingin warna solid
            };
            _levelPanel.Controls.Add(ground);
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

                // Muat gambar tebing dari Resources
                Image leftCliffImage = ByteArrayToImage(Resources.Tanah);
                Image rightCliffImage = ByteArrayToImage(Resources.Tanah);

                // Gambar tebing kiri
                g.DrawImage(leftCliffImage, 0, 0, 350, screenHeight);

                // Gambar tebing kanan
                g.DrawImage(rightCliffImage, 7600, 0, 350, screenHeight);
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
            double backgroundSpeedFactor = 2;

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
