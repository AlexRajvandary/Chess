using System;
using System.Media;
using System.Windows.Media;
using ChessLib;
using System.Threading.Tasks;

namespace ChessWPF.Services
{
    /// <summary>
    /// Service for playing sounds during chess moves
    /// </summary>
    public class SoundService
    {
        public SoundService()
        {
        }

        /// <summary>
        /// Plays sound for a move based on move type
        /// </summary>
        public void PlayMoveSound(MoveResult moveResult)
        {
            try
            {
                switch (moveResult.MoveType)
                {
                    case MoveType.Capture:
                        PlayCaptureSound();
                        break;
                    case MoveType.Castle:
                        PlayCastleSound();
                        break;
                    case MoveType.Normal:
                    case MoveType.EnPassant:
                    case MoveType.Promotion:
                    default:
                        PlayMoveSound();
                        break;
                }

                // Play check sound if applicable
                if (moveResult.IsCheck)
                {
                    // Small delay to play after move sound
                    System.Threading.Tasks.Task.Delay(150).ContinueWith(_ => PlayCheckSound());
                }
            }
            catch
            {
                // Silently fail if sound cannot be played
            }
        }

        private void PlayMoveSound()
        {
            try
            {
                // Characteristic chess move sound: low, soft click (like a piece on wood)
                // Frequency: 200 Hz (low, wooden sound), Duration: 50ms
                Task.Run(() =>
                {
                    Console.Beep(200, 50);
                });
            }
            catch { }
        }

        private void PlayCaptureSound()
        {
            try
            {
                // Capture sound: sharper, more pronounced (like taking a piece)
                // Frequency: 400 Hz (medium), Duration: 80ms
                Task.Run(() =>
                {
                    Console.Beep(400, 80);
                });
            }
            catch { }
        }

        private void PlayCastleSound()
        {
            try
            {
                // Castling sound: two quick sounds (king and rook moving)
                Task.Run(() =>
                {
                    Console.Beep(250, 40);
                    Task.Delay(30).Wait();
                    Console.Beep(250, 40);
                });
            }
            catch { }
        }

        private void PlayCheckSound()
        {
            try
            {
                // Check sound: higher, alerting tone
                // Frequency: 600 Hz (higher, alerting), Duration: 100ms
                Task.Run(() =>
                {
                    Console.Beep(600, 100);
                });
            }
            catch { }
        }

        /// <summary>
        /// Plays checkmate sound
        /// </summary>
        public void PlayCheckmateSound()
        {
            try
            {
                // Checkmate sound: triumphant sequence (low to high)
                Task.Run(() =>
                {
                    Console.Beep(300, 100);
                    Task.Delay(50).Wait();
                    Console.Beep(400, 100);
                    Task.Delay(50).Wait();
                    Console.Beep(500, 150);
                });
            }
            catch { }
        }
    }
}

