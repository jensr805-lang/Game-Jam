using System.Drawing;
using System.Windows.Forms;

ApplicationConfiguration.Initialize();
Application.Run(new GameForm());

public class GameForm : Form
{
    private const int WidthCells = 20;
    private const int HeightCells = 15;
    private const int CellSize = 28;
    private const int BorderSize = 16;
    private const int DropSpeed = 12;

    private record struct Wall(Point Location, bool IsExplosive);

    private readonly List<Wall> walls = new();
    private readonly List<Point> bullets = new();
    private readonly System.Windows.Forms.Timer gameTimer;
    private readonly Random random = new();

    private int playerX;
    private int playerY;
    private int score;
    private int lives;
    private int dropCounter;

    public GameForm()
    {
        Text = "Wall Destroyer";
        DoubleBuffered = true;
        KeyPreview = true;
        BackColor = Color.Black;

        playerX = WidthCells / 2;
        playerY = HeightCells - 1;
        score = 0;
        lives = 3;
        dropCounter = 0;

        ClientSize = new Size(WidthCells * CellSize, HeightCells * CellSize + 60);
        StartPosition = FormStartPosition.CenterScreen;

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
        if (dropCounter >= DropSpeed)
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

            int hitIndex = walls.FindIndex(w => w.Location.X == bullet.X && w.Location.Y == newY);
            if (hitIndex >= 0)
            {
                var hitWall = walls[hitIndex];
                if (hitWall.IsExplosive)
                {
                    int destroyed = RemoveWallsNear(hitWall.Location);
                    score += destroyed * 10;
                }
                else
                {
                    walls.RemoveAt(hitIndex);
                    score += 10;
                }

                bullets.RemoveAt(i);
                continue;
            }

            bullets[i] = new Point(bullet.X, newY);
        }
    }

    private int RemoveWallsNear(Point center)
    {
        return walls.RemoveAll(w => Math.Abs(w.Location.X - center.X) <= 1 && Math.Abs(w.Location.Y - center.Y) <= 1);
    }

    private void MoveWallsDown()
    {
        for (int i = 0; i < walls.Count; i++)
        {
            var wall = walls[i];
            walls[i] = new Wall(new Point(wall.Location.X, wall.Location.Y + 1), wall.IsExplosive);
        }

        for (int i = walls.Count - 1; i >= 0; i--)
        {
            if (walls[i].Location.Y >= playerY)
            {
                walls.RemoveAt(i);
                lives--;
            }
        }
    }

    private void DrawGame(Graphics g)
    {
        g.Clear(Color.Black);

        using var normalWallBrush = new SolidBrush(Color.LightGray);
        using var explosiveWallBrush = new SolidBrush(Color.OrangeRed);
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
            Rectangle cellRect = new(wall.Location.X * CellSize + 1, wall.Location.Y * CellSize + 1, CellSize - 2, CellSize - 2);
            g.FillRectangle(wall.IsExplosive ? explosiveWallBrush : normalWallBrush, cellRect);
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
                bool isExplosive = random.Next(0, 5) == 0;
                walls.Add(new Wall(new Point(x, y), isExplosive));
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
