//QRCoder is project by Raffael Herrmann and was first released in 10/2013. It's licensed under the MIT license.
//https://github.com/codebude/QRCoder

using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace QRCoder
{
	public class QRCodeData
	{
		public int Version { get; private set; }
		public List<BitArray> ModuleMatrix { get; set; }

		public QRCodeData(int version)
		{
			Version = version;

			var size = ModulesPerSideFromVersion(version);
			ModuleMatrix = new List<BitArray>();
			for (int i = 0; i < size; i++)
			{
				ModuleMatrix.Add(new BitArray(size));
			}
		}

		private int ModulesPerSideFromVersion(int version)
		{
			return 21 + (version - 1) * 4;
		}

		public Bitmap GetBitmap(int pixelsPerModule, Color darkColor, Color lightColor)
		{
			var size = ModuleMatrix.Count * pixelsPerModule;

			var bmp = new Bitmap(size, size);
			using (var g = Graphics.FromImage(bmp))
			{
				for (int x = 0; x < size; x = x + pixelsPerModule)
				{
					for (int y = 0; y < size; y = y + pixelsPerModule)
					{
						var module = ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1];
						if (module)
						{
							g.FillRectangle(new SolidBrush(darkColor), new Rectangle(x, y, pixelsPerModule, pixelsPerModule));
						}
						else
						{
							g.FillRectangle(new SolidBrush(lightColor), new Rectangle(x, y, pixelsPerModule, pixelsPerModule));
						}
					}
				}
			}

			return bmp;
		}
	}
}
