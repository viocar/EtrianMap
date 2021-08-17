namespace EtrianMap
{
    partial class MapSelector
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ListBox_MapSelect = new System.Windows.Forms.ListBox();
            this.OpenButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ListBox_MapSelect
            // 
            this.ListBox_MapSelect.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListBox_MapSelect.FormattingEnabled = true;
            this.ListBox_MapSelect.Location = new System.Drawing.Point(12, 12);
            this.ListBox_MapSelect.Name = "ListBox_MapSelect";
            this.ListBox_MapSelect.Size = new System.Drawing.Size(90, 416);
            this.ListBox_MapSelect.TabIndex = 0;
            this.ListBox_MapSelect.DoubleClick += new System.EventHandler(this.ListBox_MapSelect_DoubleClick);
            this.ListBox_MapSelect.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListBox_MapSelect_KeyDown);
            // 
            // OpenButton
            // 
            this.OpenButton.Location = new System.Drawing.Point(12, 434);
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(90, 23);
            this.OpenButton.TabIndex = 1;
            this.OpenButton.Text = "Open";
            this.OpenButton.UseVisualStyleBackColor = true;
            this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // MapSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(114, 462);
            this.Controls.Add(this.OpenButton);
            this.Controls.Add(this.ListBox_MapSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MapSelector";
            this.Text = "Select Map";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox ListBox_MapSelect;
        private System.Windows.Forms.Button OpenButton;
    }
}