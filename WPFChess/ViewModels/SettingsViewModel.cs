using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using ChessWPF.Models;
using System.Windows;
using ChessWPF.Commands;

namespace ChessWPF.ViewModels
{
    public class SettingsViewModel : NotifyPropertyChanged
    {
        private ColorScheme selectedColorScheme;
        private Color customLightColor;
        private Color customDarkColor;
        private bool isUpdatingCustomScheme = false;
        private bool isUpdatingFromSelection = false;

        public ObservableCollection<ColorScheme> ColorSchemes { get; set; }

        public ColorScheme SelectedColorScheme
        {
            get => selectedColorScheme;
            set
            {
                if (selectedColorScheme == value) return;
                
                selectedColorScheme = value;
                OnPropertyChanged();
                
                // If custom scheme is selected, update custom color properties
                if (value?.Name == "Custom" && !isUpdatingCustomScheme)
                {
                    isUpdatingFromSelection = true;
                    try
                    {
                        if (value.LightSquareColor is SolidColorBrush lightBrush)
                        {
                            customLightColor = lightBrush.Color;
                            OnPropertyChanged(nameof(CustomLightColor));
                        }
                        if (value.DarkSquareColor is SolidColorBrush darkBrush)
                        {
                            customDarkColor = darkBrush.Color;
                            OnPropertyChanged(nameof(CustomDarkColor));
                        }
                    }
                    finally
                    {
                        isUpdatingFromSelection = false;
                    }
                }
                
                // Notify immediately when selection changes
                OnColorSchemeChanged?.Invoke(value);
            }
        }

        public Color CustomLightColor
        {
            get => customLightColor;
            set
            {
                if (customLightColor == value) return;
                
                customLightColor = value;
                OnPropertyChanged();
                
                if (!isUpdatingFromSelection)
                {
                    UpdateCustomColorScheme();
                }
            }
        }

        public Color CustomDarkColor
        {
            get => customDarkColor;
            set
            {
                if (customDarkColor == value) return;
                
                customDarkColor = value;
                OnPropertyChanged();
                
                if (!isUpdatingFromSelection)
                {
                    UpdateCustomColorScheme();
                }
            }
        }

        public ICommand SelectCustomLightColorCommand { get; }
        public ICommand SelectCustomDarkColorCommand { get; }

        public event System.Action<ColorScheme> OnColorSchemeChanged;

        public SettingsViewModel()
        {
            // Initialize custom colors with default values first
            CustomLightColor = Color.FromRgb(240, 217, 181);
            CustomDarkColor = Color.FromRgb(181, 136, 99);
            
            InitializeColorSchemes();
            
            // Add custom scheme after initializing others
            UpdateCustomColorScheme();
            
            SelectCustomLightColorCommand = new RelayCommand(_ => SelectCustomColor(true));
            SelectCustomDarkColorCommand = new RelayCommand(_ => SelectCustomColor(false));
        }

        private void SelectCustomColor(bool isLightColor)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = isLightColor ? 
                System.Drawing.Color.FromArgb(CustomLightColor.R, CustomLightColor.G, CustomLightColor.B) :
                System.Drawing.Color.FromArgb(CustomDarkColor.R, CustomDarkColor.G, CustomDarkColor.B);
            
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedColor = colorDialog.Color;
                var wpfColor = Color.FromRgb(selectedColor.R, selectedColor.G, selectedColor.B);
                
                if (isLightColor)
                {
                    CustomLightColor = wpfColor;
                }
                else
                {
                    CustomDarkColor = wpfColor;
                }
            }
        }

        private void UpdateCustomColorScheme()
        {
            if (isUpdatingCustomScheme || ColorSchemes == null) return;
            
            isUpdatingCustomScheme = true;
            try
            {
                // Find or create custom color scheme
                var customScheme = ColorSchemes.FirstOrDefault(s => s.Name == "Custom");
                if (customScheme == null)
                {
                    customScheme = new ColorScheme(
                        "Custom",
                        "Your personalized color scheme",
                        new SolidColorBrush(CustomLightColor),
                        new SolidColorBrush(CustomDarkColor),
                        "🎨"
                    );
                    ColorSchemes.Add(customScheme);
                }
                else
                {
                    customScheme.LightSquareColor = new SolidColorBrush(CustomLightColor);
                    customScheme.DarkSquareColor = new SolidColorBrush(CustomDarkColor);
                }
                
                // Select custom scheme if colors are being edited (but avoid recursion)
                if (SelectedColorScheme?.Name != "Custom")
                {
                    selectedColorScheme = customScheme;
                    OnPropertyChanged(nameof(SelectedColorScheme));
                    OnColorSchemeChanged?.Invoke(customScheme);
                }
            }
            finally
            {
                isUpdatingCustomScheme = false;
            }
        }

        private void InitializeColorSchemes()
        {
            ColorSchemes = new ObservableCollection<ColorScheme>
            {
                new ColorScheme(
                    "Classic Ivory",
                    "Traditional chess board with warm ivory and sandy brown",
                    new SolidColorBrush(Color.FromRgb(240, 217, 181)), // Bisque
                    new SolidColorBrush(Color.FromRgb(181, 136, 99)),  // SandyBrown
                    "♟️"
                ),
                new ColorScheme(
                    "Midnight Forest",
                    "Dark and mysterious like a moonlit forest",
                    new SolidColorBrush(Color.FromRgb(118, 150, 86)),  // Forest green light
                    new SolidColorBrush(Color.FromRgb(56, 84, 44)),    // Forest green dark
                    "🌲"
                ),
                new ColorScheme(
                    "Ocean Depths",
                    "Cool and calming like the deep ocean",
                    new SolidColorBrush(Color.FromRgb(173, 216, 230)), // Light blue
                    new SolidColorBrush(Color.FromRgb(70, 130, 180)),  // Steel blue
                    "🌊"
                ),
                new ColorScheme(
                    "Cherry Blossom",
                    "Elegant pink tones inspired by spring",
                    new SolidColorBrush(Color.FromRgb(255, 228, 225)), // Misty rose
                    new SolidColorBrush(Color.FromRgb(255, 182, 193)), // Light pink
                    "🌸"
                ),
                new ColorScheme(
                    "Royal Purple",
                    "Regal and majestic purple tones",
                    new SolidColorBrush(Color.FromRgb(221, 160, 221)), // Plum
                    new SolidColorBrush(Color.FromRgb(138, 43, 226)),  // Blue violet
                    "👑"
                ),
                new ColorScheme(
                    "Desert Sand",
                    "Warm earth tones like a desert sunset",
                    new SolidColorBrush(Color.FromRgb(245, 222, 179)), // Wheat
                    new SolidColorBrush(Color.FromRgb(210, 180, 140)), // Tan
                    "🏜️"
                ),
                new ColorScheme(
                    "Arctic Ice",
                    "Cool and crisp like fresh snow",
                    new SolidColorBrush(Color.FromRgb(240, 248, 255)), // Alice blue
                    new SolidColorBrush(Color.FromRgb(176, 196, 222)), // Light steel blue
                    "❄️"
                ),
                new ColorScheme(
                    "Golden Hour",
                    "Warm golden tones like sunset",
                    new SolidColorBrush(Color.FromRgb(255, 248, 220)), // Cornsilk
                    new SolidColorBrush(Color.FromRgb(218, 165, 32)),  // Goldenrod
                    "🌅"
                )
            };

            // Set default to Classic Ivory
            SelectedColorScheme = ColorSchemes.First();
        }
    }
}

