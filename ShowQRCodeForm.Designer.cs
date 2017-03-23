namespace KeePassQRCodeView
{
	partial class ShowQRCodeForm
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
			this.printButton = new System.Windows.Forms.Button();
			this.saveButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// printButton
			// 
			this.printButton.Location = new System.Drawing.Point(0, 0);
			this.printButton.Name = "printButton";
			this.printButton.Size = new System.Drawing.Size(25, 25);
			this.printButton.TabIndex = 0;
			this.printButton.UseVisualStyleBackColor = true;
			this.printButton.Click += new System.EventHandler(this.printButton_Click);
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(25, 0);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(25, 25);
			this.saveButton.TabIndex = 1;
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// ShowQRCodeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.printButton);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ShowQRCodeForm";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Click += new System.EventHandler(this.ShowQRCodeForm_Click);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button printButton;
		private System.Windows.Forms.Button saveButton;
	}
}