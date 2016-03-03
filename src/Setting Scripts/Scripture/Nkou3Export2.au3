﻿;-----------------------------------------------------------------------------
; Name:        Nkou3Export.au3
; Purpose:     Export scripture from NKOu3 project
;              (Edited script created by AutoItRecorder.)
;
; Author:      <greg_trihus@sil.org>
;
; Created:     2013/10/18
; Copyright:   (c) 2013 SIL International
; Licence:     <MIT>
;-----------------------------------------------------------------------------
Opt("WinWaitDelay",100)
Opt("WinDetectHiddenText",1)
Opt("MouseCoordMode",0)
$TitleMatchStart = 1
Opt("WinTitleMatchMode", $TitleMatchStart) 

Run("C:\\Program Files (x86)\\Paratext 7\\Paratext.exe")
_WinWaitActivate("Paratext","")
MouseClick("left",35,47,1)
MouseClick("left",78,86,1)
_WinWaitActivate("Open Project/Resource","")
MouseClick("left",120,126,1)
Send("nnnn{ENTER}")
_WinWaitActivate("Paratext 7.4","")
Send("{CTRLDOWN}b{CTRLUP}mat{ENTER}{CTRLDOWN}b{CTRLUP}{TAB}21{ENTER}")
MouseClick("left",33,49,1)
MouseClick("left",124,491,1)
_WinWaitActivate("Export Book(s) to Pathway","")
MouseClick("left",303,210,1)
_WinWaitActivate("Select Your Organization","")
MouseClick("left",201,167,1)
_WinWaitActivate("Export Through Pathway","")
MouseClick("left",157,105,1)
MouseClick("left",158,182,1)
Send("{SHIFTDOWN}m{SHIFTUP}y{SPACE}{SHIFTDOWN}t{SHIFTUP}itle")
MouseClick("left",147,131,1)
MouseClick("left",98,186,1)
MouseClick("left",138,403,1)
MouseClick("left",198,464,1)
_WinWaitActivate("LibreOffice - Security Warning","")
MouseClick("left",161,185,1)
Opt("WinTitleMatchMode", 2) 
_WinWaitActivate("LibreOffice Writer","")
MouseClick("left",1913,13,1)
_WinWaitActivate("Paratext","")
MouseMove(39,46)
MouseDown("left")
MouseMove(38,46)
MouseUp("left")
MouseClick("left",118,512,1)

#region --- Internal functions Au3Recorder Start ---
Func _WinWaitActivate($title,$text,$timeout=0)
	WinWait($title,$text,$timeout)
	If Not WinActive($title,$text) Then WinActivate($title,$text)
	WinWaitActive($title,$text,$timeout)
EndFunc
#endregion --- Internal functions Au3Recorder End ---
