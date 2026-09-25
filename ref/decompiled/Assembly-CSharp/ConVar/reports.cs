using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("reports")]
public class reports : ConsoleSystem
{
	[ServerVar(Default = "600")]
	[ClientVar(Default = "600", Help = "(Generated) Maximum character length of exception reports submitted to the crash reporter; clamped to a minimum of 250 characters")]
	public static int ExceptionReportMaxLength
	{
		get
		{
			return ExceptionReporter.ReportMessageMaxLength;
		}
		set
		{
			ExceptionReporter.ReportMessageMaxLength = Mathf.Max(value, 250);
		}
	}
}
