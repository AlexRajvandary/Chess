using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ChessLib.Tools
{
    public static class CustomTraceLogger
    {
        private static int _counter;

        public static void TraceCaller(
            string message,
            int skipFrames = 1,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
#if DEBUG
            _counter++;

            // skipFrames=1 пропускает сам TraceCaller
            var st = new StackTrace(skipFrames, fNeedFileInfo: true);
            var frames = st.GetFrames();

            string caller = "<unknown>";
            if (frames != null)
            {
                // Ищем первый кадр НЕ из Pawn и НЕ из системных/ WPF сборок
                foreach (var f in frames)
                {
                    var m = f.GetMethod();
                    var dt = m?.DeclaringType;
                    var typeName = dt?.FullName ?? "";
                    var methodName = m?.Name ?? "";

                    if (string.IsNullOrWhiteSpace(typeName))
                        continue;

                    // отсекаем инфраструктуру
                    if (typeName.StartsWith("System.", StringComparison.Ordinal) ||
                        typeName.StartsWith("Microsoft.", StringComparison.Ordinal) ||
                        typeName.StartsWith("MS.", StringComparison.Ordinal) ||
                        typeName.StartsWith("PresentationFramework", StringComparison.Ordinal) ||
                        typeName.StartsWith("WindowsBase", StringComparison.Ordinal))
                        continue;

                    // отсекаем сам класс Pawn (чтобы найти реального вызвавшего)
                    if (typeName.Contains(".Pawn", StringComparison.Ordinal) || typeName.EndsWith("Pawn", StringComparison.Ordinal))
                        continue;

                    caller = $"{typeName}.{methodName}";
                    break;
                }
            }

            Trace.WriteLine($"{Path.GetFileName(file)}:{line} #{_counter} caller={caller} → {message}");
#endif
        }

        public static void TraceStack(
            string message,
            int take = 12,
            int skipFrames = 1,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
#if DEBUG
            _counter++;
            var st = new StackTrace(skipFrames, fNeedFileInfo: true);
            var frames = st.GetFrames() ?? Array.Empty<StackFrame>();

            var lines = frames
                .Select(f => f.GetMethod())
                .Where(m => m != null)
                .Select(m => $"{m!.DeclaringType?.FullName}.{m.Name}")
                .Take(take);

            Trace.WriteLine($"{Path.GetFileName(file)}:{line} #{_counter} stack:\n  {string.Join("\n  ", lines)}\n  → {message}");
#endif
        }
    }
}