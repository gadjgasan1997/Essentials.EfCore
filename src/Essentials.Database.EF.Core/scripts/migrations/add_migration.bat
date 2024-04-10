echo off
chcp 65001

if not defined migrations_dir (
	echo Не указана директория для сохранения миграций. Будет использована директория по-умолчанию [Migrations]
	set migrations_dir=Migrations	
)

if not defined configuration (
	echo Не указана конфигурация для сборки. Будет использована конфигурация по-умолчанию [Debug]
	set configuration=Debug
)

if not defined project (
	echo Для применения миграции к БД укажите название проекта [Переменная project]
	goto exit
)

if not defined startup_project (
	echo Для применения миграции к БД укажите название проекта [Переменная startup_project]
	goto exit
)

if not defined context (
	echo Для применения миграции к БД укажите название контекста [Переменная context]
	goto exit
)

if not defined migration_name (
	echo Для применения миграции к БД укажите название миграции [Переменная migration_name]
	goto exit
)

if not defined connection_string (
	echo Для применения миграции к БД укажите строку подключения [Переменная connection_string]
	goto exit
)

set MigrationsConnectionString='%connection_string%'

echo Применяется миграция '%migration_name%' на контексте '%context%'
echo Используемая строка подключения: '%connection_string%'

"C:\Program Files\dotnet\dotnet.exe" ef migrations add ^
--project %project% ^
--startup-project %startup_project% ^
--context %context% ^
--configuration %configuration% ^
--output-dir %migrations_dir% ^
--verbose ^
%migration_name%

:exit