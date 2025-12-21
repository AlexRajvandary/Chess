using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable
namespace ChessWPF.Models
{
    /// <summary>
    /// Represents a historical chess game
    /// </summary>
    public class HistoricalGame
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string WhitePlayer { get; set; } = "White";

        [Required]
        [MaxLength(200)]
        public string BlackPlayer { get; set; } = "Black";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PlayedAt { get; set; }

        [Required]
        [Column(TypeName = "TEXT")]
        public string PgnNotation { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Event { get; set; }

        [MaxLength(1000)]
        public string Site { get; set; }

        [MaxLength(50)]
        public string Result { get; set; }

        [MaxLength(50)]
        public string Round { get; set; }

        [Column(TypeName = "TEXT")]
        public string InitialFen { get; set; }

        [Column(TypeName = "TEXT")]
        public string FinalFen { get; set; }

        public int MoveCount { get; set; }
    }
}


