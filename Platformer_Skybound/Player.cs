﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Security.Policy;

namespace Platformer_Skybound
{
    public class Player
    {
        private const int PlayerWidth = 40;
        private const int PlayerHeight = 45;
        private const int JumpSpeed = -16;
        private const int Gravity = 1;
        public int Health { get; private set; }

        const int GroundLevel = MorningLevel.GroundLevel;

        private PictureBox _playerPictureBox;
        private Dictionary<string, (Image spriteSheet, int frameCount)> _animations;

        private int AnimationInterval = 125;
        public int speed = 5;
        private string _currentAnimation;
        private int _currentFrame;
        private bool _isMoving;
        private bool _isFacingLeft;
        private bool _isFalling;
        private bool _isJumping;
        private int _verticalVelocity;

        private System.Windows.Forms.Timer _animationTimer;
        public System.Windows.Forms.Timer _physicsTimer;
        int LevelWidth = MorningLevel.LevelWidth;
        public event Action<int> PlayerMoved;

        // For dealing damage
        private const int AttackDuration = 750;
        private System.Windows.Forms.Timer _attackTimer;
        public bool IsAttacking { get; private set; }
        public System.Windows.Forms.Timer AttackTimer { get; private set; }

        public Player(int health, Point startPosition, int levelWidth)
        {
            Health = health;
            LevelWidth = levelWidth;
            _animations = new Dictionary<string, (Image spriteSheet, int frameCount)>();
            IsAttacking = false;

            LoadAnimation("idle", Resources.player_idle, 4);
            LoadAnimation("run", Resources.player_run, 6);
            LoadAnimation("jump", Resources.player_jump, 8);
            LoadAnimation("attack", Resources.player_attack, 6);

            _currentAnimation = "idle";
            _currentFrame = 0;
            _isFacingLeft = false;
            _isJumping = false;
            _isFalling = false;
            _verticalVelocity = 0;

            _playerPictureBox = new PictureBox
            {
                Size = new Size(PlayerWidth, PlayerHeight),
                Location = startPosition,
                BackColor = Color.Transparent
            };

            _animationTimer = new System.Windows.Forms.Timer { Interval = AnimationInterval };
            _animationTimer.Tick += (sender, e) => Animate();
            _animationTimer.Start();

            _physicsTimer = new System.Windows.Forms.Timer { Interval = 16 }; // Smaller interval for physics
            _physicsTimer.Tick += (sender, e) => HandleJumpAndFall();
            _physicsTimer.Start();

            // Timer for attacking
            _attackTimer = new System.Windows.Forms.Timer
            {
                Interval = AttackDuration
            };
            _attackTimer.Tick += (sender, args) => IsAttacking = false; // Reset attack
            _attackTimer.Stop();

            UpdateSprite();
        }

        public PictureBox GetPictureBox() => _playerPictureBox;

        private void LoadAnimation(string animationName, byte[] spriteData, int frameCount)
        {
            using (MemoryStream ms = new MemoryStream(spriteData))
            {
                _animations[animationName] = (Image.FromStream(ms), frameCount);
            }
        }

        public int GetPlayerXPosition() => _playerPictureBox.Left;

        public void HandleKeyDown(Keys key)
        {
            switch (key)
            {
                case Keys.Left:
                    if (!_isJumping && !_isFalling)
                    {
                        _currentAnimation = "run";
                    }
                    _isFacingLeft = true;
                    break;

                case Keys.Right:
                    if (!_isJumping && !_isFalling)
                    {
                        _currentAnimation = "run";
                    }
                    _isFacingLeft = false;
                    break;

                case Keys.Up:
                    if (!_isJumping && !_isFalling)
                    {
                        StartJump();
                    }
                    break;

                default:
                    break;
            }

            UpdateSprite(); // Memperbarui animasi setelah setiap langkah
        }

        public void Attack()
        {
            if (IsAttacking) return; // Prevent starting a new attack if one is ongoing

            IsAttacking = true;
            _currentAnimation = "attack";
            _currentFrame = 0; // Reset animation frame for attack
            UpdateSprite();
            _attackTimer.Start();
        }

        public void SetPositionX(int x)
        {
            _playerPictureBox.Left = x;
        }
        public int GetSpeed()
        {
            return speed;
        }



        public void StopMovement()
        {
            if (!_isJumping && !_isFalling)
            {
                _currentAnimation = "idle";
                _isMoving = false;
            }
            UpdateSprite();
        }

        private void StartJump()
        {
            _isJumping = true;
            _verticalVelocity = JumpSpeed;
            _currentAnimation = "jump";
            UpdateSprite();
        }

        public void Animate()
        {
            if (_isJumping || _isFalling)
            {
                _animationTimer.Interval = 100;
                _currentAnimation = "jump";
            }
            else
            {
                _animationTimer.Interval = 125;  // Regular animation speed when on the ground

                if (IsAttacking && _currentAnimation != "attack")
                {
                    _currentAnimation = "attack";
                }
                else if (!_isMoving && _currentAnimation != "idle" && !IsAttacking)
                {
                    _currentAnimation = "idle";
                }
                else if (_isMoving && _currentAnimation != "run" && !IsAttacking)
                {
                    _currentAnimation = "run";
                }
            }

            _currentFrame = (_currentFrame + 1) % _animations[_currentAnimation].frameCount;
            UpdateSprite();
        }

        private void HandleJumpAndFall()
        {
            if (_isJumping || _isFalling)
            {
                _playerPictureBox.Top += _verticalVelocity;
                _verticalVelocity += Gravity;

                if (_playerPictureBox.Bottom >= GroundLevel)
                {
                    _playerPictureBox.Top = GroundLevel - PlayerHeight;
                    _isJumping = false;
                    _isFalling = false;
                    _verticalVelocity = 0;
                }
                else if (_verticalVelocity > 0) //Starts falling once player starts to move downwards
                {
                    _isJumping = false;
                    _isFalling = true;
                }
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
            Image oldImage = _playerPictureBox.Image;
            _playerPictureBox.Image = currentFrameImage;

            // Dispose of the old image to free memory
            oldImage?.Dispose();
        }
        public void SetPositionY(int y)
        {
            _playerPictureBox.Top = y;
        }

        private System.Windows.Forms.Timer _damageBlinkTimer;
        private int _blinkCount;

        public void TakeDamage()
        {
            Health -= 1;

            // Setup for blinking effect
            _blinkCount = 0;

            if (_damageBlinkTimer == null)
            {
                _damageBlinkTimer = new System.Windows.Forms.Timer
                {
                    Interval = 150 // Time between blinks in milliseconds
                };
                _damageBlinkTimer.Tick += (sender, args) =>
                {
                    // Toggle visibility for blinking
                    _playerPictureBox.Visible = !_playerPictureBox.Visible;
                    _blinkCount++;

                    // Stop the timer after a few blinks
                    if (_blinkCount >= 10) // Adjust for 3 full on/off cycles
                    {
                        _damageBlinkTimer.Stop();
                        _playerPictureBox.Visible = true; // Ensure visibility is restored
                    }
                };
            }

            _damageBlinkTimer.Start();
        }

    }
}
