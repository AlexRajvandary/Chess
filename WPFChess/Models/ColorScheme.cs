using System.Windows.Media;

namespace ChessWPF.Models
{
    /// <summary>
    /// Represents a color scheme for the chess board
    /// </summary>
    public class ColorScheme
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Brush LightSquareColor { get; set; }
        public Brush DarkSquareColor { get; set; }
        public string Icon { get; set; } // Emoji or symbol for visual representation

        public ColorScheme(string name, string description, Brush lightColor, Brush darkColor, string icon)
        {
            Name = name;
            Description = description;
            LightSquareColor = lightColor;
            DarkSquareColor = darkColor;
            Icon = icon;
        }
    }
}






