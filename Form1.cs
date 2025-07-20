using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyQuery
{
    public partial class Form1 : Form
    {
        private ApiService _apiService;
        private readonly ApiConfig _config;
        private bool _isQuerying = false;

        public Form1()
        {
            InitializeComponent();
            
            // 加载配置
            _config = ApiConfig.Load();
            _apiService = new ApiService(_config);
            
            // 设置窗体标题
            this.Text = "硅基流动Key余额批量查询工具";
            
            // 初始化界面
            InitializeUI();
        }

        private void InitializeUI()
        {
            // 设置窗体大小
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 创建控件
            CreateControls();
            
            // 绑定事件
            BindEvents();
        }

        private void CreateControls()
        {
            var lblInput = new Label
            {
                Text = "请输入API Key（每行一个）：",
                Location = new Point(20, 20),
                Size = new Size(200, 25),
                Font = new Font("微软雅黑", 10)
            };

            txtAddresses = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(450, 200),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9),
                PlaceholderText = "请输入API Key，每行一个\n例如：\nsk-1234567890abcdef...\nsk-abcdef1234567890..."
            };

            // 按钮区域
            btnQuery = new Button
            {
                Text = "开始查询",
                Location = new Point(20, 270),
                Size = new Size(100, 35),
                Font = new Font("微软雅黑", 10),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnClear = new Button
            {
                Text = "清空",
                Location = new Point(140, 270),
                Size = new Size(80, 35),
                Font = new Font("微软雅黑", 10),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnExport = new Button
            {
                Text = "导出结果",
                Location = new Point(240, 270),
                Size = new Size(100, 35),
                Font = new Font("微软雅黑", 10),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            btnSettings = new Button
            {
                Text = "设置",
                Location = new Point(360, 270),
                Size = new Size(80, 35),
                Font = new Font("微软雅黑", 10),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };

            // 进度条
            progressBar = new ProgressBar
            {
                Location = new Point(460, 275),
                Size = new Size(200, 25),
                Visible = false
            };

            lblProgress = new Label
            {
                Text = "",
                Location = new Point(670, 275),
                Size = new Size(200, 25),
                Font = new Font("微软雅黑", 9),
                ForeColor = Color.Gray
            };

            // 结果显示区域
            var lblResult = new Label
            {
                Text = "查询结果：",
                Location = new Point(20, 320),
                Size = new Size(200, 25),
                Font = new Font("微软雅黑", 10)
            };

            dataGridView = new DataGridView
            {
                Location = new Point(20, 350),
                Size = new Size(950, 280),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new Font("微软雅黑", 9)
            };

            // 状态栏
            statusLabel = new Label
            {
                Text = "就绪",
                Location = new Point(20, 640),
                Size = new Size(400, 20),
                Font = new Font("微软雅黑", 9),
                ForeColor = Color.Gray
            };

            // 作者信息
            var authorLabel = new Label
            {
                Text = "by: n5012346@52pojie",
                Location = new Point(this.ClientSize.Width - 200, 640),
                Size = new Size(180, 20),
                Font = new Font("微软雅黑", 9),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            // 添加控件到窗体
            this.Controls.AddRange(new Control[]
            {
                lblInput, txtAddresses, btnQuery, btnClear, btnExport, btnSettings,
                progressBar, lblProgress, lblResult, dataGridView, statusLabel, authorLabel
            });
        }

        private void BindEvents()
        {
            btnQuery.Click += BtnQuery_Click;
            btnClear.Click += BtnClear_Click;
            btnExport.Click += BtnExport_Click;
            btnSettings.Click += BtnSettings_Click;
            this.FormClosing += Form1_FormClosing;
        }

        private async void BtnQuery_Click(object sender, EventArgs e)
        {
            if (_isQuerying)
            {
                MessageBox.Show("正在查询中，请稍候...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var apiKeys = txtAddresses.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Select(key => key.Trim())
                .ToList();

            if (apiKeys.Count == 0)
            {
                MessageBox.Show("请输入至少一个API Key", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isQuerying = true;
            btnQuery.Enabled = false;
            btnQuery.Text = "查询中...";
            progressBar.Visible = true;
            progressBar.Maximum = apiKeys.Count;
            progressBar.Value = 0;

            try
            {
                await QueryBalancesAsync(apiKeys);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询过程中发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isQuerying = false;
                btnQuery.Enabled = true;
                btnQuery.Text = "开始查询";
                progressBar.Visible = false;
                statusLabel.Text = "查询完成";
            }
        }

        private async Task QueryBalancesAsync(List<string> apiKeys)
        {
            var results = new List<BalanceResult>();
            var completedCount = 0;

            foreach (var apiKey in apiKeys)
            {
                try
                {
                    statusLabel.Text = $"正在查询: {MaskApiKey(apiKey)}";
                    lblProgress.Text = $"{completedCount + 1}/{apiKeys.Count}";

                    var balance = await QuerySingleBalanceAsync(apiKey);
                    results.Add(new BalanceResult
                    {
                        Address = apiKey, // Keep original apiKey here for potential future use
                        Balance = balance,
                        Status = "成功",
                        QueryTime = DateTime.Now
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new BalanceResult
                    {
                        Address = apiKey,
                        Balance = "0",
                        Status = $"失败: {ex.Message}",
                        QueryTime = DateTime.Now
                    });
                }

                completedCount++;
                progressBar.Value = completedCount;
                Application.DoEvents();
            }

            DisplayResults(results);
        }

        private async Task<string> QuerySingleBalanceAsync(string apiKey)
        {
            try
            {
                return await _apiService.QueryBalanceAsync(apiKey);
            }
            catch (Exception ex)
            {
                throw new Exception($"查询地址 {apiKey} 失败: {ex.Message}");
            }
        }

        private void DisplayResults(List<BalanceResult> results)
        {
            var dt = new DataTable();
            dt.Columns.Add("API Key", typeof(string));
            dt.Columns.Add("余额", typeof(string));
            dt.Columns.Add("状态", typeof(string));
            dt.Columns.Add("查询时间", typeof(DateTime));

            foreach (var result in results)
            {
                dt.Rows.Add(MaskApiKey(result.Address), result.Balance, result.Status, result.QueryTime);
            }

            dataGridView.DataSource = dt;
            btnExport.Enabled = results.Count > 0;
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtAddresses.Clear();
            dataGridView.DataSource = null;
            btnExport.Enabled = false;
            statusLabel.Text = "已清空";
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dataGridView.DataSource == null)
            {
                MessageBox.Show("没有数据可导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV文件|*.csv|Excel文件|*.xlsx";
                saveFileDialog.Title = "导出查询结果";
                saveFileDialog.FileName = $"硅基流动Key余额查询结果_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExportToFile(saveFileDialog.FileName);
                        MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportToFile(string fileName)
        {
            if (fileName.EndsWith(".csv"))
            {
                ExportToCsv(fileName);
            }
            else if (fileName.EndsWith(".xlsx"))
            {
                ExportToExcel(fileName);
            }
        }

        private void ExportToCsv(string fileName)
        {
            var dt = (DataTable)dataGridView.DataSource;
            var csv = new StringBuilder();

            csv.AppendLine("API Key,余额,状态,查询时间");

            foreach (DataRow row in dt.Rows)
            {
                csv.AppendLine($"\"{row[0]}\",\"{row[1]}\",\"{row[2]}\",\"{row[3]}\"");
            }

            File.WriteAllText(fileName, csv.ToString(), Encoding.UTF8);
        }

        private void ExportToExcel(string fileName)
        {
            // 这里可以添加Excel导出功能
            // 需要引用Excel相关的NuGet包
            MessageBox.Show("Excel导出功能需要额外配置，暂时导出为CSV格式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ExportToCsv(fileName.Replace(".xlsx", ".csv"));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _apiService?.Dispose();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(_config))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // 重新创建API服务
                    _apiService?.Dispose();
                    _apiService = new ApiService(_config);
                    statusLabel.Text = "设置已更新";
                }
            }
        }

        private string MaskApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 8)
                return "****";
            
            return apiKey.Substring(0, 4) + "..." + apiKey.Substring(apiKey.Length - 4);
        }

        // 控件声明
        private TextBox txtAddresses;
        private Button btnQuery;
        private Button btnClear;
        private Button btnExport;
        private Button btnSettings;
        private ProgressBar progressBar;
        private Label lblProgress;
        private DataGridView dataGridView;
        private Label statusLabel;
    }

    public class BalanceResult
    {
        public string Address { get; set; } = string.Empty;
        public string Balance { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime QueryTime { get; set; }
    }
}