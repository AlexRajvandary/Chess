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

### Architecture Layers

```mermaid
graph TB
    subgraph WPF["WPF Application"]
        direction TB
        MW["MainWindow"]
        MV["MainViewModel"]
        GV["GameViewModel<br/>UI logic only"]
        BV["BoardViewModel"]
    end
    
    subgraph Application["Application Layer"]
        direction TB
        IGS["IGameService<br/>Abstraction<br/>&lt;&lt;interface&gt;&gt;"]
        GS_Impl["GameService<br/>Implementation"]
    end
    
    subgraph ChessLib["ChessLib Domain"]
        direction TB
        IGE["IGameEngine<br/>Abstraction<br/>&lt;&lt;interface&gt;&gt;"]
        GE["GameEngine<br/>Encapsulated<br/>implementation"]
        IGS_State["IGameState<br/>State abstraction"]
        Cache["StateCache<br/>Caching"]
        
        subgraph Board["Board Abstraction"]
            IBS["IBoardState"]
            IBR["IBoardRepresentation"]
            ABR["ArrayBoardRep<br/>Current implementation"]
            BBR["BitboardRep<br/>Future optimization"]
        end
        
        subgraph Strategies["Move Strategies"]
            IMS["IMoveStrategy<br/>&lt;&lt;interface&gt;&gt;"]
            MSR["MoveStrategyRegistry"]
            PMS["PawnMoveStrategy"]
            KMS["KingMoveStrategy"]
            QMS["QueenMoveStrategy"]
            ISMS["ISpecialMoveStrategy<br/>&lt;&lt;interface&gt;&gt;<br/>Special moves"]
            EPS["EnPassantStrategy<br/>En Passant"]
            CS["CastlingStrategy<br/>Castling"]
        end
    end
    
    %% Connections
    MW --> MV
    MV --> GV
    GV --> BV
    GV --> IGS
    IGS -.->|"implements"| GS_Impl
    GS_Impl --> IGE
    IGE -.->|"implements"| GE
    GE --> IBS
    GE --> Cache
    GE --> IGS_State
    GE --> IMS
    IBS --> IBR
    IBR -.->|"implements"| ABR
    IBR -.->|"future"| BBR
    IMS -.->|"implements"| PMS
    IMS -.->|"implements"| KMS
    IMS -.->|"implements"| QMS
    MSR --> IMS
    ISMS -.->|"implements"| EPS
    ISMS -.->|"implements"| CS
    
    %% Styling
    style GV fill:#2f9e44,stroke:#1e7e34,stroke-width:3px,color:#ffffff
    style IGS fill:#2f9e44,stroke:#1e7e34,stroke-width:3px,color:#ffffff
    style GS_Impl fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style IGE fill:#2f9e44,stroke:#1e7e34,stroke-width:3px,color:#ffffff
    style GE fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style IGS_State fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style Cache fill:#2f9e44,stroke:#1e7e34,stroke-width:2px,color:#ffffff
    style BBR fill:#6c757d,stroke:#495057,stroke-width:2px,color:#ffffff
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