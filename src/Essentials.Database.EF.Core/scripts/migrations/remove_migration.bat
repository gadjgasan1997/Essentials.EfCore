echo off
chcp 65001

if not defined configuration (
	echo Не указана конфигурация для сборки. Будет использована конфигурация по-умолчанию [Debug]
	set configuration=Debug
)

if not defined project (
	echo Для удаления миграции БД укажите название проекта [Переменная project]
	goto exit
)

if not defined startup_project (
	echo Для удаления миграции БД укажите название проекта [Переменная startup_project]
	goto exit
)

if not defined context (
	echo Для удаления миграции БД укажите название контекста [Переменная context]
	goto exit
)

if not defined connection_string (
	echo Для удаления миграции БД укажите строку подключения [Переменная connection_string]
	goto exit
)

set MigrationsConnectionString='%connection_string%'

echo Происходит удаление миграции
echo Используемая строка подключения: '%connection_string%'

"C:\Program Files\dotnet\dotnet.exe" ef migrations remove ^
--project %project% ^
--startup-project %startup_project% ^
--context %context% ^
--configuration %configuration% ^
--verbose ^
--force

:exit