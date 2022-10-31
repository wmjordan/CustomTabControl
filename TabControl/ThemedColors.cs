/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
*/

using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace System.Drawing
{
	internal static class ThemedColors
	{
		#region "    Variables and Constants "

		const string NormalColor = "NormalColor";
		const string HomeStead = "HomeStead";
		const string Metallic = "Metallic";
		const string NoTheme = "NoTheme";

		static readonly Color[] _toolBorder = new Color[] {Color.FromArgb(127, 157, 185), Color.FromArgb(164, 185, 127), Color.FromArgb(165, 172, 178), Color.FromArgb(132, 130, 132)};
		#endregion

		#region "    Properties "

		public static ColorScheme CurrentThemeIndex => GetCurrentThemeIndex();

		public static Color ToolBorder => _toolBorder[(int)CurrentThemeIndex];

		#endregion

		static ColorScheme GetCurrentThemeIndex()
		{
			if (VisualStyleInformation.IsSupportedByOS && VisualStyleInformation.IsEnabledByUser && Application.RenderWithVisualStyles)
			{
				switch (VisualStyleInformation.ColorScheme) {
					case NormalColor:
						return ColorScheme.NormalColor;
					case HomeStead:
						return ColorScheme.HomeStead;
					case Metallic:
						return ColorScheme.Metallic;
					default:
						return ColorScheme.NoTheme;
				}
			}
			return ColorScheme.NoTheme;
		}

		public enum ColorScheme
		{
			NormalColor = 0,
			HomeStead = 1,
			Metallic = 2,
			NoTheme = 3
		}

	}

}
