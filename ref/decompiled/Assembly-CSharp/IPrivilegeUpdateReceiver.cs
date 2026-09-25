using System.Collections.Generic;

public interface IPrivilegeUpdateReceiver
{
	void OnPrivilegeUpdated(BaseEntity privilegeEntity, HashSet<ulong> authorizedPlayers);
}
