using System;
using System.Collections.Generic;

const int Width = 20;
const int Height = 15;

char[,] screen = new char[Height, Width];
int playerX = Width / 2;
int playerY = Height - 1;
int score = 0;
int lives = 3;

var walls = new List<(int x, int y)>();
var bullets = new List<(int x, int y)>();

InitializeWalls();

Console.CursorVisible = false;
int dropCounter = 0;
const int DropSpeed = 12;

while (true)
{
    HandleInput();
    UpdateBullets();

    dropCounter++;
    if (dropCounter >= DropSpeed)
    {
        dropCounter = 0;
        MoveWallsDown();
    }

    Draw();

    if (walls.Count == 0)
    {
        ShowMessage("You win! Press any key to play again.");
        dropCounter = 0;
        Reset();
        continue;
    }

    if (lives <= 0)
    {
        ShowMessage("Game over! Press any key to play again.");
        dropCounter = 0;
        Reset();
        continue;
    }

    System.Threading.Thread.Sleep(80);
}

void HandleInput()
{
    while (Console.KeyAvailable)
    {
        var key = Console.ReadKey(true).Key;
        switch (key)
        {
            case ConsoleKey.LeftArrow:
            case ConsoleKey.A:
                playerX = Math.Max(0, playerX - 1);
                break;
            case ConsoleKey.RightArrow:
            case ConsoleKey.D:
                playerX = Math.Min(Width - 1, playerX + 1);
                break;
            case ConsoleKey.Spacebar:
                bullets.Add((playerX, playerY - 1));
                break;
            case ConsoleKey.Q:
                Environment.Exit(0);
                break;
        }
    }
}

void UpdateBullets()
{
    for (int i = bullets.Count - 1; i >= 0; i--)
    {
        var bullet = bullets[i];
        int newY = bullet.y - 1;
        if (newY < 0)
        {
            bullets.RemoveAt(i);
            continue;
        }

        var hitWall = walls.FindIndex(w => w.x == bullet.x && w.y == newY);
        if (hitWall >= 0)
        {
            walls.RemoveAt(hitWall);
            bullets.RemoveAt(i);
            score += 10;
            continue;
        }

        bullets[i] = (bullet.x, newY);
    }
}

void MoveWallsDown()
{
    for (int i = 0; i < walls.Count; i++)
    {
        walls[i] = (walls[i].x, walls[i].y + 1);
    }

    for (int i = walls.Count - 1; i >= 0; i--)
    {
        if (walls[i].y >= playerY)
        {
            walls.RemoveAt(i);
            lives--;
        }
    }
}

void Draw()
{
    ClearScreenArray();
    DrawWalls();
    DrawPlayer();
    DrawBullets();
    RenderScreen();
}

void ClearScreenArray()
{
    for (int y = 0; y < Height; y++)
    for (int x = 0; x < Width; x++)
        screen[y, x] = ' ';
}

void DrawWalls()
{
    foreach (var wall in walls)
    {
        screen[wall.y, wall.x] = '#';
    }
}

void DrawPlayer()
{
    screen[playerY, playerX] = '^';
}

void DrawBullets()
{
    foreach (var bullet in bullets)
    {
        if (bullet.y >= 0 && bullet.y < Height)
            screen[bullet.y, bullet.x] = '|';
    }
}

void RenderScreen()
{
    Console.SetCursorPosition(0, 0);
    for (int y = 0; y < Height; y++)
    {
        for (int x = 0; x < Width; x++)
            Console.Write(screen[y, x]);
        Console.WriteLine();
    }
    Console.WriteLine(new string('-', Width));
    Console.WriteLine($"Score: {score}   Lives: {lives}   Press A/D or Arrow keys to move, Space to shoot, Q to quit");
}

void InitializeWalls()
{
    walls.Clear();
    for (int y = 1; y <= 4; y++)
    for (int x = 2; x < Width - 2; x += 2)
        walls.Add((x, y));
}

void ShowMessage(string message)
{
    Console.Clear();
    Console.SetCursorPosition(0, 0);
    Console.WriteLine(message);
    Console.WriteLine($"Final Score: {score}");
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey(true);
}

void Reset()
{
    score = 0;
    lives = 3;
    bullets.Clear();
    playerX = Width / 2;
    InitializeWalls();
    Console.Clear();
}
