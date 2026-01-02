using ChessLib.Common;
using System.Collections.Generic;

namespace ChessLib
{
    public class MoveStrategyRegistry : IMoveStrategyRegistry
    {
        private readonly Dictionary<PieceType, IMoveStrategy> strategies;

        public MoveStrategyRegistry(IEnumerable<IMoveStrategy> strategies)
        {
            this.strategies = new Dictionary<PieceType, IMoveStrategy>();
            foreach (var strategy in strategies)
            {
                this.strategies[strategy.PieceType] = strategy;
            }
        }

        public IMoveStrategy GetStrategy(PieceType pieceType)
        {
            return strategies.TryGetValue(pieceType, out var strategy) 
                ? strategy 
                : throw new System.ArgumentException($"No strategy found for piece type: {pieceType}");
        }
    }
}

