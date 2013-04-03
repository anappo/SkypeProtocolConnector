SkypeProtocolConnector
======================

Skype Desktop (Public API) connector lib. .NET / C#


The SkypeConnector class exposes simple access to the Skype Public API (Skype aka Desktop API). For more information on Skype Public API http://dev.skype.com/accessories

1. Establishing connection to Skype desktop client
Attempt to establish connection is made automatically at SkypeConnector instantiation. Sending and receiving messages becomes possible after OnAttach event fires with state == success.

2. Sending commands
Once connection is established, you can use SendCommand method to transmit Public API protocol commands. For more information see http://dev.skype.com/desktop-api-reference

3. Receiving messages
Feedback messages from Skype can be had via OnReceive event. This file can be copied into an empty class library project and compiled as dll. If you paste it directly into your project, you will need to modify the namespace. NB! The Public API transport goes over Windows messaging system. Thus a valid window handle and a WndProc are required for this lib to work. The SkypeConnector constructor takes a Form argument (presumably the main form but any will do) and inserts a hidden control into it, which provides hwind and WndProc for the transport. A side effect of this is dependancy on Windows.Forms, but a making a WPF version should not be terribly difficult.
