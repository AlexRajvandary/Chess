# Describtion
Classical Chess.
Supports FEN, algebraic notation, PGN games import, saving games to SQLite, autoplaying loaded games.
Clean MVVM without code-behind.
MVVM, WPF, EF Core, DI Container, SQLite

### WPF
### Autoplaying imported game
![img](images/magnus.gif)
![img](images/autoplay.gif)
### Selecting different color themes
![img](images/colors2.gif)
### Importing games
![img](images/import_games.gif)
### Basic game
![img](images/basic_play.gif)
![img](images/enpassant.gif)
### Console
![img](images/chessConsolePlay.gif)
## Features
- Full chess rules implementation
- Available moves highlighting
- FEN notation (read and write)
- Algebraic move notation
- PGN format support (import and export)
- Move history with navigation
- Modern user interface
- Custom color configuration (light and dark squares)
- Sound effects for moves
- Save and load games
- Import historical games from PGN files
- Auto-play for historical games
- Text-based interface for console play
- ASCII board visualization
- Interactive piece and move selection

## Architecture

The application is built on clean architecture principles with clear layer separation:

### Project Structure

```
Chess/
├── ChessLib/              # Domain Layer - game logic
├── WPFChess/              # Presentation Layer - WPF application
├── ChessConsole/          # Presentation Layer - console application
└── ChessLib.Tests/        # Unit tests
```

### Architecture

