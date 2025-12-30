# Архитектура Chess Application

## Диаграмма архитектуры WPF и ChessLib

```mermaid
graph TB
    subgraph WPF["WPF Application Layer"]
        MW[MainWindow]
        MV[MainViewModel]
        GV[GameViewModel]
        BV[BoardViewModel]
        CV[CellViewModel]
        THV[TimerViewModel]
        CPV[CapturedPiecesViewModel]
        MHV[MoveHistoryViewModel]
        GSV[GameStorageViewModel]
        HGV[HistoricalGamesViewModel]
        SV[SettingsViewModel]
        PMV[PanelManagementViewModel]
        
        subgraph WPF_Services["WPF Services"]
            CGS[ChessGameService]
            GSS[GameStorageService]
            PGN[PgnService]
            SS[SoundService]
        end
        
        subgraph WPF_Models["WPF Models"]
            GR[GameRecord]
            HG[HistoricalGame]
            CS[ColorScheme]
            TO[TimeOption]
        end
        
        subgraph WPF_Controls["User Controls"]
            GPV[GamePlayView]
            CBV[ChessBoardView]
            GSV_UC[GameStorageView]
            HGV_UC[HistoricalGamesView]
            SV_UC[SettingsView]
        end
    end
    
    subgraph ChessLib["ChessLib Domain Layer"]
        G[Game]
        
        subgraph ChessLib_Services["ChessLib Services"]
            MV_Val[MoveValidator]
            ME[MoveExecutor]
            GF[GameField]
            FEN[Fen]
            AN[AlgebraicNotation]
            GS[GameState]
        end
        
        subgraph Pieces["Pieces"]
            IP[IPiece]
            PB[PieceBase]
            P[Pawn]
            K[King]
            Q[Queen]
            R[Rook]
            B[Bishop]
            N[Knight]
        end
        
        subgraph Common["Common Types"]
            POS[Position]
            PL[Player]
            MN[MoveNotation]
            MR[MoveResult]
            MT[MoveType]
            PC[PieceColor]
        end
    end
    
    %% WPF ViewModel connections
    MW --> MV
    MV --> GV
    MV --> THV
    MV --> CPV
    MV --> MHV
    MV --> GSV
    MV --> HGV
    MV --> SV
    MV --> PMV
    
    GV --> BV
    BV --> CV
    GV --> CGS
    GV --> SS
    GV --> MHV
    GV --> CPV
    
    %% WPF Service connections
    CGS --> G
    GSV --> GSS
    HGV --> PGN
    GSS --> GR
    HGV --> HG
    
    %% ChessLib connections
    G --> MV_Val
    G --> ME
    G --> GF
    G --> FEN
    G --> AN
    G --> GS
    G --> PL
    G --> MN
    
    MV_Val --> GF
    ME --> GF
    
    G --> IP
    IP --> PB
    PB --> P
    PB --> K
    PB --> Q
    PB --> R
    PB --> B
    PB --> N
    
    G --> POS
    G --> MR
    G --> MT
    G --> PC
    
    %% Cross-layer connections
    CGS -.->|uses| G
    CGS -.->|uses| POS
    CGS -.->|uses| IP
    GV -.->|uses| CGS
    
    style WPF fill:#e1f5ff
    style ChessLib fill:#fff4e1
    style WPF_Services fill:#d4edda
    style ChessLib_Services fill:#f8d7da
    style Pieces fill:#e7d4f8
    style Common fill:#fff9c4
```

## Диаграмма классов ChessLib

