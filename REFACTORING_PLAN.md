# План рефакторинга для правильного разделения слоев

## Текущие проблемы

### В MainViewModel (WPF) есть бизнес-логика, которая должна быть в ChessLib:
1. **Валидация ходов:**
   - `GetCheckStatusAfterMove()` - проверка, будет ли шах после хода
   - `GetAtackStatus()` - проверка, атакована ли клетка
   - Фильтрация валидных ходов с учетом шаха

2. **Логика рокировки:**
   - `ShortCastling()`, `LongCastling()` - проверка возможности рокировки
   - Выполнение рокировки

3. **Логика взятия на проходе:**
   - Проверка возможности EnPassant
   - Выполнение EnPassant

4. **Проверка правил:**
   - Проверка, можно ли атаковать короля
   - Проверка валидности хода с учетом всех правил

## Что должно быть в ChessLib

### Класс Game должен предоставлять чистый API:

```csharp
public class Game
{
    // Состояние игры
    public GameState State { get; }
    public PieceColor CurrentPlayerColor { get; }
    public bool IsGameOver { get; }
    
    // Основные методы
    public MoveResult MakeMove(Position from, Position to);
    public List<Position> GetValidMoves(Position piecePosition);
    public List<Position> GetValidMoves(IPiece piece);
    public bool IsValidMove(Position from, Position to);
    
    // Проверка правил
    public bool IsCheck(PieceColor color);
    public bool IsCheckmate(PieceColor color);
    public bool IsStalemate(PieceColor color);
    
    // Специальные ходы
    public bool CanCastle(PieceColor color, CastleType type);
    public MoveResult Castle(PieceColor color, CastleType type);
    public bool CanEnPassant(Position pawnPosition, Position targetPosition);
    
    // Получение состояния
    public BoardState GetBoardState();
    public IPiece GetPieceAt(Position position);
}
```

### Класс MoveValidator должен содержать всю валидацию:

```csharp
public class MoveValidator
{
    public bool IsValidMove(GameState state, Position from, Position to);
    public bool WouldMoveCauseCheck(GameState state, Position from, Position to);
    public bool IsSquareAttacked(GameState state, Position square, PieceColor byColor);
    public List<Position> FilterValidMoves(GameState state, IPiece piece, List<Position> possibleMoves);
}
```

### Класс MoveExecutor должен выполнять ходы:

```csharp
public class MoveExecutor
{
    public MoveResult ExecuteMove(GameState state, Position from, Position to);
    public MoveResult ExecuteCastle(GameState state, PieceColor color, CastleType type);
    public MoveResult ExecuteEnPassant(GameState state, Position from, Position to);
}
```

## Что должно быть в UI проектах

### ConsoleGameController (chess):
- Только UI логика: ввод/вывод
- Вызов методов Game: `game.MakeMove(from, to)`
- Отображение результатов

### MainViewModel (WPF):
- Только UI логика: обработка кликов, обновление UI
- Вызов методов Game: `game.MakeMove(from, to)`, `game.GetValidMoves(piece)`
- Преобразование UI координат в Position
- Обновление View на основе состояния Game

## План действий

1. **Создать структуру Position** (вместо кортежей)
2. **Создать класс GameState** (инкапсулирует состояние игры)
3. **Создать класс MoveResult** (результат выполнения хода)
4. **Создать MoveValidator** (вся валидация)
5. **Создать MoveExecutor** (выполнение ходов)
6. **Рефакторить Game** (предоставить чистый API)
7. **Упростить MainViewModel** (только UI логика)
8. **Упростить ConsoleGameController** (только UI логика)

