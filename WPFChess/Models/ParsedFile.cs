using System;
using System.ComponentModel.DataAnnotations;

#nullable disable
namespace ChessWPF.Models
{
    /// <summary>
    /// Represents a PGN file that has been parsed and imported into the database
    /// </summary>
    public class ParsedFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string FileName { get; set; }

        [Required]
        public DateTime ParsedAt { get; set; } = DateTime.Now;

        public int GamesImported { get; set; }
    }
}


