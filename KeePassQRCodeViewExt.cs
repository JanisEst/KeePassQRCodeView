﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Utility;
using QRCoder;

namespace KeePassQRCodeView
{
	public class KeePassQRCodeViewExt : Plugin
	{
		private const string CONTEXT_MENU_ITEM_LABEL = "QR Code";

		private IPluginHost host;

		private ToolStripMenuItem menuItem;

		private DynamicMenu dynamicMenu;

		public override Image SmallIcon { get { return Properties.Resources.icon; } }

		public override string UpdateUrl { get { return "https://github.com/JanisEst/KeePassQRCodeView/raw/master/keepass.version"; } }

		public override bool Initialize(IPluginHost host)
		{
			Contract.Requires(host != null);

			this.host = host;

			menuItem = new ToolStripMenuItem
			{
				Image = Properties.Resources.icon,
				Text = CONTEXT_MENU_ITEM_LABEL
			};

			dynamicMenu = new DynamicMenu(menuItem.DropDownItems);
			dynamicMenu.MenuClick += OnShowQRCode;

			host.MainWindow.EntryContextMenu.Opening += OnEntryContextMenuOpening;

			return true;
		}

		public override void Terminate()
		{
			host.MainWindow.EntryContextMenu.Opening -= OnEntryContextMenuOpening;

			dynamicMenu.MenuClick -= OnShowQRCode;
		}

		public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
		{
			if (t == PluginMenuType.Entry)
			{
				return menuItem;
			}

			return null;
		}

		private void OnEntryContextMenuOpening(object sender, CancelEventArgs e)
		{
			dynamicMenu.Clear();

			if (!host.Database.IsOpen)
			{
				return;
			}

			if (host.MainWindow.GetSelectedEntriesCount() != 1)
			{
				return;
			}

			var pe = host.MainWindow.GetSelectedEntry(true);
			if (pe == null)
			{
				return;
			}

			var items = new List<Tuple<string, string>>();
			foreach (var kvp in pe.Strings)
			{
				if (kvp.Value.IsEmpty)
				{
					continue;
				}

				items.Add(Tuple.Create(
					StrUtil.EncodeMenuText(TryTranslateKey(kvp.Key)),
					kvp.Key
				));
			}

			foreach (var kv in items.OrderBy(t => t.Item1))
			{
				dynamicMenu.AddItem(
					kv.Item1,
					Properties.Resources.icon,
					kv.Item2
				);
			}
		}

		public void OnShowQRCode(object sender, DynamicMenuEventArgs e)
		{
			var key = e.Tag as string;
			if (key == null)
			{
				return;
			}

			var pe = host.MainWindow.GetSelectedEntry(true);
			if (pe == null)
			{
				return;
			}

			var context = new SprContext(pe, host.Database, SprCompileFlags.All);
			var value = SprEngine.Compile(pe.Strings.GetSafe(key).ReadString(), context);

			try
			{
				var data = new QRCodeGenerator().CreateQrCode(value, QRCodeGenerator.ECCLevel.L);
				if (data != null)
				{
					var form = new ShowQRCodeForm(
						host,
						data.GetBitmap(10, Color.Black, Color.White),
						SprEngine.Compile(pe.Strings.GetSafe(PwDefs.TitleField).ReadString(), context),
						TryTranslateKey(key)
					);
					form.ShowDialog();
				}
			}
			catch
			{
				MessageBox.Show("The data can't be displayed as a QR Code.");
			}
		}

		private static string TryTranslateKey(string key)
		{
			Contract.Requires(key != null);

			switch (key)
			{
				case PwDefs.TitleField:
					return KPRes.Title;
				case PwDefs.UserNameField:
					return KPRes.UserName;
				case PwDefs.PasswordField:
					return KPRes.Password;
				case PwDefs.UrlField:
					return KPRes.Url;
				case PwDefs.NotesField:
					return KPRes.Notes;
				default:
					return key;
			}
		}
	}
}
