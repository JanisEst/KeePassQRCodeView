using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassQRCodeView
{
	public partial class ShowQRCodeForm : Form
	{
		public ShowQRCodeForm(Bitmap qrcode)
		{
			Contract.Requires(qrcode != null);

			InitializeComponent();

			Width = qrcode.Width;
			Height = qrcode.Height;
			BackgroundImage = qrcode;
		}

		private void ShowQRCodeForm_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
