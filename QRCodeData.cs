//QRCoder is project by Raffael Herrmann and was first released in 10/2013. It's licensed under the MIT license.
//https://github.com/codebude/QRCoder

using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace QRCoder
{
	public class QRCodeData
	{
		public List<BitArray> ModuleMatrix { get; set; }

		public QRCodeData(int version)
		{
			var size = ModulesPerSideFromVersion(version);
			ModuleMatrix = new List<BitArray>();
			for (var i = 0; i < size; i++)
			{
				ModuleMatrix.Add(new BitArray(size));
			}
		}

		private static int ModulesPerSideFromVersion(int version)
		{
			return 21 + (version - 1) * 4;
		}

		public Bitmap GetBitmap(int pixelsPerModule, Color darkColor, Color lightColor)
		{
			var size = ModuleMatrix.Count * pixelsPerModule;

			var bmp = new Bitmap(size, size);
			using (var g = Graphics.FromImage(bmp))
			{
				using (var darkBrush = new SolidBrush(darkColor))
				{
					using (var lightBrush = new SolidBrush(lightColor))
					{
						for (var x = 0; x < size; x += pixelsPerModule)
						{
							for (var y = 0; y < size; y += pixelsPerModule)
							{
								var module = ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1];
								var brush = module ? darkBrush : lightBrush;
								g.FillRectangle(brush, new Rectangle(x, y, pixelsPerModule, pixelsPerModule));
							}
						}
					}
				}
			}

			return bmp;
		}
	}
}
