using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeyQuery
{
    public partial class SettingsForm : Form
    {
        private readonly ApiConfig _config;
        private TextBox txtBaseUrl;
        private TextBox txtApiKey;
        private NumericUpDown numTimeout;
        private NumericUpDown numRetries;
        private NumericUpDown numRetryDelay;
        private Button btnTest;
        private Button btnSave;
        private Button btnCancel;

        public SettingsForm(ApiConfig config)
        {
            _config = config;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "API设置";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 创建控件
            CreateControls();
            BindEvents();
        }

        private void CreateControls()
        {
            // 标题
            var lblTitle = new Label
            {
                Text = "硅基流动API设置",
                Location = new Point(20, 20),
                Size = new Size(200, 25),
                Font = new Font("微软雅黑", 12, FontStyle.Bold)
            };

            // API基础URL
            var lblBaseUrl = new Label
            {
                Text = "API基础URL:",
                Location = new Point(20, 60),
                Size = new Size(100, 25),
                Font = new Font("微软雅黑", 10)
            };

            txtBaseUrl = new TextBox
            {
                Location = new Point(130, 60),
                Size = new Size(330, 25),
                Font = new Font("微软雅黑", 9)
            };

            // API密钥
            var lblApiKey = new Label
            {
                Text = "API密钥:",
                Location = new Point(20, 100),
                Size = new Size(100, 25),
                Font = new Font("微软雅黑", 10)
            };

            txtApiKey = new TextBox
            {
                Location = new Point(130, 100),
                Size = new Size(330, 25),
                Font = new Font("微软雅黑", 9),
                UseSystemPasswordChar = true
            };

            // 超时时间
            var lblTimeout = new Label
            {
                Text = "超时时间(秒):",
                Location = new Point(20, 140),
                Size = new Size(100, 25),
                Font = new Font("微软雅黑", 10)
            };

            numTimeout = new NumericUpDown
            {
                Location = new Point(130, 140),
                Size = new Size(100, 25),
                Minimum = 5,
                Maximum = 300,
                Value = 30
            };

            // 重试次数
            var lblRetries = new Label
            {
                Text = "重试次数:",
                Location = new Point(20, 180),
                Size = new Size(100, 25),
                Font = new Font("微软雅黑", 10)
            };

            numRetries = new NumericUpDown
            {
                Location = new Point(130, 180),
                Size = new Size(100, 25),
                Minimum = 0,
                Maximum = 10,
                Value = 3
            };

            // 重试延迟
            var lblRetryDelay = new Label
            {
                Text = "重试延迟(毫秒):",
                Location = new Point(20, 220),
                Size = new Size(100, 25),
                Font = new Font("微软雅黑", 10)
            };

            numRetryDelay = new NumericUpDown
            {
                Location = new Point(130, 220),
                Size = new Size(100, 25),
                Minimum = 100,
                Maximum = 10000,
                Value = 1000,
                Increment = 100
            };

            // 测试连接按钮
            btnTest = new Button
            {
                Text = "测试连接",
                Location = new Point(130, 260),
                Size = new Size(100, 35),
                Font = new Font("微软雅黑", 10),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // 保存按钮
            btnSave = new Button
            {
                Text = "保存",
                Location = new Point(280, 310),
                Size = new Size(80, 35),
                Font = new Font("微软雅黑", 10),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            // 取消按钮
            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(380, 310),
                Size = new Size(80, 35),
                Font = new Font("微软雅黑", 10),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };

            // 添加控件
            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblBaseUrl, txtBaseUrl, lblApiKey, txtApiKey,
                lblTimeout, numTimeout, lblRetries, numRetries,
                lblRetryDelay, numRetryDelay, btnTest, btnSave, btnCancel
            });
        }

        private void BindEvents()
        {
            btnTest.Click += BtnTest_Click;
            btnSave.Click += BtnSave_Click;
        }

        private void LoadSettings()
        {
            txtBaseUrl.Text = _config.BaseUrl;
            txtApiKey.Text = _config.ApiKey;
            numTimeout.Value = _config.TimeoutSeconds;
            numRetries.Value = _config.MaxRetries;
            numRetryDelay.Value = _config.RetryDelayMs;
        }

        private async void BtnTest_Click(object sender, EventArgs e)
        {
            btnTest.Enabled = false;
            btnTest.Text = "测试中...";

            try
            {
                var testConfig = new ApiConfig
                {
                    BaseUrl = txtBaseUrl.Text.Trim(),
                    ApiKey = txtApiKey.Text.Trim(),
                    TimeoutSeconds = (int)numTimeout.Value,
                    MaxRetries = (int)numRetries.Value,
                    RetryDelayMs = (int)numRetryDelay.Value
                };

                using (var apiService = new ApiService(testConfig))
                {
                    var isConnected = await apiService.TestConnectionAsync();
                    
                    if (isConnected)
                    {
                        MessageBox.Show("连接测试成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("连接测试失败，请检查API设置。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"连接测试失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTest.Enabled = true;
                btnTest.Text = "测试连接";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(txtBaseUrl.Text))
            {
                MessageBox.Show("请输入API基础URL", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBaseUrl.Focus();
                return;
            }

            // 保存设置
            _config.BaseUrl = txtBaseUrl.Text.Trim();
            _config.ApiKey = txtApiKey.Text.Trim();
            _config.TimeoutSeconds = (int)numTimeout.Value;
            _config.MaxRetries = (int)numRetries.Value;
            _config.RetryDelayMs = (int)numRetryDelay.Value;

            _config.Save();
        }
    }
} 