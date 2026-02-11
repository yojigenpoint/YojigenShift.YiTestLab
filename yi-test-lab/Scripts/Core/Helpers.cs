using Godot;

namespace YojigenShift.YiTestLab.Core
{
	public static class Helpers
	{
		public static string GetLocalizedFormat(string key, params object[] args)
		{
			string translated = TranslationServer.Translate(key).ToString();
			if (args != null && args.Length > 0)
			{
				try { return string.Format(translated, args); }
				catch { return translated; }
			}
			return translated;
		}
	}
}
