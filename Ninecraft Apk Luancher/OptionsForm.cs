using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NinecraftApkLauncher
{
    public class OptionsForm : Form
    {
        private string filePath;
        private TableLayoutPanel table;
        private Button saveButton;
        private Button cancelButton;

        private Dictionary<string, Control> controlMap = new Dictionary<string, Control>();

        public OptionsForm(string path)
        {
            filePath = path;
            InitializeComponents();
            LoadOptions();
        }

        private void InitializeComponents()
        {
            this.Text = "Edit Options";
            this.ClientSize = new System.Drawing.Size(500, 415);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoScroll = true,
                ColumnCount = 2,
                RowCount = 0,
                Padding = new Padding(10),
                AutoSize = true,
            };

            this.Controls.Add(table);

            saveButton = new Button()
            {
                Text = "Save",
                Width = 100,
                Anchor = AnchorStyles.Right
            };
            saveButton.Click += SaveButton_Click;

            cancelButton = new Button()
            {
                Text = "Cancel",
                Width = 100,
                Anchor = AnchorStyles.Right
            };
            cancelButton.Click += (s, e) => this.Close();

            var buttonPanel = new FlowLayoutPanel()
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10),
            };
            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(saveButton);
            this.Controls.Add(buttonPanel);
        }

        private void LoadOptions()
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                int row = 0;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(":"))
                        continue;

                    var parts = line.Split(new[] { ':' }, 2);
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    Label label = new Label()
                    {
                        Text = key,
                        AutoSize = true,
                        Anchor = AnchorStyles.Left,
                        Padding = new Padding(0, 6, 0, 0)
                    };

                    Control inputControl;

                    if (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        inputControl = new CheckBox()
                        {
                            Checked = bool.Parse(value),
                            Anchor = AnchorStyles.Left
                        };
                    }
                    else
                    {
                        inputControl = new TextBox()
                        {
                            Text = value,
                            Width = 250
                        };
                    }

                    controlMap[key] = inputControl;

                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.Controls.Add(label, 0, row);
                    table.Controls.Add(inputControl, 1, row);
                    row++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load options.txt:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                var lines = controlMap.Select(kvp =>
                {
                    string key = kvp.Key;
                    Control control = kvp.Value;
                    string value;

                    if (control is CheckBox cb)
                        value = cb.Checked.ToString().ToLower();
                    else
                        value = control.Text.Trim();

                    return $"{key}:{value}";
                });

                File.WriteAllLines(filePath, lines);
                MessageBox.Show("Options saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save options.txt:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
