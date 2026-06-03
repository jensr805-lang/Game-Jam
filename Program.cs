using System.Drawing;
using System.Windows.Forms;

ApplicationConfiguration.Initialize();
Application.Run(new MainMenuForm());

public class MainMenuForm : Form
{
    private readonly Button easyButton;
    private readonly Button mediumButton;
    private readonly Button hardButton;
    private readonly Label titleLabel;
    private readonly Label instructionLabel;

    public MainMenuForm()
    {
        Text = "Wall Destroyer";
        DoubleBuffered = true;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Black;
        ClientSize = new Size(20 * 28, 15 * 28 + 100);

        titleLabel = new Label
        {
            Text = "Wall Destroyer",
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Font = new Font("Consolas", 24, FontStyle.Bold),
            AutoSize = true,
            Location = new Point((ClientSize.Width - 340) / 2, 30)
        };

        instructionLabel = new Label
        {
            Text = "Select difficulty to start",
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Font = new Font("Consolas", 12, FontStyle.Regular),
            AutoSize = true,
            Location = new Point((ClientSize.Width - 240) / 2, 90)
        };

        easyButton = CreateMenuButton("Easy", new Point(40, 160));
        mediumButton = CreateMenuButton("Medium", new Point(160, 160));
        hardButton = CreateMenuButton("Hard", new Point(280, 160));

        Controls.Add(titleLabel);
        Controls.Add(instructionLabel);
        Controls.Add(easyButton);
        Controls.Add(mediumButton);
        Controls.Add(hardButton);
    }

    private Button CreateMenuButton(string text, Point location)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(100, 40),
            Location = location,
            Font = new Font("Consolas", 10, FontStyle.Bold)
        };
        button.Click += OnDifficultyButtonClick;
        return button;
    }

    private void OnDifficultyButtonClick(object? sender, EventArgs e)
    {
        int speed = sender == easyButton ? 16 : sender == mediumButton ? 12 : 8;
        var gameForm = new GameForm(speed);
        gameForm.FormClosed += (s, args) => Show();
        Hide();
        gameForm.Show();
    }
}

public class GameForm : Form
{
    private const int WidthCells = 20;
    private const int HeightCells = 15;
    private const int CellSize = 28;
    private const int DefaultDropSpeed = 12;

    private readonly List<Point> walls = new();
    private readonly List<Point> bullets = new();
    private readonly System.Windows.Forms.Timer gameTimer;

    private int playerX;
    private int playerY;
    private int score;
    private int lives;
    private int dropCounter;
    private int dropSpeed;

    public GameForm(int initialDropSpeed)
    {
        Text = "Wall Destroyer";
        DoubleBuffered = true;
        KeyPreview = true;
        BackColor = Color.Black;
        ClientSize = new Size(WidthCells * CellSize, HeightCells * CellSize + 60);
        StartPosition = FormStartPosition.CenterScreen;

        playerX = WidthCells / 2;
        playerY = HeightCells - 1;
        score = 0;
        lives = 3;
        dropCounter = 0;
        dropSpeed = initialDropSpeed;

        InitializeWalls();

        gameTimer = new System.Windows.Forms.Timer();
        gameTimer.Interval = 80;
        gameTimer.Tick += OnGameTick;
        gameTimer.Start();

        KeyDown += OnKeyDown;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        DrawGame(e.Graphics);
    }

    private void OnGameTick(object? sender, EventArgs e)
    {
        UpdateBullets();
        dropCounter++;
        if (dropCounter >= dropSpeed)
        {
            dropCounter = 0;
            MoveWallsDown();
        }

        if (walls.Count == 0)
        {
            ShowEndMessage("You win!");
            return;
        }

        if (lives <= 0)
        {
            ShowEndMessage("Game over!");
            return;
        }

        Invalidate();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:
            case Keys.A:
                playerX = Math.Max(0, playerX - 1);
                break;
            case Keys.Right:
            case Keys.D:
                playerX = Math.Min(WidthCells - 1, playerX + 1);
                break;
            case Keys.Space:
                bullets.Add(new Point(playerX, playerY - 1));
                break;
            case Keys.Q:
                Close();
                break;
        }

