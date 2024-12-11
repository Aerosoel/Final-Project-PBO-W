using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer_Skybound
{
    public class Tengu : Monster, IFlying
    {
        private const int TenguWidth = 40;
        private const int TenguHeight = 60;

        private PictureBox _tenguPictureBox;
        private Dictionary<string, (Image spriteSheet, int frameCount)> _animations;

        int LevelWidth = MorningLevel.LevelWidth;

        public Tengu(int health, int movementSpeed, Point startPosition)
            : base(health, movementSpeed, startPosition)
        {

        }

        public void AnimateHover()
        {

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
    }
}