```mermaid
classDiagram
    class Game {
        -MoveValidator moveValidator
        -MoveExecutor moveExecutor
        +GameField GameField
        +int CurrentPlayer
        +List~IPiece~ Pieces
        +List~Player~ Players
        +bool IsGameOver
        +List~MoveNotation~ MoveHistory
        +MakeMove(Position, Position) MoveResult
        +GetValidMoves(Position) List~Position~
        +IsCheck(PieceColor) bool
        +IsCheckmate(PieceColor) bool
        +GetState() GameState
        +StartNewGame() void
    }
    
    class IPiece {
        <<interface>>
        +bool IsDead
        +PieceColor Color
        +Position Position
        +AvailableMoves(string[,]) List~Position~
        +AvailableKills(string[,]) List~Position~
        +ChangePosition(Position) void
    }
    
    class PieceBase {
        <<abstract>>
        #PieceColor Color
        #Position Position
        #bool IsDead
        +AvailableMoves(string[,])* List~Position~
        +AvailableKills(string[,])* List~Position~
        +ChangePosition(Position)* void
    }
    
    class Pawn {
        +bool EnPassantAvailable
        +Position StartPos
        +AvailableMoves(string[,]) List~Position~
        +AvailableKills(string[,]) List~Position~
    }
    
    class King {
        +bool IsMoved
        +AvailableMoves(string[,]) List~Position~
        +AvailableKills(string[,]) List~Position~
    }
    
    class Queen {
        +AvailableMoves(string[,]) List~Position~
        +AvailableKills(string[,]) List~Position~
    }
    
    class Rook {
        +bool IsMoved
        +RookKind Kind
        +AvailableMoves(string[,]) List~Position~
        +AvailableKills(string[,]) List~Position~
    }
    
    class Bishop {
        +AvailableMoves(string[,]) List~Position~
        +AvailableKills(string[,]) List~Position~
    }
    
    class Knight {
        +AvailableMoves(string[,]) List~Position~
        +AvailableKills(string[,]) List~Position~
    }
    
    class MoveValidator {
        -GameField gameField
        +IsValidMove(List~IPiece~, IPiece, Position, string[,]) bool
        +CanCastle(List~IPiece~, King, Rook, CastleType, string[,]) bool
        +FilterValidMoves(List~IPiece~, IPiece, List~Position~, string[,]) List~Position~
    }
    
    class MoveExecutor {
        -GameField gameField
        +ExecuteMove(List~IPiece~, IPiece, Position, string[,]) MoveResult
        +ExecuteCastling(List~IPiece~, King, Rook, CastleType) MoveResult
        +ExecuteEnPassant(List~IPiece~, Pawn, Position, Pawn) MoveResult
        +RemoveDeadPieces(List~IPiece~) void
    }
    
    class GameField {
        +Update(List~IPiece~, string[,], PieceColor) void
        +IsCellFree(Position) bool
        +GetAtackStatus(List~IPiece~, Position, string[,]) bool
    }
    
    class Position {
        +int X
        +int Y
        +Equals(Position) bool
    }
    
    class Player {
        +PieceColor Color
        +List~IPiece~ Pieces
        +string Name
    }
    
    class MoveNotation {
        +Position From
        +Position To
        +IPiece Piece
        +MoveType MoveType
        +IPiece CapturedPiece
        +PieceColor PlayerColor
        +int MoveNumber
        +bool IsCheck
        +bool IsCheckmate
    }
    
    class MoveResult {
        +bool IsSuccess
        +string Message
        +MoveType MoveType
        +IPiece CapturedPiece
        +bool IsCheck
        +bool IsCheckmate
    }
    
    Game --> MoveValidator : uses
    Game --> MoveExecutor : uses
    Game --> GameField : uses
    Game --> IPiece : contains
    Game --> Player : contains
    Game --> MoveNotation : contains
    
    IPiece <|.. PieceBase : implements
    PieceBase <|-- Pawn : extends
    PieceBase <|-- King : extends
    PieceBase <|-- Queen : extends
    PieceBase <|-- Rook : extends
    PieceBase <|-- Bishop : extends
    PieceBase <|-- Knight : extends
    
    MoveValidator --> GameField : uses
    MoveExecutor --> GameField : uses
    MoveValidator --> IPiece : validates
    MoveExecutor --> IPiece : executes
    Game --> Position : uses
    MoveNotation --> Position : uses
    MoveNotation --> IPiece : references
    MoveResult --> IPiece : references
```

## Диаграмма классов WPF

```mermaid
classDiagram
    class MainWindow {
        +MainWindow(MainViewModel)
        -SettingsOverlay_MouseDown()
        -GameOverlay_MouseDown()
    }
    
    class MainViewModel {
        -GameViewModel gameViewModel
        -TimerViewModel timerViewModel
        -CapturedPiecesViewModel capturedPiecesViewModel
        -GameStorageViewModel gameStorageViewModel
        -HistoricalGamesViewModel historicalGamesViewModel
        -MoveHistoryViewModel moveHistoryViewModel
        -SettingsViewModel settingsViewModel
        +Brush DarkSquareColor
        +Brush LightSquareColor
        +GameViewModel Game
        +TimerViewModel Timer
    }
    
    class GameViewModel {
        -ChessGameService gameService
        -SoundService soundService
        -BoardViewModel board
        +string Fen
        +string MoveHistory
        +BoardViewModel Board
        +ICommand CellCommand
        +MakeMove(Position, Position) void
        +StartNewGame() void
    }
    
    class BoardViewModel {
        -ObservableCollection~CellViewModel~ cells
        +ObservableCollection~CellViewModel~ Cells
        +SetupBoard(BoardStateSnapshot) void
    }
    
    class CellViewModel {
        +int X
        +int Y
        +PieceInfo Piece
        +bool IsSelected
        +bool IsAvailableMove
        +Brush BackgroundColor
    }
    
    class ChessGameService {
        -Game _game
        +Game CurrentGame
        +StartNewGame() void
        +GetBoardState() BoardStateSnapshot
        +GetValidMoves(Position) List~Position~
        +MakeMove(Position, Position) MoveResult
        +GetFen() string
    }
    
    class GameStorageService {
        +SaveGame(GameRecord) void
        +LoadGames() List~GameRecord~
        +DeleteGame(int) void
    }
    
    class PgnService {
        +ParsePgnMoves(string) List~Move~
        +ExportToPgn(Game) string
    }
    
    class SoundService {
        +PlayMoveSound() void
        +PlayCaptureSound() void
        +PlayCheckSound() void
    }
    
    class TimerViewModel {
        +TimeSpan WhiteTime
        +TimeSpan BlackTime
        +bool IsRunning
        +Start() void
        +Stop() void
        +Reset() void
    }
    
    class MoveHistoryViewModel {
        -ObservableCollection~MoveDisplayItem~ moves
        +ObservableCollection~MoveDisplayItem~ Moves
        +LoadGame(List~Move~) void
        +NavigateToMove(int) void
    }
    
    class GameRecord {
        +int Id
        +string PgnNotation
        +DateTime CreatedAt
        +string FinalFen
    }
    
    MainWindow --> MainViewModel : uses
    MainViewModel --> GameViewModel : contains
    MainViewModel --> TimerViewModel : contains
    MainViewModel --> CapturedPiecesViewModel : contains
    MainViewModel --> GameStorageViewModel : contains
    MainViewModel --> HistoricalGamesViewModel : contains
    MainViewModel --> MoveHistoryViewModel : contains
    MainViewModel --> SettingsViewModel : contains
    
    GameViewModel --> BoardViewModel : contains
    GameViewModel --> ChessGameService : uses
    GameViewModel --> SoundService : uses
    GameViewModel --> TimerViewModel : uses
    GameViewModel --> MoveHistoryViewModel : uses
    
    BoardViewModel --> CellViewModel : contains
    
    ChessGameService --> Game : uses
    GameStorageService --> GameRecord : manages
    PgnService --> Game : uses
```

