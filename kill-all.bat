REM al reejecutar test a veces puede ocurrir que usando chrome falle la compilacion desde linea de comandos
REM por haber archivos bloqueados. Tambien puede ocurrir por procesos de dotnet
REM Elimina todos los drivers y como todas las tareas dotnet
REM solo para windows y chrome, no ejecutar en CI
taskkill /im dotnet.exe /f
taskkill /im chromedriver.exe /f
taskkill /im chrome.exe /f
pause
