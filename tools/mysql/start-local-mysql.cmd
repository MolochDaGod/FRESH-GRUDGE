@echo off
setlocal

set CONTAINER_NAME=grudge-mysql
set MYSQL_IMAGE=mysql:8.0

docker info >nul 2>&1
if errorlevel 1 (
    echo Docker daemon is not available. Start Docker Desktop and retry.
    exit /b 1
)

docker ps -a --format "{{.Names}}" | findstr /R /C:"^%CONTAINER_NAME%$" >nul
if not errorlevel 1 (
    echo Starting existing %CONTAINER_NAME% container...
    docker start %CONTAINER_NAME%
    exit /b %errorlevel%
)

echo Creating new %CONTAINER_NAME% container on 127.0.0.1:3306...
docker run -d ^
  --name %CONTAINER_NAME% ^
  -e MYSQL_ROOT_PASSWORD=07a960772947282439a05d4351d9e111d322e737 ^
  -e MYSQL_DATABASE=grudge_game ^
  -e MYSQL_USER=grudge_admin ^
  -e MYSQL_PASSWORD=92120a0586492a3c03739c826692bdba ^
  -p 3306:3306 ^
  %MYSQL_IMAGE% ^
  --default-authentication-plugin=mysql_native_password

exit /b %errorlevel%