using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Game2048
{
  /// <summary>
  /// Logique d'interaction pour MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private const int GridSize = 4;

    private readonly int[,] _board = new int[GridSize, GridSize];
    private readonly Random _random = new Random();
    private readonly Border[,] _cellBorders;
    private readonly TextBlock[,] _cellTexts;

    private int _score;
    private int _bestScore;
    private bool _hasWon;

    public MainWindow()
    {
      InitializeComponent();

      _cellBorders = new[,]
      {
        { CellBorder00, CellBorder01, CellBorder02, CellBorder03 },
        { CellBorder10, CellBorder11, CellBorder12, CellBorder13 },
        { CellBorder20, CellBorder21, CellBorder22, CellBorder23 },
        { CellBorder30, CellBorder31, CellBorder32, CellBorder33 }
      };

      _cellTexts = new[,]
      {
        { CellText00, CellText01, CellText02, CellText03 },
        { CellText10, CellText11, CellText12, CellText13 },
        { CellText20, CellText21, CellText22, CellText23 },
        { CellText30, CellText31, CellText32, CellText33 }
      };

      StartNewGame();
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e)
    {
      StartNewGame();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
      bool moved;

      switch (e.Key)
      {
        case Key.Left:
          moved = MoveLeft();
          break;
        case Key.Right:
          moved = MoveRight();
          break;
        case Key.Up:
          moved = MoveUp();
          break;
        case Key.Down:
          moved = MoveDown();
          break;
        default:
          return;
      }

      if (!moved)
      {
        return;
      }

      AddRandomTile();
      RefreshBoard();
      UpdateGameState();
      e.Handled = true;
    }

    private void StartNewGame()
    {
      Array.Clear(_board, 0, _board.Length);
      _score = 0;
      _hasWon = false;

      AddRandomTile();
      AddRandomTile();
      RefreshBoard();
      StatusTextBlock.Text = "Fusionnez jusqu'a 2048 !";
      Focus();
    }

    private void AddRandomTile()
    {
      var emptyCells = new List<Tuple<int, int>>();

      for (int row = 0; row < GridSize; row++)
      {
        for (int column = 0; column < GridSize; column++)
        {
          if (_board[row, column] == 0)
          {
            emptyCells.Add(Tuple.Create(row, column));
          }
        }
      }

      if (emptyCells.Count == 0)
      {
        return;
      }

      var selectedCell = emptyCells[_random.Next(emptyCells.Count)];
      _board[selectedCell.Item1, selectedCell.Item2] = _random.Next(10) == 0 ? 4 : 2;
    }

    private bool MoveLeft()
    {
      var moved = false;

      for (int row = 0; row < GridSize; row++)
      {
        var line = new int[GridSize];

        for (int column = 0; column < GridSize; column++)
        {
          line[column] = _board[row, column];
        }

        if (MergeLine(line))
        {
          moved = true;
        }

        for (int column = 0; column < GridSize; column++)
        {
          _board[row, column] = line[column];
        }
      }

      return moved;
    }

    private bool MoveRight()
    {
      var moved = false;

      for (int row = 0; row < GridSize; row++)
      {
        var line = new int[GridSize];

        for (int column = 0; column < GridSize; column++)
        {
          line[column] = _board[row, GridSize - 1 - column];
        }

        if (MergeLine(line))
        {
          moved = true;
        }

        for (int column = 0; column < GridSize; column++)
        {
          _board[row, GridSize - 1 - column] = line[column];
        }
      }

      return moved;
    }

    private bool MoveUp()
    {
      var moved = false;

      for (int column = 0; column < GridSize; column++)
      {
        var line = new int[GridSize];

        for (int row = 0; row < GridSize; row++)
        {
          line[row] = _board[row, column];
        }

        if (MergeLine(line))
        {
          moved = true;
        }

        for (int row = 0; row < GridSize; row++)
        {
          _board[row, column] = line[row];
        }
      }

      return moved;
    }

    private bool MoveDown()
    {
      var moved = false;

      for (int column = 0; column < GridSize; column++)
      {
        var line = new int[GridSize];

        for (int row = 0; row < GridSize; row++)
        {
          line[row] = _board[GridSize - 1 - row, column];
        }

        if (MergeLine(line))
        {
          moved = true;
        }

        for (int row = 0; row < GridSize; row++)
        {
          _board[GridSize - 1 - row, column] = line[row];
        }
      }

      return moved;
    }

    private bool MergeLine(int[] line)
    {
      var originalLine = (int[])line.Clone();
      var compacted = new List<int>(GridSize);

      for (int index = 0; index < line.Length; index++)
      {
        if (line[index] != 0)
        {
          compacted.Add(line[index]);
        }
      }

      var merged = new List<int>(GridSize);
      var currentIndex = 0;

      while (currentIndex < compacted.Count)
      {
        if (currentIndex < compacted.Count - 1 && compacted[currentIndex] == compacted[currentIndex + 1])
        {
          var mergedValue = compacted[currentIndex] * 2;
          merged.Add(mergedValue);
          _score += mergedValue;

          if (_score > _bestScore)
          {
            _bestScore = _score;
          }

          if (mergedValue == 2048)
          {
            _hasWon = true;
          }

          currentIndex += 2;
        }
        else
        {
          merged.Add(compacted[currentIndex]);
          currentIndex++;
        }
      }

      while (merged.Count < GridSize)
      {
        merged.Add(0);
      }

      for (int index = 0; index < GridSize; index++)
      {
        line[index] = merged[index];
      }

      for (int index = 0; index < GridSize; index++)
      {
        if (line[index] != originalLine[index])
        {
          return true;
        }
      }

      return false;
    }

    private void RefreshBoard()
    {
      ScoreTextBlock.Text = _score.ToString();
      BestScoreTextBlock.Text = _bestScore.ToString();

      for (int row = 0; row < GridSize; row++)
      {
        for (int column = 0; column < GridSize; column++)
        {
          var value = _board[row, column];
          var border = _cellBorders[row, column];
          var textBlock = _cellTexts[row, column];

          border.Background = CreateBrush(GetTileBackground(value));
          textBlock.Text = value == 0 ? string.Empty : value.ToString();
          textBlock.Foreground = CreateBrush(GetTileForeground(value));
          textBlock.FontSize = GetFontSize(value);
        }
      }
    }

    private void UpdateGameState()
    {
      if (_hasWon)
      {
        StatusTextBlock.Text = "2048 atteint ! Continuez ou relancez.";
        _hasWon = false;
        return;
      }

      StatusTextBlock.Text = CanMove()
        ? "Continuez, la grille est encore ouverte."
        : "Partie terminee. Relancez pour recommencer.";
    }

    private bool CanMove()
    {
      for (int row = 0; row < GridSize; row++)
      {
        for (int column = 0; column < GridSize; column++)
        {
          if (_board[row, column] == 0)
          {
            return true;
          }

          if (column < GridSize - 1 && _board[row, column] == _board[row, column + 1])
          {
            return true;
          }

          if (row < GridSize - 1 && _board[row, column] == _board[row + 1, column])
          {
            return true;
          }
        }
      }

      return false;
    }

    private static Color GetTileBackground(int value)
    {
      switch (value)
      {
        case 0:
          return (Color)ColorConverter.ConvertFromString("#CDC1B4");
        case 2:
          return (Color)ColorConverter.ConvertFromString("#EEE4DA");
        case 4:
          return (Color)ColorConverter.ConvertFromString("#EDE0C8");
        case 8:
          return (Color)ColorConverter.ConvertFromString("#F2B179");
        case 16:
          return (Color)ColorConverter.ConvertFromString("#F59563");
        case 32:
          return (Color)ColorConverter.ConvertFromString("#F67C5F");
        case 64:
          return (Color)ColorConverter.ConvertFromString("#F65E3B");
        case 128:
          return (Color)ColorConverter.ConvertFromString("#EDCF72");
        case 256:
          return (Color)ColorConverter.ConvertFromString("#EDCC61");
        case 512:
          return (Color)ColorConverter.ConvertFromString("#EDC850");
        case 1024:
          return (Color)ColorConverter.ConvertFromString("#EDC53F");
        case 2048:
          return (Color)ColorConverter.ConvertFromString("#EDC22E");
        default:
          return (Color)ColorConverter.ConvertFromString("#3C3A32");
      }
    }

    private static Color GetTileForeground(int value)
    {
      return value <= 4 ? (Color)ColorConverter.ConvertFromString("#776E65") : Colors.White;
    }

    private static double GetFontSize(int value)
    {
      if (value < 128)
      {
        return 32;
      }

      if (value < 1024)
      {
        return 28;
      }

      return 24;
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
      return new SolidColorBrush(color);
    }
  }
}
