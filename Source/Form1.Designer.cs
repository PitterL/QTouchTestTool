namespace QTouch_UART_Tool
{
    partial class QTouch_UART_Tool
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.comboBox_com_port = new System.Windows.Forms.ComboBox();
            this.label_com_port = new System.Windows.Forms.Label();
            this.button_open = new System.Windows.Forms.Button();
            this.button_close = new System.Windows.Forms.Button();
            this.richTextBox_com = new System.Windows.Forms.RichTextBox();
            this.dataGridView_Test = new System.Windows.Forms.DataGridView();
            this.Index = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Reference = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Reference_Low = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Reference_High = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Delta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Delta_Low = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Delta_High = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Result = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button_test = new System.Windows.Forms.Button();
            //this.timer_Test = new System.Windows.Forms.Timer(this.components);
            this.button_stop = new System.Windows.Forms.Button();
            this.label_result = new System.Windows.Forms.Label();
            this.checkBox_reference = new System.Windows.Forms.CheckBox();
            this.checkBox_Delta = new System.Windows.Forms.CheckBox();
            this.textBox_Tool = new System.Windows.Forms.TextBox();
            this.label_tool = new System.Windows.Forms.Label();
            this.label_Interface = new System.Windows.Forms.Label();
            this.textBox_Interface = new System.Windows.Forms.TextBox();
            this.label_Device = new System.Windows.Forms.Label();
            this.textBox_Device = new System.Windows.Forms.TextBox();
            this.button_ProgramTestFW = new System.Windows.Forms.Button();
            this.button_ProgramMPFW = new System.Windows.Forms.Button();
            this.button_program_fw_and_bootloader = new System.Windows.Forms.Button();
            this.label_program_result = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Test)).BeginInit();
            this.SuspendLayout();
            // 
            // serialPort1
            // 
            this.serialPort1.BaudRate = 38400;
            // 
            // comboBox_com_port
            // 
            this.comboBox_com_port.FormattingEnabled = true;
            this.comboBox_com_port.Location = new System.Drawing.Point(101, 16);
            this.comboBox_com_port.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_com_port.Name = "comboBox_com_port";
            this.comboBox_com_port.Size = new System.Drawing.Size(160, 24);
            this.comboBox_com_port.TabIndex = 0;
            // 
            // label_com_port
            // 
            this.label_com_port.AutoSize = true;
            this.label_com_port.Location = new System.Drawing.Point(23, 20);
            this.label_com_port.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_com_port.Name = "label_com_port";
            this.label_com_port.Size = new System.Drawing.Size(73, 17);
            this.label_com_port.TabIndex = 1;
            this.label_com_port.Text = "COM_Port";
            // 
            // button_open
            // 
            this.button_open.Location = new System.Drawing.Point(25, 51);
            this.button_open.Margin = new System.Windows.Forms.Padding(4);
            this.button_open.Name = "button_open";
            this.button_open.Size = new System.Drawing.Size(100, 31);
            this.button_open.TabIndex = 2;
            this.button_open.Text = "Open";
            this.button_open.UseVisualStyleBackColor = true;
            this.button_open.Click += new System.EventHandler(this.button_open_Click);
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(163, 51);
            this.button_close.Margin = new System.Windows.Forms.Padding(4);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(100, 31);
            this.button_close.TabIndex = 3;
            this.button_close.Text = "Close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // richTextBox_com
            // 
            this.richTextBox_com.Location = new System.Drawing.Point(16, 140);
            this.richTextBox_com.Margin = new System.Windows.Forms.Padding(4);
            this.richTextBox_com.Name = "richTextBox_com";
            this.richTextBox_com.Size = new System.Drawing.Size(799, 88);
            this.richTextBox_com.TabIndex = 5;
            this.richTextBox_com.Text = "";
            // 
            // dataGridView_Test
            // 
            this.dataGridView_Test.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Test.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Index,
            this.Reference,
            this.Reference_Low,
            this.Reference_High,
            this.Delta,
            this.Delta_Low,
            this.Delta_High,
            this.Result});
            this.dataGridView_Test.Location = new System.Drawing.Point(16, 265);
            this.dataGridView_Test.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView_Test.Name = "dataGridView_Test";
            this.dataGridView_Test.RowTemplate.Height = 23;
            this.dataGridView_Test.Size = new System.Drawing.Size(800, 311);
            this.dataGridView_Test.TabIndex = 6;
            // 
            // Index
            // 
            this.Index.HeaderText = "Index";
            this.Index.Name = "Index";
            this.Index.Width = 70;
            // 
            // Reference
            // 
            this.Reference.HeaderText = "Ref";
            this.Reference.Name = "Reference";
            this.Reference.Width = 70;
            // 
            // Reference_Low
            // 
            this.Reference_Low.HeaderText = "Ref_Low";
            this.Reference_Low.Name = "Reference_Low";
            this.Reference_Low.Width = 70;
            // 
            // Reference_High
            // 
            this.Reference_High.HeaderText = "Ref_High";
            this.Reference_High.Name = "Reference_High";
            this.Reference_High.Width = 70;
            // 
            // Delta
            // 
            this.Delta.HeaderText = "Delta";
            this.Delta.Name = "Delta";
            this.Delta.Width = 70;
            // 
            // Delta_Low
            // 
            this.Delta_Low.HeaderText = "Delta_Low";
            this.Delta_Low.Name = "Delta_Low";
            this.Delta_Low.Width = 70;
            // 
            // Delta_High
            // 
            this.Delta_High.HeaderText = "Delta_High";
            this.Delta_High.Name = "Delta_High";
            this.Delta_High.Width = 70;
            // 
            // Result
            // 
            this.Result.HeaderText = "Result";
            this.Result.Name = "Result";
            this.Result.Width = 70;
            // 
            // button_test
            // 
            this.button_test.Location = new System.Drawing.Point(25, 89);
            this.button_test.Margin = new System.Windows.Forms.Padding(4);
            this.button_test.Name = "button_test";
            this.button_test.Size = new System.Drawing.Size(100, 31);
            this.button_test.TabIndex = 7;
            this.button_test.Text = "Test";
            this.button_test.UseVisualStyleBackColor = true;
            this.button_test.Click += new System.EventHandler(this.button_test_Click);
            // 
            // timer_Test
            // 
            //this.timer_Test.Interval = 10;
            //this.timer_Test.Tick += new System.EventHandler(this.timer_Test_Tick);
            // 
            // button_stop
            // 
            this.button_stop.Location = new System.Drawing.Point(163, 89);
            this.button_stop.Margin = new System.Windows.Forms.Padding(4);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(100, 31);
            this.button_stop.TabIndex = 8;
            this.button_stop.Text = "Stop";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // label_result
            // 
            this.label_result.BackColor = System.Drawing.Color.Gray;
            this.label_result.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_result.Location = new System.Drawing.Point(283, 89);
            this.label_result.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_result.Name = "label_result";
            this.label_result.Size = new System.Drawing.Size(250, 40);
            this.label_result.TabIndex = 9;
            this.label_result.Text = "Waiting For Test";
            this.label_result.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkBox_reference
            // 
            this.checkBox_reference.AutoSize = true;
            this.checkBox_reference.Checked = true;
            this.checkBox_reference.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_reference.Location = new System.Drawing.Point(16, 236);
            this.checkBox_reference.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_reference.Name = "checkBox_reference";
            this.checkBox_reference.Size = new System.Drawing.Size(139, 21);
            this.checkBox_reference.TabIndex = 10;
            this.checkBox_reference.Text = "Check Reference";
            this.checkBox_reference.UseVisualStyleBackColor = true;
            this.checkBox_reference.CheckedChanged += new System.EventHandler(this.checkBox_reference_CheckedChanged);
            // 
            // checkBox_Delta
            // 
            this.checkBox_Delta.AutoSize = true;
            this.checkBox_Delta.Checked = true;
            this.checkBox_Delta.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_Delta.Location = new System.Drawing.Point(181, 236);
            this.checkBox_Delta.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_Delta.Name = "checkBox_Delta";
            this.checkBox_Delta.Size = new System.Drawing.Size(106, 21);
            this.checkBox_Delta.TabIndex = 11;
            this.checkBox_Delta.Text = "Check Delta";
            this.checkBox_Delta.UseVisualStyleBackColor = true;
            this.checkBox_Delta.CheckedChanged += new System.EventHandler(this.checkBox_Delta_CheckedChanged);
            // 
            // textBox_Tool
            // 
            this.textBox_Tool.Location = new System.Drawing.Point(322, 20);
            this.textBox_Tool.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_Tool.Name = "textBox_Tool";
            this.textBox_Tool.Size = new System.Drawing.Size(100, 22);
            this.textBox_Tool.TabIndex = 12;
            this.textBox_Tool.Text = "atmelice";
            this.textBox_Tool.TextChanged += new System.EventHandler(this.textBox_Tool_TextChanged);
            // 
            // label_tool
            // 
            this.label_tool.AutoSize = true;
            this.label_tool.Location = new System.Drawing.Point(278, 20);
            this.label_tool.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_tool.Name = "label_tool";
            this.label_tool.Size = new System.Drawing.Size(36, 17);
            this.label_tool.TabIndex = 13;
            this.label_tool.Text = "Tool";
            // 
            // label_Interface
            // 
            this.label_Interface.AutoSize = true;
            this.label_Interface.Location = new System.Drawing.Point(648, 20);
            this.label_Interface.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Interface.Name = "label_Interface";
            this.label_Interface.Size = new System.Drawing.Size(63, 17);
            this.label_Interface.TabIndex = 14;
            this.label_Interface.Text = "Interface";
            // 
            // textBox_Interface
            // 
            this.textBox_Interface.Location = new System.Drawing.Point(719, 20);
            this.textBox_Interface.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_Interface.Name = "textBox_Interface";
            this.textBox_Interface.Size = new System.Drawing.Size(100, 22);
            this.textBox_Interface.TabIndex = 15;
            this.textBox_Interface.Text = "updi";
            this.textBox_Interface.TextChanged += new System.EventHandler(this.textBox_Interface_TextChanged);
            // 
            // label_Device
            // 
            this.label_Device.AutoSize = true;
            this.label_Device.Location = new System.Drawing.Point(446, 20);
            this.label_Device.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Device.Name = "label_Device";
            this.label_Device.Size = new System.Drawing.Size(51, 17);
            this.label_Device.TabIndex = 16;
            this.label_Device.Text = "Device";
            // 
            // textBox_Device
            // 
            this.textBox_Device.Location = new System.Drawing.Point(509, 20);
            this.textBox_Device.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_Device.Name = "textBox_Device";
            this.textBox_Device.Size = new System.Drawing.Size(100, 22);
            this.textBox_Device.TabIndex = 17;
            this.textBox_Device.Text = "attiny1616";
            this.textBox_Device.TextChanged += new System.EventHandler(this.textBox_Device_TextChanged);
            // 
            // button_ProgramTestFW
            // 
            this.button_ProgramTestFW.Location = new System.Drawing.Point(281, 50);
            this.button_ProgramTestFW.Margin = new System.Windows.Forms.Padding(4);
            this.button_ProgramTestFW.Name = "button_ProgramTestFW";
            this.button_ProgramTestFW.Size = new System.Drawing.Size(160, 31);
            this.button_ProgramTestFW.TabIndex = 18;
            this.button_ProgramTestFW.Text = "Program Test FW";
            this.button_ProgramTestFW.UseVisualStyleBackColor = true;
            this.button_ProgramTestFW.Click += new System.EventHandler(this.button_ProgramTestFW_Click);
            // 
            // button_ProgramMPFW
            // 
            this.button_ProgramMPFW.Location = new System.Drawing.Point(449, 50);
            this.button_ProgramMPFW.Margin = new System.Windows.Forms.Padding(4);
            this.button_ProgramMPFW.Name = "button_ProgramMPFW";
            this.button_ProgramMPFW.Size = new System.Drawing.Size(160, 31);
            this.button_ProgramMPFW.TabIndex = 19;
            this.button_ProgramMPFW.Text = "Program MP FW";
            this.button_ProgramMPFW.UseVisualStyleBackColor = true;
            this.button_ProgramMPFW.Click += new System.EventHandler(this.button_ProgramMPFW_Click);
            // 
            // button_program_fw_and_bootloader
            // 
            this.button_program_fw_and_bootloader.Location = new System.Drawing.Point(615, 50);
            this.button_program_fw_and_bootloader.Name = "button_program_fw_and_bootloader";
            this.button_program_fw_and_bootloader.Size = new System.Drawing.Size(200, 31);
            this.button_program_fw_and_bootloader.TabIndex = 21;
            this.button_program_fw_and_bootloader.Text = "Program Bootloader and FW";
            this.button_program_fw_and_bootloader.UseVisualStyleBackColor = true;
            this.button_program_fw_and_bootloader.Click += new System.EventHandler(this.button_program_fw_and_bootloader_Click);
            // 
            // label_program_result
            // 
            this.label_program_result.BackColor = System.Drawing.Color.Gray;
            this.label_program_result.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_program_result.Location = new System.Drawing.Point(565, 89);
            this.label_program_result.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_program_result.Name = "label_program_result";
            this.label_program_result.Size = new System.Drawing.Size(250, 40);
            this.label_program_result.TabIndex = 22;
            this.label_program_result.Text = "Waiting For Program";
            this.label_program_result.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // QTouch_UART_Tool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(832, 589);
            this.Controls.Add(this.label_program_result);
            this.Controls.Add(this.button_program_fw_and_bootloader);
            this.Controls.Add(this.button_ProgramMPFW);
            this.Controls.Add(this.button_ProgramTestFW);
            this.Controls.Add(this.textBox_Device);
            this.Controls.Add(this.label_Device);
            this.Controls.Add(this.textBox_Interface);
            this.Controls.Add(this.label_Interface);
            this.Controls.Add(this.label_tool);
            this.Controls.Add(this.textBox_Tool);
            this.Controls.Add(this.checkBox_Delta);
            this.Controls.Add(this.checkBox_reference);
            this.Controls.Add(this.label_result);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_test);
            this.Controls.Add(this.dataGridView_Test);
            this.Controls.Add(this.richTextBox_com);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.button_open);
            this.Controls.Add(this.label_com_port);
            this.Controls.Add(this.comboBox_com_port);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "QTouch_UART_Tool";
            this.Text = "QTouch_UART_Tool";
            this.Load += new System.EventHandler(this.QTouch_UART_Tool_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QTouch_UART_Tool_Close);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Test)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.ComboBox comboBox_com_port;
        internal System.Windows.Forms.Label label_com_port;
        private System.Windows.Forms.Button button_open;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.RichTextBox richTextBox_com;
        private System.Windows.Forms.DataGridView dataGridView_Test;
        private System.Windows.Forms.Button button_test;
        //private System.Windows.Forms.Timer timer_Test;
        private System.Windows.Forms.DataGridViewTextBoxColumn Index;
        private System.Windows.Forms.DataGridViewTextBoxColumn Reference;
        private System.Windows.Forms.DataGridViewTextBoxColumn Reference_Low;
        private System.Windows.Forms.DataGridViewTextBoxColumn Reference_High;
        private System.Windows.Forms.DataGridViewTextBoxColumn Delta;
        private System.Windows.Forms.DataGridViewTextBoxColumn Delta_Low;
        private System.Windows.Forms.DataGridViewTextBoxColumn Delta_High;
        private System.Windows.Forms.DataGridViewTextBoxColumn Result;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Label label_result;
        private System.Windows.Forms.CheckBox checkBox_reference;
        private System.Windows.Forms.CheckBox checkBox_Delta;
        private System.Windows.Forms.TextBox textBox_Tool;
        private System.Windows.Forms.Label label_tool;
        private System.Windows.Forms.Label label_Interface;
        private System.Windows.Forms.TextBox textBox_Interface;
        private System.Windows.Forms.Label label_Device;
        private System.Windows.Forms.TextBox textBox_Device;
        private System.Windows.Forms.Button button_ProgramTestFW;
        private System.Windows.Forms.Button button_ProgramMPFW;
        private System.Windows.Forms.Button button_program_fw_and_bootloader;
        private System.Windows.Forms.Label label_program_result;
    }
}

