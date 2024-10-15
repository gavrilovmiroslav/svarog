@echo %1
@echo %2
@echo %3

xcopy /c /q /y %2\%1 %3\Plugins\

if exist %2\Data\ (
  xcopy /c /q /y /s /e %2\Data\* %3\
)