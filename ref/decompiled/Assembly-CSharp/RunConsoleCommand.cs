using UnityEngine;

public class RunConsoleCommand : MonoBehaviour
{
	[SerializeField]
	private bool runQuiet;

	[UnityEvent]
	public void ClientRun(string command)
	{
		ConsoleSystem.Run(runQuiet ? ConsoleSystem.Option.Client.Quiet() : ConsoleSystem.Option.Client, command);
	}
}
