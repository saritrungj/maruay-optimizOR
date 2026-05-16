using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using optimizOR.Models;

namespace optimizOR.Core
{
    public class Logger
    {
        private readonly RichTextBox _rtbLog;

        public Logger(RichTextBox rtbLog)
        {
            if (rtbLog == null)
            {
                throw new ArgumentNullException("rtbLog");
            }

            _rtbLog = rtbLog;
        }

        public void Log(string message, LogLevel level)
        {
            string formatted = string.Format("[{0:HH:mm:ss}] [{1,-7}] {2}{3}",
                DateTime.Now,
                level.ToString().ToUpperInvariant(),
                message,
                Environment.NewLine);
            Color color = GetColor(level);

            if (_rtbLog.InvokeRequired)
            {
                _rtbLog.Invoke(new Action(() => AppendColored(formatted, color)));
            }
            else
            {
                AppendColored(formatted, color);
            }
        }

        public void Clear()
        {
            if (_rtbLog.InvokeRequired)
            {
                _rtbLog.Invoke(new Action(Clear));
                return;
            }

            _rtbLog.Clear();
        }

        public void Export(string filePath)
        {
            if (_rtbLog.InvokeRequired)
            {
                _rtbLog.Invoke(new Action(() => Export(filePath)));
                return;
            }

            File.WriteAllText(filePath, _rtbLog.Text);
        }

        private static Color GetColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Success:
                    return ColorTranslator.FromHtml("#39FF14");
                case LogLevel.Warning:
                    return ColorTranslator.FromHtml("#FFD700");
                case LogLevel.Error:
                    return ColorTranslator.FromHtml("#FF4444");
                default:
                    return ColorTranslator.FromHtml("#B0B0B0");
            }
        }

        private void AppendColored(string text, Color color)
        {
            _rtbLog.SelectionStart = _rtbLog.TextLength;
            _rtbLog.SelectionLength = 0;
            _rtbLog.SelectionColor = color;
            _rtbLog.AppendText(text);
            _rtbLog.SelectionColor = _rtbLog.ForeColor;
            _rtbLog.ScrollToCaret();
        }
    }
}

