namespace ChessLib.Common
{
    public interface IView
    {
        public void Show(string msg);
        public void Visualize(string[,] GameField, int CurrentPlayer);
    }
}