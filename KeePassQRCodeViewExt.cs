using System;
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
		private const string INSERT_AFTER_ENTRY_KEY = "m_ctxEntrySaveAttachedFiles";

		private IPluginHost host = null;

		private ToolStripMenuItem ctxEntryShowQRCode;
		private DynamicMenu dynQRCodes;

		public override Image SmallIcon
		{
			get { return Properties.Resources.icon; }
		}

		public override string UpdateUrl
		{
			get { return "https://github.com/KN4CK3R/KeePassQRCodeView/raw/master/keepass.version"; }
		}

		public override bool Initialize(IPluginHost host)
		{
			Contract.Requires(host != null);

			this.host = host;

			ctxEntryShowQRCode = new ToolStripMenuItem
			{
				Image = Properties.Resources.icon,
				Text = CONTEXT_MENU_ITEM_LABEL
			};
			dynQRCodes = new DynamicMenu(ctxEntryShowQRCode.DropDownItems);
			dynQRCodes.MenuClick += OnShowQRCode;

			var insertAfterIndex = host.MainWindow.EntryContextMenu.Items.IndexOfKey(INSERT_AFTER_ENTRY_KEY);
			if (insertAfterIndex != -1)
			{
				//insert after "Save Attachements"
				host.MainWindow.EntryContextMenu.Items.Insert(insertAfterIndex + 1, ctxEntryShowQRCode);
			}
			else
			{
				//add at the end
				host.MainWindow.EntryContextMenu.Items.Add(ctxEntryShowQRCode);
			}
			host.MainWindow.EntryContextMenu.Opening += OnEntryContextMenuOpening;

			return true;
		}

		public override void Terminate()
		{
			host.MainWindow.EntryContextMenu.Opening -= OnEntryContextMenuOpening;
			host.MainWindow.EntryContextMenu.Items.Remove(ctxEntryShowQRCode);

			dynQRCodes.MenuClick -= OnShowQRCode;
		}

		private void OnEntryContextMenuOpening(object sender, CancelEventArgs e)
		{
			ctxEntryShowQRCode.Visible = false;

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

			dynQRCodes.Clear();

			var items = new List<Tuple<string, string>>();
			foreach (var kvp in pe.Strings)
			{
				if (kvp.Value.IsEmpty)
				{
					continue;
				}

				items.Add(Tuple.Create(
					StrUtil.EncodeMenuText(TryTranslate(kvp.Key)),
					kvp.Key
				));
			}

			foreach (var kv in items.OrderBy(t => t.Item1))
			{
				dynQRCodes.AddItem(
					kv.Item1,
					Properties.Resources.icon,
					kv.Item2
				);
			}

			ctxEntryShowQRCode.Visible = true;
		}

		private string TryTranslate(string key)
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

			var value = pe.Strings.GetSafe(key).ReadString();

			char scan, wanted;
			pe = SprEngine.FindRefTarget(
				value,
				new SprContext(pe, host.Database, SprCompileFlags.All),
				out scan,
				out wanted
			);
			if (null != pe)
			{
				value = pe.Strings.GetSafe(key).ReadString();
			}

			var data = new QRCodeGenerator().CreateQrCode(value, QRCodeGenerator.ECCLevel.L);
			if (data != null)
			{
				new ShowQRCodeForm(data.GetBitmap(10, Color.Black, Color.White)).ShowDialog();
			}
		}
	}
}
