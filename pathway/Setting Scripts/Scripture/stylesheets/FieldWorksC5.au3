﻿#region ---Au3Recorder generated code Start ---
Opt("WinWaitDelay",100)
Opt("WinDetectHiddenText",1)
Opt("MouseCoordMode",0)

_WinWaitActivate("Pathway7","")
MouseClick("left",308,425,1)
MouseClick("left",308,425,1)
_WinWaitActivate("Pathway Configuration Tool - BTE 1.4.0.3397","")
MouseClick("left",338,56,1)
_WinWaitActivate("Select Your Organization - Dictionary","")
MouseClick("left",190,162,1)
_WinWaitActivate("Set Defaults - Dictionary","")
MouseClick("left",204,78,1)
MouseClick("left",201,99,1)
MouseClick("left",159,97,1)
MouseClick("left",159,178,1)
Send("{SHIFTDOWN}m{SHIFTUP}y{SPACE}{SHIFTDOWN}t{SHIFTUP}t{BACKSPACE}itle")
MouseClick("left",159,131,1)
MouseClick("left",75,182,1)
MouseClick("left",233,134,1)
MouseClick("left",187,257,1)
MouseClick("left",153,403,1)
MouseClick("left",188,472,1)
_WinWaitActivate("Pathway Configuration Tool - BTE 1.4.0.3397","")
MouseClick("left",1144,4,1)

#region --- Internal functions Au3Recorder Start ---
Func _WinWaitActivate($title,$text,$timeout=0)
	WinWait($title,$text,$timeout)
	If Not WinActive($title,$text) Then WinActivate($title,$text)
	WinWaitActive($title,$text,$timeout)
EndFunc
#endregion --- Internal functions Au3Recorder End ---

#endregion --- Au3Recorder generated code End ---
