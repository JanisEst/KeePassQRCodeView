using KeePass.Plugins;
using KeePassLib.Utility;
using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace KeePassQRCodeView
{
	public partial class ShowQRCodeForm : Form
	{
		private Bitmap qrcode;
		private string title;
		private string field;

		public ShowQRCodeForm(IPluginHost host, Bitmap qrcode, string title, string field)
		{
			Contract.Requires(qrcode != null);
			Contract.Requires(title != null);
			Contract.Requires(field != null);

			InitializeComponent();

			this.qrcode = qrcode;
			this.title = title.Trim();
			this.field = field.Trim();

			Text = JoinIfNotEmpty(this.title, this.field);

			ClientSize = new Size(qrcode.Width, qrcode.Height);
			BackgroundImage = qrcode;

			printButton.Image = host.Resources.GetObject("B16x16_FilePrint") as Image;
		}

		private void ShowQRCodeForm_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void printButton_Click(object sender, EventArgs e)
		{
			var document = new PrintDocument();
			document.PrintPage += (s, page) =>
			{
				var lineHeight = (int)Math.Ceiling(SystemFonts.DefaultFont.GetHeight(page.Graphics));
				var descriptionBounds = new Rectangle(
					page.MarginBounds.X,
					page.MarginBounds.Y,
					page.MarginBounds.Width,
					lineHeight
				);

				page.Graphics.DrawString(
					JoinIfNotEmpty(title, field),
					SystemFonts.DefaultFont,
					Brushes.Black,
					descriptionBounds,
					new StringFormat
					{
						Alignment = StringAlignment.Center,
						LineAlignment = StringAlignment.Center
					}
				);

				var imageBounds = new Rectangle(
					page.MarginBounds.X,
					page.MarginBounds.Y + lineHeight,
					page.MarginBounds.Width,
					page.MarginBounds.Height - lineHeight
				);
				if (qrcode.Width < imageBounds.Width && qrcode.Height < imageBounds.Height)
				{
					imageBounds = new Rectangle(
						imageBounds.X + (int)(imageBounds.Width * 0.5f - qrcode.Width * 0.5f),
						imageBounds.Y,
						qrcode.Width,
						qrcode.Height
					);
				}
				else
				{
					var scale = Math.Min(
						imageBounds.Width / (float)qrcode.Width,
						imageBounds.Height / (float)qrcode.Height
					);

					imageBounds = new Rectangle(
						imageBounds.X + (int)(imageBounds.Width * 0.5f - (qrcode.Width * scale) * 0.5f),
						imageBounds.Y,
						(int)(qrcode.Width * scale),
						(int)(qrcode.Height * scale)
					);
				}

				page.Graphics.DrawImage(qrcode, imageBounds);
			};

			var print = new PrintDialog();
			print.Document = document;

			if (print.ShowDialog() == DialogResult.OK)
			{
				try
				{
					document.Print();
				}
				catch (Exception ex)
				{
					MessageService.ShowWarning(ex);
				}
			}
		}

		private string JoinIfNotEmpty(params string[] param)
		{
			return string.Join(" - ", param.Where(s => !string.IsNullOrEmpty(s)));
		}
	}
}
