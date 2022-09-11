using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KeePass.Plugins;
using KeePassLib.Utility;

namespace KeePassQRCodeView
{
	public partial class ShowQRCodeForm : Form
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		private const int ScreenPadding = 100;

		private readonly Bitmap qrcode;
		private readonly string title;
		private readonly string field;

		public ShowQRCodeForm(IPluginHost host, Bitmap qrcode, string title, string field)
		{
			Contract.Requires(qrcode != null);
			Contract.Requires(title != null);
			Contract.Requires(field != null);

			InitializeComponent();

			this.qrcode = qrcode;
			this.title = title.Trim();
			this.field = field.Trim();

			DoubleBuffered = true;

			Text = JoinIfNotEmpty(this.title, this.field);

			var screenBounds = Screen.GetBounds(this);
			if (screenBounds.Width <= ScreenPadding || screenBounds.Height <= ScreenPadding)
			{
				screenBounds = new Rectangle(0, 0, 800, 800);
			}
			var maxSize = Math.Min(Math.Min(screenBounds.Width - ScreenPadding, screenBounds.Height - ScreenPadding), Math.Min(qrcode.Width, qrcode.Height));

			ClientSize = new Size(maxSize, maxSize);
			BackgroundImageLayout = ImageLayout.Stretch;
			BackgroundImage = qrcode;

			printToolStripMenuItem.Image = host.Resources.GetObject("B16x16_FilePrint") as Image;
			saveAsToolStripMenuItem.Image = host.Resources.GetObject("B16x16_FileSave") as Image;
		}

		private void ShowQRCodeForm_Click(object sender, EventArgs e)
		{
			var args = e as MouseEventArgs;
			if (args == null || args.Button != MouseButtons.Left)
			{
				return;
			}

			Close();
		}

		private void printToolStripMenuItem_Click(object sender, EventArgs e)
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

			using (var print = new PrintDialog
			{
				Document = document
			})
			{
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
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var sfd = new SaveFileDialog
			{
				Filter = "Images|*.jpg;*.png;*.gif;*.tiff;*.bmp",
				DefaultExt = "jpg"
			})
			{
				if (sfd.ShowDialog() == DialogResult.OK)
				{
					var format = ImageFormat.Jpeg;
					switch (Path.GetExtension(sfd.FileName).ToLower())
					{
						case ".png": format = ImageFormat.Png; break;
						case ".gif": format = ImageFormat.Gif; break;
						case ".tiff": format = ImageFormat.Tiff; break;
						case ".bmp": format = ImageFormat.Bmp; break;
					}
					try
					{
						BackgroundImage.Save(sfd.FileName, format);
					}
					catch (Exception ex)
					{
						MessageService.ShowWarning(ex);
					}
				}
			}
		}

		protected override void WndProc(ref Message m)
		{
			const int WMSZ_LEFT = 1;
			const int WMSZ_RIGHT = 2;
			const int WMSZ_TOP = 3;
			const int WMSZ_TOPLEFT = 4;
			const int WMSZ_TOPRIGHT = 5;
			const int WMSZ_BOTTOM = 6;
			const int WMSZ_BOTTOMLEFT = 7;
			const int WMSZ_BOTTOMRIGHT = 8;

			if (m.Msg == 0x214 /*WM_MOVING || WM_SIZING*/)
			{
				var rc = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
				var w = rc.Right - rc.Left;
				var h = rc.Bottom - rc.Top;
				var z = w > h ? w : h;

				switch ((int)m.WParam)
				{
					case WMSZ_LEFT:
						rc.Bottom = rc.Top + w;
						rc.Top = rc.Top + (h - (rc.Bottom - rc.Top)) / 2;
						break;
					case WMSZ_RIGHT:
						rc.Bottom = rc.Top + w;
						rc.Top = rc.Top + (h - (rc.Bottom - rc.Top)) / 2;
						break;
					case WMSZ_TOP:
						rc.Right = rc.Left + h;
						rc.Left = rc.Left + (w - (rc.Right - rc.Left)) / 2;
						break;
					case WMSZ_TOPLEFT:
						rc.Top = rc.Bottom - z;
						rc.Left = rc.Right - z;
						break;
					case WMSZ_TOPRIGHT:
						rc.Top = rc.Bottom - z;
						rc.Right = rc.Left + z;
						break;
					case WMSZ_BOTTOM:
						rc.Right = rc.Left + h;
						rc.Left = rc.Left + (w - (rc.Right - rc.Left)) / 2;
						break;
					case WMSZ_BOTTOMLEFT:
						rc.Bottom = rc.Top + z;
						rc.Left = rc.Right - z;
						break;
					case WMSZ_BOTTOMRIGHT:
						rc.Bottom = rc.Top + z;
						rc.Right = rc.Left + z;
						break;
				}

				Marshal.StructureToPtr(rc, m.LParam, false);
				m.Result = (IntPtr)1;

				return;
			}

			base.WndProc(ref m);
		}

		private static string JoinIfNotEmpty(params string[] param)
		{
			return string.Join(" - ", param.Where(s => !string.IsNullOrEmpty(s)));
		}
	}
}