## Диаграмма последовательности выполнения хода

```mermaid
sequenceDiagram
    participant User
    participant CellView
    participant GameViewModel
    participant ChessGameService
    participant Game
    participant MoveValidator
    participant MoveExecutor
    participant BoardViewModel
    
    User->>CellView: Click on cell
    CellView->>GameViewModel: CellCommand.Execute()
    GameViewModel->>ChessGameService: MakeMove(from, to)
    ChessGameService->>Game: MakeMove(from, to)
    
    Game->>Game: Get piece at position
    Game->>MoveValidator: IsValidMove(piece, to)
    MoveValidator->>MoveValidator: Check move rules
    MoveValidator-->>Game: true/false
    
    alt Move is valid
        Game->>MoveExecutor: ExecuteMove(piece, to)
        MoveExecutor->>MoveExecutor: Check for capture
        MoveExecutor->>MoveExecutor: Update piece position
        MoveExecutor-->>Game: MoveResult
        
        Game->>Game: Update game state
        Game->>Game: Check for check/checkmate
        Game->>Game: Switch player
        Game-->>ChessGameService: MoveResult
        
        ChessGameService-->>GameViewModel: MoveResult
        
        GameViewModel->>SoundService: PlayMoveSound()
        GameViewModel->>ChessGameService: GetBoardState()
        ChessGameService->>Game: GetState()
        Game-->>ChessGameService: GameState
        ChessGameService-->>GameViewModel: BoardStateSnapshot
        
        GameViewModel->>BoardViewModel: SetupBoard(snapshot)
        BoardViewModel->>CellViewModel: Update cells
        GameViewModel->>MoveHistoryViewModel: Update move history
        GameViewModel->>CapturedPiecesViewModel: Update captured pieces
    else Move is invalid
        Game-->>ChessGameService: MoveResult.Failure
        ChessGameService-->>GameViewModel: MoveResult.Failure
        GameViewModel->>User: Show error message
    end
```

## Диаграмма компонентов

```mermaid
graph LR
    subgraph Presentation["Presentation Layer (WPF)"]
        UI[User Interface<br/>XAML Views]
        VM[ViewModels<br/>MVVM Pattern]
    end
    
    subgraph Application["Application Layer"]
        WPF_SVC[WPF Services<br/>ChessGameService<br/>GameStorageService<br/>PgnService]
    end
    
    subgraph Domain["Domain Layer (ChessLib)"]
        Game_Logic[Game Logic<br/>Game Class]
        Validation[Move Validation<br/>MoveValidator]
        Execution[Move Execution<br/>MoveExecutor]
        Pieces[Chess Pieces<br/>IPiece implementations]
    end
    
    subgraph Infrastructure["Infrastructure"]
        Data[Data Access<br/>Entity Framework]
        Files[File System<br/>PGN Files]
    end
    
    UI --> VM
    VM --> WPF_SVC
    WPF_SVC --> Game_Logic
    Game_Logic --> Validation
    Game_Logic --> Execution
    Game_Logic --> Pieces
    WPF_SVC --> Data
    WPF_SVC --> Files
    
    style Presentation fill:#e1f5ff
    style Application fill:#d4edda
    style Domain fill:#fff4e1
    style Infrastructure fill:#f8d7da
```

