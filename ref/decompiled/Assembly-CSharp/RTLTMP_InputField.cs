using RTLTMPro;
using TMPro;

public class RTLTMP_InputField : TMP_InputField
{
	private static readonly FastStringBuilder inputBuilder = new FastStringBuilder(2048);

	public bool changeAlignment = true;
}
