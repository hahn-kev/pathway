set src=..\..\..
if exist %src%\Test\PostTestBuild.bat goto anyCPU
set src=..\..\..\..
:anyCpu
copy %src%\LiftPrepare\Lib\PalasoLib\icu??40.dll . /d /y
rem xcopy ..\..\PublishingSolutionExeUi\*.xml . /d /y
xcopy %src%\ThirdParty\epubcheck-1.1\* .\epubcheck-1.1  /i /s /q /y