```mermaid
classDiagram
    direction TB
    
    %% ============================================
    %% WPF LAYER - Presentation
    %% ============================================
    
    class MainWindow {
        +MainViewModel ViewModel
        +ShowPanel()
        +HidePanel()
    }
    
    class MainViewModel {
        +GameViewModel Game
        +BoardViewModel Board
        +TimerViewModel Timer
        +CapturedPiecesViewModel CapturedPieces
        +GameStorageViewModel GameStorage
        +MoveHistoryViewModel MoveHistory
        +SettingsViewModel Settings
        +PanelManagementViewModel Panels
        +Brush DarkSquareColor
        +Brush LightSquareColor
    }
    
    class GameViewModel {
        -IGameService gameService
        -BoardViewModel board
        -SoundService soundService
        +HandleCellClick(Position)
        +UpdateViewFromGameState()
        +GetStateFromPiece(IPiece) CellUIState
        +string Fen
        +string MoveHistory
    }
    
    class BoardViewModel {
        -CellViewModel[,] cells
        +CellUIState this[int, int]
        +UpdateFromState(IGameState)
        +IEnumerable~CellViewModel~ GetEnumerator()
    }
    
    class CellViewModel {
        +Position Position
        +CellUIState State
        +bool Active
        +bool AvailableMove
    }
    
    class TimerViewModel {
        +TimeSpan WhiteTime
        +TimeSpan BlackTime
        +void Start()
        +void Pause()
        +void Reset()
    }
    
    class CapturedPiecesViewModel {
        +ObservableCollection~IPieceInfo~ WhiteCaptured
        +ObservableCollection~IPieceInfo~ BlackCaptured
        +UpdateFromMoveHistory(Func~IPiece, CellUIState~)
    }
    
    class GameStorageViewModel {
        -IGameService gameService
        -GameStorageService storageService
        +ObservableCollection~GameRecord~ SavedGames
        +GameRecord SelectedGame
        +ICommand SaveGameCommand
        +ICommand LoadSelectedGameCommand
    }
    
    class MoveHistoryViewModel {
        -IGameService gameService
        +ObservableCollection~MoveDisplayItem~ MoveHistoryItems
        +bool IsGameLoaded
        +void LoadGame(List~string~)
        +void NavigateToMove(int)
    }
    
    class SettingsViewModel {
        +ColorScheme CurrentScheme
        +PanelPosition PanelPosition
        +bool ShowAvailableMoves
        +event OnColorSchemeChanged
        +event OnPanelPositionChanged
    }
    
    class PanelManagementViewModel {
        +bool IsGamePanelOpen
        +bool IsSettingsPanelOpen
        +void ToggleGamePanel()
        +void ToggleSettingsPanel()
    }
    
    class HistoricalGamesViewModel {
        +ObservableCollection~HistoricalGame~ Games
        +HistoricalGame SelectedHistoricalGame
        +Action~HistoricalGame~ OnGameLoadRequested
    }
    
    %% ============================================
    %% APPLICATION LAYER
    %% ============================================
    
    class IGameService {
        <<interface>>
        +MoveResult MakeMove(Position, Position)
        +IReadOnlyList~Position~ GetValidMoves(Position)
        +IGameState GetState()
        +void StartNewGame()
        +string GetFen()
        +string GetMoveHistory()
    }
    
    class GameService {
        -IGameEngine gameEngine
        +MoveResult MakeMove(Position, Position)
        +IReadOnlyList~Position~ GetValidMoves(Position)
        +IGameState GetState()
        +void StartNewGame()
    }
    
    %% ============================================
    %% CHESSLIB DOMAIN LAYER
    %% ============================================
    
    class IGameEngine {
        <<interface>>
        +MoveResult MakeMove(Position, Position)
        +IReadOnlyList~Position~ GetValidMoves(Position)
        +IGameState GetState()
        +void StartNewGame()
        +string GetFen()
        +string GetMoveHistory()
        +void LoadFromPgn(string)
        +void LoadFromFen(string)
        +void LoadFromState(IGameState)
    }
    
    class GameEngine {
        -Game game
        -IGameStateCache cache
        -IMoveCalculator moveCalculator
        +MoveResult MakeMove(Position, Position)
        +IReadOnlyList~Position~ GetValidMoves(Position)
        +IGameState GetState()
    }
    
    class IGameState {
        <<interface>>
        +PieceColor CurrentPlayerColor
        +bool IsCheck
        +bool IsCheckmate
        +bool IsGameOver
        +IReadOnlyList~IPieceInfo~ Pieces
    }
    
    class GameState {
        +PieceColor CurrentPlayerColor
        +bool IsCheck
        +bool IsCheckmate
        +bool IsGameOver
        +IReadOnlyList~IPieceInfo~ Pieces
    }
    
    class IGameStateCache {
        <<interface>>
        +IGameState GetState()
        +void Invalidate()
        +void UpdateAfterMove(Move)
    }
    
    class GameStateCache {
        -IGameState cachedState
        -Dictionary~Position, List~Position~~ cachedMoves
        +IGameState GetState()
        +void Invalidate()
    }
    
    class IMoveCalculator {
        <<interface>>
        +List~Position~ GetValidMoves(IPiece, IGameState)
    }
    
    class MoveCalculator {
        -IMoveStrategyRegistry strategyRegistry
        -ISpecialMoveStrategy[] specialStrategies
        -IMoveValidator moveValidator
        +List~Position~ GetValidMoves(IPiece, IGameState)
    }
    
    class IMoveStrategy {
        <<interface>>
        +PieceType PieceType
        +List~Position~ GetPossibleMoves(IPiece, IBoardQuery)
        +List~Position~ GetPossibleCaptures(IPiece, IBoardQuery)
    }
    
    class IMoveStrategyRegistry {
        <<interface>>
        +IMoveStrategy GetStrategy(PieceType)
    }
    
    class MoveStrategyRegistry {
        -Dictionary~PieceType, IMoveStrategy~ strategies
        +IMoveStrategy GetStrategy(PieceType)
    }
    
    class PawnMoveStrategy {
        +List~Position~ GetPossibleMoves(IPiece, IBoardQuery)
        +List~Position~ GetPossibleCaptures(IPiece, IBoardQuery)
    }
    
    class KingMoveStrategy {
        +List~Position~ GetPossibleMoves(IPiece, IBoardQuery)
        +List~Position~ GetPossibleCaptures(IPiece, IBoardQuery)
    }
    
    class QueenMoveStrategy {
        +List~Position~ GetPossibleMoves(IPiece, IBoardQuery)
        +List~Position~ GetPossibleCaptures(IPiece, IBoardQuery)
    }
    
    class RookMoveStrategy {
        +List~Position~ GetPossibleMoves(IPiece, IBoardQuery)
        +List~Position~ GetPossibleCaptures(IPiece, IBoardQuery)
    }
    
    class BishopMoveStrategy {
        +List~Position~ GetPossibleMoves(IPiece, IBoardQuery)
        +List~Position~ GetPossibleCaptures(IPiece, IBoardQuery)
    }
    
    class KnightMoveStrategy {
        +List~Position~ GetPossibleMoves(IPiece, IBoardQuery)
        +List~Position~ GetPossibleCaptures(IPiece, IBoardQuery)
    }
    
    class ISpecialMoveStrategy {
        <<interface>>
        +List~Position~ GetSpecialMoves(IPiece, IGameState)
    }
    
    class EnPassantStrategy {
        +List~Position~ GetSpecialMoves(IPiece, IGameState)
    }
    
    class CastlingStrategy {
        +List~Position~ GetSpecialMoves(IPiece, IGameState)
    }
    
    class IBoardQuery {
        <<interface>>
        +IPieceInfo GetPieceAt(Position)
        +bool IsCellFree(Position)
        +bool IsCellAttacked(Position, PieceColor)
    }
    
    class BoardQuery {
        -IGameState state
        +IPieceInfo GetPieceAt(Position)
        +bool IsCellFree(Position)
        +bool IsCellAttacked(Position, PieceColor)
    }
    
    class IBoardState {
        <<interface>>
        +IPieceInfo GetPieceAt(Position)
        +bool IsCellFree(Position)
        +IReadOnlyList~Position~ GetAttackedCells(PieceColor)
    }
    
    class BoardState {
        -IBoardRepresentation representation
        +IPieceInfo GetPieceAt(Position)
        +bool IsCellFree(Position)
    }
    
    class IBoardRepresentation {
        <<interface>>
        +IPieceInfo GetPieceAt(Position)
        +void SetPieceAt(Position, IPieceInfo)
        +void ClearCell(Position)
        +void UpdateAfterMove(Move)
    }
    
    class ArrayBoardRepresentation {
        -IPieceInfo[,] board
        +IPieceInfo GetPieceAt(Position)
        +void UpdateAfterMove(Move)
    }
    
    class BitboardRepresentation {
        -ulong[] whitePieces
        -ulong[] blackPieces
        +IPieceInfo GetPieceAt(Position)
        +void UpdateAfterMove(Move)
    }
    
    class IPieceInfo {
        <<interface>>
        +PieceType Type
        +PieceColor Color
        +Position Position
    }
    
    class PieceInfo {
        +PieceType Type
        +PieceColor Color
        +Position Position
    }
    
    class IPiece {
        <<interface>>
        +PieceType Type
        +PieceColor Color
        +Position Position
        +bool IsDead
    }
    
    class IMoveValidator {
        <<interface>>
        +bool IsValidMove(IPiece, Position, IGameState)
        +List~Position~ FilterValidMoves(IPiece, List~Position~, IGameState)
    }
    
    class MoveValidator {
        +bool IsValidMove(IPiece, Position, IGameState)
        +List~Position~ FilterValidMoves(IPiece, List~Position~, IGameState)
    }
    
    class IMoveExecutor {
        <<interface>>
        +MoveResult ExecuteMove(IPiece, Position, IGameState)
        +MoveResult ExecuteEnPassant(Pawn, Position, Pawn, IGameState)
        +MoveResult ExecuteCastling(King, Rook, CastleType, IGameState)
    }
    
    class MoveExecutor {
        +MoveResult ExecuteMove(IPiece, Position, IGameState)
        +MoveResult ExecuteEnPassant(Pawn, Position, Pawn, IGameState)
        +MoveResult ExecuteCastling(King, Rook, CastleType, IGameState)
    }
    
    class MoveResult {
        +bool Success
        +MoveType MoveType
        +string ErrorMessage
        +IPieceInfo CapturedPiece
        +bool IsCheck
        +bool IsCheckmate
    }
    
    %% ============================================
    %% WPF SERVICES
    %% ============================================
    
    class SoundService {
        +void PlayMoveSound()
        +void PlayCaptureSound()
        +void PlayCheckSound()
    }
    
    class GameStorageService {
        +void SaveGame(GameRecord)
        +List~GameRecord~ LoadSavedGames()
        +void ImportGames(string)
    }
    
    class PgnService {
        +string GeneratePgn(IGameState)
        +List~string~ ParsePgnMoves(string)
        +Dictionary~string, string~ ParsePgnHeaders(string)
    }
    
    %% ============================================
    %% CONNECTIONS - WPF Layer
    %% ============================================
    
    MainWindow --> MainViewModel
    MainViewModel --> GameViewModel
    MainViewModel --> BoardViewModel
    MainViewModel --> TimerViewModel
    MainViewModel --> CapturedPiecesViewModel
    MainViewModel --> GameStorageViewModel
    MainViewModel --> MoveHistoryViewModel
    MainViewModel --> SettingsViewModel
    MainViewModel --> PanelManagementViewModel
    MainViewModel --> HistoricalGamesViewModel
    
    GameViewModel --> IGameService : ✅ через интерфейс
    GameViewModel --> BoardViewModel
    GameViewModel --> SoundService
    BoardViewModel --> CellViewModel
    
    GameStorageViewModel --> IGameService
    GameStorageViewModel --> GameStorageService
    MoveHistoryViewModel --> IGameService
    
    %% ============================================
    %% CONNECTIONS - Application Layer
    %% ============================================
    
    IGameService <|.. GameService : implements
    GameService --> IGameEngine : ✅ через интерфейс
    
    %% ============================================
    %% CONNECTIONS - Domain Layer
    %% ============================================
    
    IGameEngine <|.. GameEngine : implements
    GameEngine --> IGameStateCache
    GameEngine --> IMoveCalculator
    GameEngine --> IGameState : returns
    
    IGameStateCache <|.. GameStateCache : implements
    IGameState <|.. GameState : implements
    
    IMoveCalculator <|.. MoveCalculator : implements
    MoveCalculator --> IMoveStrategyRegistry
    MoveCalculator --> ISpecialMoveStrategy
    MoveCalculator --> IMoveValidator
    
    IMoveStrategyRegistry <|.. MoveStrategyRegistry : implements
    IMoveStrategyRegistry --> IMoveStrategy : provides
    
    IMoveStrategy <|.. PawnMoveStrategy : implements
    IMoveStrategy <|.. KingMoveStrategy : implements
    IMoveStrategy <|.. QueenMoveStrategy : implements
    IMoveStrategy <|.. RookMoveStrategy : implements
    IMoveStrategy <|.. BishopMoveStrategy : implements
    IMoveStrategy <|.. KnightMoveStrategy : implements
    
    ISpecialMoveStrategy <|.. EnPassantStrategy : implements
    ISpecialMoveStrategy <|.. CastlingStrategy : implements
    
    IMoveStrategy --> IBoardQuery : uses
    IBoardQuery <|.. BoardQuery : implements
    BoardQuery --> IGameState
    
    IBoardState <|.. BoardState : implements
    BoardState --> IBoardRepresentation
    IBoardRepresentation <|.. ArrayBoardRepresentation : implements
    IBoardRepresentation <|.. BitboardRepresentation : implements
    
    IMoveValidator <|.. MoveValidator : implements
    IMoveExecutor <|.. MoveExecutor : implements
    
    GameState --> IPieceInfo
    IPieceInfo <|.. PieceInfo : implements
    
    %% ============================================
    %% STYLING
    %% ============================================
    
    style MainWindow fill:#4a90e2,stroke:#2c5aa0,stroke-width:2px,color:#ffffff
    style MainViewModel fill:#4a90e2,stroke:#2c5aa0,stroke-width:2px,color:#ffffff
    style GameViewModel fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style BoardViewModel fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style CellViewModel fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    
    style IGameService fill:#2f9e44,stroke:#1e7e34,stroke-width:3px,color:#ffffff
    style GameService fill:#2f9e44,stroke:#1e7e44,stroke-width:2px,color:#ffffff
    
    style IGameEngine fill:#2f9e44,stroke:#1e7e34,stroke-width:3px,color:#ffffff
    style GameEngine fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style IGameState fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style IGameStateCache fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style IMoveCalculator fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style IMoveStrategy fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style ISpecialMoveStrategy fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style IBoardQuery fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style IBoardState fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style IBoardRepresentation fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style IPieceInfo fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style BitboardRepresentation fill:#6c757d,stroke:#495057,stroke-width:2px,color:#ffffff
```

### Key Principles

**WPF Layer (Presentation)**:
- All ViewModels work only with UI logic
- `GameViewModel` uses `IGameService` (doesn't know about the library)
- `BoardViewModel` uses `IGameState` directly (no duplication)
- Clear separation of responsibilities between ViewModels

**Application Layer**:
- `IGameService` - single entry point for WPF
- `GameService` - simple wrapper over `IGameEngine`
- Isolation of WPF from library details

**Domain Layer (ChessLib)**:
- `IGameEngine` - main library interface
- `GameEngine` - encapsulated implementation
- `IMoveStrategy` - Strategy Pattern for all pieces
- `ISpecialMoveStrategy` - special moves (En Passant, castling)
- `IGameStateCache` - caching for performance
- `IBoardQuery` - board abstraction for strategies
- `IBoardRepresentation` - preparation for bitboards

**Architecture Benefits**:
- Full encapsulation - WPF doesn't know about library internals
- Flexibility - can change implementation without WPF changes
- Performance - caching and Strategy Pattern
- Testability - all components are isolated
- Extensibility - easy to add new strategies or board representations

> For detailed architecture analysis and refactoring plan, see [ARCHITECTURE_ANALYSIS.md](ARCHITECTURE_ANALYSIS.md)
