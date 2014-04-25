echo %PATH%
python binz.py Release
copy bin.zip %CD%\installer\release
pause