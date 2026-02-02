using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.WhsiperAIDemo
{
    /// <summary>
    /// TextWriter that tees console output to a log file while retaining console output.
    /// Use ConsoleTee.Enable(logPath) to activate and dispose the returned IDisposable when done.
    /// </summary>
    internal sealed class ConsoleTee : TextWriter, IDisposable
    {
        private readonly TextWriter _originalOut;
        private readonly TextWriter _originalError;
        private readonly StreamWriter _fileWriter;
        private readonly object _sync = new object();
        private bool _disposed;

        // Private constructor that replaces Console.Out and Console.Error with this instance
        private ConsoleTee(string logPath)
        {
            _originalOut = Console.Out;
            _originalError = Console.Error;

            _fileWriter = new StreamWriter(new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                AutoFlush = true,
                NewLine = Environment.NewLine
            };

            // Redirect console output/error to this TextWriter so both console and file get messages
            Console.SetOut(this);
            Console.SetError(this);
        }

        // Public helper to enable teeing. Caller should hold onto the returned instance to Dispose when done.
        public static ConsoleTee Enable(string logPath) => new ConsoleTee(logPath);

        public override Encoding Encoding => _originalOut?.Encoding ?? Encoding.UTF8;

        // All write operations lock on a sync object to ensure atomic writes to both console and file.
        public override void Write(char value)
        {
            lock (_sync)
            {
                _originalOut?.Write(value);
                _fileWriter?.Write(value);
            }
        }

        public override void Write(string? value)
        {
            lock (_sync)
            {
                _originalOut?.Write(value);
                _fileWriter?.Write(value);
            }
        }

        public override void WriteLine()
        {
            lock (_sync)
            {
                _originalOut?.WriteLine();
                _fileWriter?.WriteLine();
            }
        }

        public override void WriteLine(string? value)
        {
            lock (_sync)
            {
                _originalOut?.WriteLine(value);
                _fileWriter?.WriteLine(value);
            }
        }

        public override void Flush()
        {
            lock (_sync)
            {
                _originalOut?.Flush();
                _fileWriter?.Flush();
            }
        }

        // Restore the original console outputs and dispose the file writer (best-effort)
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                if (_originalOut != null) Console.SetOut(_originalOut);
                if (_originalError != null) Console.SetError(_originalError);
            }
            catch
            {
                // ignore
            }

            try
            {
                _fileWriter?.Flush();
                _fileWriter?.Dispose();
            }
            catch
            {
                // ignore
            }
        }
    }
}
