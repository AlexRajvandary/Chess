using ChessBoard;
namespace ChessBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this);
        }
        public void AddNewWhiteMove(string Move)
        {
            ((MainViewModel)DataContext).playerMoves.Add(Move);
            this.MovesList.Items.Add(Move);

        }
        public void AddNewBlackMove(string Move)
        {
            ((MainViewModel)DataContext).playerMoves.Add(Move);
            this.MovesList.Items.Add(Move);

        }
    }
}
