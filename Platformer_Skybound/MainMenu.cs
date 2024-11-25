using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace Platformer_Skybound
{
    public class MainMenu : Form
    {
        private Button startGameButton;

        public MainMenu()
        {
            InitializeForm();
            InitializeControls();
        }

        private void InitializeForm()
        {
            this.Text = "Main Menu";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeControls()
        {
            startGameButton = new Button
            {
                Text = "Start Game",
                Location = new Point(150, 50),
                Size = new Size(100, 30)
            };
            startGameButton.Click += StartGameButton_Click;
            this.Controls.Add(startGameButton);
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            MorningLevel morningLevel = new MorningLevel();
            morningLevel.FormClosed += (s, args) => this.Show();
            morningLevel.Show();
            this.Hide();
        }
    }
}
