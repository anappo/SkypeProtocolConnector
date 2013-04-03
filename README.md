SkypeProtocolConnector
======================

Skype Desktop (Public API) connector lib. .NET / C#


The SkypeConnector class exposes simple access to the Skype Public API (Skype aka Desktop API). For more information on Skype Public API http://dev.skype.com/accessories

<h5>Establishing connection to Skype desktop client</h5>
Attempt to establish connection is made automatically at SkypeConnector instantiation. Sending and receiving messages becomes possible after OnAttach event fires with state == success.

<h5>Sending commands</h5>
Once connection is established, you can use SendCommand method to transmit Public API protocol commands. For more information see http://dev.skype.com/desktop-api-reference

<h5>Receiving messages</h5>
Feedback messages from Skype can be had via OnReceive event. 

<b>NB!</b> The Public API transport goes over Windows messaging system. Thus a valid window handle and a WndProc are required for this lib to work. The SkypeConnector constructor takes a Form argument (presumably the main form but any will do) and inserts a hidden control into it, which provides hwind and WndProc for the transport. A side effect of this is dependancy on Windows.Forms, but a making a WPF version should not be terribly difficult.