        Invalidate();
    }

    private void UpdateBullets()
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            int newY = bullet.Y - 1;
            if (newY < 0)
            {
                bullets.RemoveAt(i);
                continue;
            }

            int hitIndex = walls.FindIndex(w => w.X == bullet.X && w.Y == newY);
            if (hitIndex >= 0)
            {
                walls.RemoveAt(hitIndex);
                bullets.RemoveAt(i);
                score += 10;
                continue;
            }

            bullets[i] = new Point(bullet.X, newY);
        }
    }

    private void MoveWallsDown()
    {
        for (int i = 0; i < walls.Count; i++)
        {
            walls[i] = new Point(walls[i].X, walls[i].Y + 1);
        }

        for (int i = walls.Count - 1; i >= 0; i--)
        {
            if (walls[i].Y >= playerY)
            {
                walls.RemoveAt(i);
                lives--;
            }
        }
    }

    private void DrawGame(Graphics g)
    {
        g.Clear(Color.Black);

        using var wallBrush = new SolidBrush(Color.LightGray);
        using var playerBrush = new SolidBrush(Color.Cyan);
        using var bulletBrush = new SolidBrush(Color.Red);
        using var gridPen = new Pen(Color.DimGray);

        for (int y = 0; y < HeightCells; y++)
        {
            for (int x = 0; x < WidthCells; x++)
            {
                Rectangle cellRect = new(x * CellSize, y * CellSize, CellSize, CellSize);
                g.DrawRectangle(gridPen, cellRect);
            }
        }

        foreach (var wall in walls)
        {
            Rectangle cellRect = new(wall.X * CellSize + 1, wall.Y * CellSize + 1, CellSize - 2, CellSize - 2);
            g.FillRectangle(wallBrush, cellRect);
        }

        foreach (var bullet in bullets)
        {
            Rectangle bulletRect = new(bullet.X * CellSize + CellSize / 3, bullet.Y * CellSize + 2, CellSize / 3, CellSize / 2);
            g.FillRectangle(bulletBrush, bulletRect);
        }

        Point playerTop = new(playerX * CellSize + CellSize / 2, playerY * CellSize + 4);
        Point playerLeft = new(playerX * CellSize + 4, playerY * CellSize + CellSize - 4);
        Point playerRight = new(playerX * CellSize + CellSize - 4, playerY * CellSize + CellSize - 4);
        g.FillPolygon(playerBrush, new[] { playerTop, playerLeft, playerRight });

        using var textBrush = new SolidBrush(Color.White);
        g.DrawString($"Score: {score}   Lives: {lives}", new Font("Consolas", 12), textBrush, 8, HeightCells * CellSize + 6);
        g.DrawString("Use A/D or arrows to move, Space to shoot, Q to quit", new Font("Consolas", 10), textBrush, 8, HeightCells * CellSize + 26);
    }

    private void InitializeWalls()
    {
        walls.Clear();
        for (int y = 1; y <= 4; y++)
        {
            for (int x = 2; x < WidthCells - 2; x += 2)
            {
                walls.Add(new Point(x, y));
            }
        }
    }

    private void ShowEndMessage(string title)
    {
        gameTimer.Stop();
        var result = MessageBox.Show(this, $"{title}\r\nFinal Score: {score}\r\nPlay again?", "Wall Destroyer", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (result == DialogResult.Yes)
        {
            ResetGame();
            gameTimer.Start();
        }
        else
        {
            Close();
        }
    }

    private void ResetGame()
    {
        score = 0;
        lives = 3;
        bullets.Clear();
        playerX = WidthCells / 2;
        playerY = HeightCells - 1;
        dropCounter = 0;
        InitializeWalls();
        Invalidate();
    }
}
