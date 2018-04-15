# server
C# server for the game that is detached from Unity.

## Prerequisites
1. Install Mono by running the command `sudo dnf install mono-devel`.
	Replace dnf with the package manager of your choice.
2. Disable the firewall by running these commands
	```
	iptables -F
	iptables -X
	```

To run a development version of the server run `sudo ./run-server.sh`.
To build a release version of the server run `sudo ./deploy-server.sh`.

Root is needed for the development version because the networking library needs to be copied into /usr/lib.
On the other hand, the networking library must remain in the same directory as the executable when deployed.

