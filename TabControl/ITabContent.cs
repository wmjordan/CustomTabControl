using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows.Forms
{
	public interface ITabContent
	{
		bool CanClose { get; }
	}
}
