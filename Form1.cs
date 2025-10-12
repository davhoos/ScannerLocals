using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ScannerLocal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeListView();
        }

        // Initialize ListView with appropriate columns
        private void InitializeListView()
        {
            listView1.View = View.Details;
            listView1.Columns.Add("Protocol", 70, HorizontalAlignment.Left);
            listView1.Columns.Add("Local Address", 150, HorizontalAlignment.Left);
            listView1.Columns.Add("Foreign Address", 150, HorizontalAlignment.Left);
            listView1.Columns.Add("State", 100, HorizontalAlignment.Left);
            listView1.Columns.Add("PID", 50, HorizontalAlignment.Left);
            listView1.Columns.Add("NAME", 150, HorizontalAlignment.Left);
        }

        // Start the netstat process
        private void RunNetstat()
        {
            Process cmd = new Process();

            cmd.StartInfo.FileName = "netstat.exe";
            cmd.StartInfo.Arguments = "-abo";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            // Attach event handler for output
            cmd.OutputDataReceived += Cmd_OutputDataReceived;
            cmd.ErrorDataReceived += Cmd_ErrorDataReceived;

            // Start the process and begin reading output
            cmd.Start();
            cmd.BeginOutputReadLine();
            cmd.BeginErrorReadLine();
        }

        // Event handler for capturing standard output
        private void Cmd_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                // Skip the header lines (first few lines of netstat output)
                if (e.Data.Contains("Proto") || e.Data.Contains("Active") || string.IsNullOrWhiteSpace(e.Data))
                    return;

                // Parse the output (use appropriate space delimiters)
                var tokens = e.Data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Ensure tokens are enough to fill the columns
                if (tokens.Length >= 5) // Adjusted to avoid index out of range errors
                {
                    string protocol = tokens[0];
                    string localAddress = tokens[1];
                    string foreignAddress = tokens[2];
                    string state = tokens[3];
                    string pidString = tokens[4];
                    string processName = "Unknown";  // Default if process name is not found

                    // Convert PID to integer and fetch the process name
                    if (int.TryParse(pidString, out int pid))
                    {
                        try
                        {
                            Process process = Process.GetProcessById(pid);
                            processName = process.ProcessName; // Get the process name
                        }
                        catch (Exception)
                        {
                            // Handle cases where the process might no longer exist or access is denied
                            processName = "Access Denied or Not Available";
                        }
                    }

                    // Only show the row if it's ESTABLISHED or other specific states (optional)
                    if (state.Equals("ESTABLISHED", StringComparison.OrdinalIgnoreCase)) // || state.Equals("LISTENING", StringComparison.OrdinalIgnoreCase))
                    {
                        // Invoke the update to the ListView on the UI thread
                        this.Invoke((MethodInvoker)delegate
                        {
                            // Create a new ListViewItem and assign the columns
                            ListViewItem item = new ListViewItem(protocol);        // Protocol
                            item.SubItems.Add(localAddress);                        // Local Address
                            item.SubItems.Add(foreignAddress);                      // Foreign Address
                            item.SubItems.Add(state);                               // State
                            item.SubItems.Add(pidString);                           // PID
                            item.SubItems.Add(processName);                         // Process Name

                            // Add the item to the ListView
                            listView1.Items.Add(item);
                            listView1.FullRowSelect = true; // Select full row
                        });
                    }
                }
            }
        }

        // Event handler for capturing standard error (if any)
        private void Cmd_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.Invoke((MethodInvoker)delegate
                {
                    // Handle errors (optional)
                    ListViewItem item = new ListViewItem("Error: " + e.Data);
                    listView1.Items.Add(item);
                });
            }
        }

        // Button click event to trigger the process
        private void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear(); // Clear the ListView before running
            RunNetstat(); // Run netstat and capture output
        }

        // Placeholder for button2 functionality (optional, could be removed if not needed)
        private void button2_Click(object sender, EventArgs e)
        {
            Process cmd = new Process();

            cmd.StartInfo.FileName = "nslookup.exe";
            cmd.StartInfo.Arguments = "";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            cmd.Start();
        }
    }
}
