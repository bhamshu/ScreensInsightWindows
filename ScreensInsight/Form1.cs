using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Timers; // This is the correct namespace for System.Timers.Timer
using System.Windows.Forms;

namespace ScreenSight
{
    public partial class Form1 : Form
    {
        private System.Timers.Timer captureTimer; // Specify the full namespace to resolve ambiguity
        private bool isCapturing = true; // Start capturing right away

        public Form1()
        {
            InitializeComponent();
            captureTimer = new System.Timers.Timer(); // Specify the full namespace to resolve ambiguity
            captureTimer.Interval = 60*1000; // 1 minute
            captureTimer.Elapsed += CaptureScreen;
            captureTimer.Start(); // Start the timer right away
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "Status: Capturing";
            // Ensure the button click event handler is connected
            btnStartStop.Click += btnStartStop_Click;
        }

        private void btnStartStop_Click(object? sender, EventArgs e)
        {
            if (isCapturing)
            {
                captureTimer.Stop();
                btnStartStop.Text = "Start"; // Update button label to "Start"
                lblStatus.Text = "Status: Stopped";
            }
            else
            {
                captureTimer.Start();
                btnStartStop.Text = "Stop"; // Update button label to "Stop"
                lblStatus.Text = "Status: Capturing";
            }
            isCapturing = !isCapturing;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                // Keep capturing even after the form is closed
            }
            base.OnFormClosing(e);
        }

        private void CaptureScreen(object? sender, ElapsedEventArgs e)
        {
            try
            {
                // Check if the primary screen is available
                if (Screen.PrimaryScreen == null)
                {
                    Invoke(new Action(() => lblStatus.Text = "Error: No primary screen available."));
                    ResetButtonAndStatus();
                    return;
                }

                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // Try to perform the screen capture
                        try
                        {
                            g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                        }
                        catch (Exception ex)
                        {
                            // Handle specific GDI+ errors
                            Invoke(new Action(() => lblStatus.Text = $"Error capturing screen: {ex.Message}"));
                            ResetButtonAndStatus();
                            return;
                        }
                    }

                    string fileName = $"screencapture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    string directoryPath = Path.Combine(Path.GetTempPath(), "screensight");
                    Directory.CreateDirectory(directoryPath); // Ensure the directory exists
                    string filePath = Path.Combine(directoryPath, fileName);

                    // Save the bitmap to file
                    try
                    {
                        bitmap.Save(filePath, ImageFormat.Png);
                        Invoke(new Action(() => lblStatus.Text = $"Status: Screen capture saved to {filePath}"));
                    }
                    catch (Exception ex)
                    {
                        // Handle save errors
                        Invoke(new Action(() => lblStatus.Text = $"Error saving screen capture: {ex.Message}"));
                        ResetButtonAndStatus();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Invoke(new Action(() => lblStatus.Text = $"Unexpected error: {ex.Message}"));
                ResetButtonAndStatus();
            }
        }

        private void ResetButtonAndStatus()
        {
            Invoke(new Action(() =>
            {
                btnStartStop.Text = "Start";
                isCapturing = false;
            }));
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {
        }
    }
}
