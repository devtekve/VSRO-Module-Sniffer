# VSRO-Module-Sniffer

This project has as purpouse to sniff packets between AgentServer and Gameserver, but it is not limited to them.

## Installation

Installation it's easy, simply go ahead and change the settings at your will.

##INI Example

- [SETTINGS]
- IP=127.0.0.1					(Your server's IP address)
- GATEWAY=13580					(Real Farm manager Port) [Sorry for the name mismatch, it is late, it will be fixed soon]
- AGENT=45005						(You can safely ignore this)
- BINDIP=127.0.0.1				(Your server's IP address for binding [Pay attention to this setting])
- GWBIND=5001						(Fake Farm Manager port) [You MUST set this port in server.cfg section Gameserver]
- AGBIND=5002						(You can safely ignore this)
- REAL_MODULE_PORT = 15885		(This is the original Gameserver port you have in your packt.dat [Certification]) [This is the port in which Agent server will eventually connect, this will end up being the listening port for the sniffer]
- FAKE_NEW_MODULE_PORT = 14885	(This is the port in which Gameserver Will start listening, Sniffer will redirect connection to this port.)

## Usage

Start Certification port spoofer, then, execute all modules and finally execute the sniffer (SimpleTCPConnector)

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## Credits

- Goofie, because I am using your code as a base.
- PushEDX, because you taught me how to handle packets.
- Neks, Gave me SimpleTCPConnector base in a desperate need =D
- Florian0, Always supporting my learning, and heling closely to achieve my goals.

