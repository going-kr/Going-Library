using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Going.UIEditor.Utils;
using Microsoft.Web.WebView2.WinForms;
using Pty.Net;

namespace Going.UIEditor.Windows
{
    public partial class PromptWindow : xWindow
    {
        private WebView2 webView;
        private IPtyConnection ptyConnection;
        private bool ready;
        private readonly StringBuilder outputBuffer = new();
        private System.Windows.Forms.Timer flushTimer;

        public PromptWindow()
        {
            InitializeComponent();

            webView = new WebView2 { Dock = DockStyle.Fill };
            pnl.Controls.Add(webView);

            flushTimer = new System.Windows.Forms.Timer { Interval = 16 }; // ~60fps
            flushTimer.Tick += (s, e) => FlushOutput();
            flushTimer.Start();

            Load += async (s, e) => await InitAsync();
            FormClosing += (s, e) => Cleanup();

            Title = "Claude";
            TitleIconString = "fa-terminal";
            TitleIconSize = 16;
        }

        protected override void OnLoad(EventArgs e)
        {
            Title = LM.Claude;
            base.OnLoad(e);
        }

        private async Task InitAsync()
        {
            await webView.EnsureCoreWebView2Async();

            var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "terminal.html");
            if (File.Exists(htmlPath))
                webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);

            // xterm.js → C# (raw key input or resize)
            webView.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                var input = e.TryGetWebMessageAsString();
                if (input == null || ptyConnection == null) return;

                // Check if resize message
                if (input.StartsWith("{"))
                {
                    try
                    {
                        var msg = JsonSerializer.Deserialize<JsonElement>(input);
                        if (msg.TryGetProperty("type", out var type) && type.GetString() == "resize")
                        {
                            var cols = msg.GetProperty("cols").GetInt32();
                            var rows = msg.GetProperty("rows").GetInt32();
                            ptyConnection.Resize(cols, rows);
                            return;
                        }
                    }
                    catch { }
                }

                var bytes = Encoding.UTF8.GetBytes(input);
                ptyConnection.WriterStream.Write(bytes, 0, bytes.Length);
                ptyConnection.WriterStream.Flush();
            };

            webView.NavigationCompleted += async (s, e) =>
            {
                if (!ready)
                {
                    ready = true;
                    await StartTerminal();
                }
            };
        }

        private async Task StartTerminal()
        {
            var dir = Program.FilePath != null ? Path.GetDirectoryName(Program.FilePath) : Environment.CurrentDirectory;

            try
            {
                if (!Program.ClaudeInstalled || Program.ClaudePath == null) return;

                var systemPrompt = "본 세션은 going-ui-skill을 이용하여 개발하는 HMI 프로그램이다";
                var options = new PtyOptions
                {
                    Name = "GoingTerminal",
                    App = Program.ClaudePath,
                    CommandLine = new[] { "--append-system-prompt", systemPrompt },
                    Cols = 120,
                    Rows = 30,
                    Cwd = dir
                };

                ptyConnection = await PtyProvider.SpawnAsync(options, default);

                // Read PTY output → xterm.js
                _ = Task.Run(async () =>
                {
                    var buffer = new byte[4096];
                    try
                    {
                        while (true)
                        {
                            int bytesRead = await ptyConnection.ReaderStream.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead <= 0) break;

                            var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            WriteToXterm(text);
                        }
                    }
                    catch { }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"터미널 실행 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WriteToXterm(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            lock (outputBuffer)
            {
                outputBuffer.Append(data);
            }
        }

        private void FlushOutput()
        {
            string data;
            lock (outputBuffer)
            {
                if (outputBuffer.Length == 0) return;
                data = outputBuffer.ToString();
                outputBuffer.Clear();
            }

            if (webView?.CoreWebView2 == null) return;
            var escaped = JsonSerializer.Serialize(data);
            webView.CoreWebView2.ExecuteScriptAsync($"writeToTerminal({escaped})");
        }

        private void Cleanup()
        {
            flushTimer?.Stop();
            flushTimer?.Dispose();
            try { ptyConnection?.Dispose(); } catch { }
            ptyConnection = null;
        }
    }
}
